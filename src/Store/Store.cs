#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;

namespace Playdux.src.Store
{
    /// <summary>
    /// A Playdux state container. The core of Playdux.
    /// Includes capability to dispatch actions to the store, get the current state, get the current state narrowed by
    /// a selector, and get an IObservable to the "selected" state.
    /// </summary>
    public sealed class Store<TRootState> : IDisposable, IStore<TRootState> where TRootState : class
    {
        /// The current state within the store.
        public TRootState State => stateStream.Value;

        /// Ensures that incoming actions maintain order and acts as a waiting line if multiple actions come in quick succession.
        /// Additionally, this handles reentrancy when an action is dispatched from within the actionStream, for instance with SideEffectors that emit new actions.
        private readonly BlockingCollection<DispatchedAction> actionQueue = new();

        /// A stream of the current state within the store.
        /// This stream is safely shared to consumers (via ObservableFor) in such a way that consumer cancellation and errors are isolated from the main stream.
        private readonly BehaviorSubject<TRootState> stateStream;

        /// Used to dispose the actionStream when this Store is done being used, to mitigate memory leaks.
        private readonly IDisposable actionStreamHandle;

        /// Stores SideEffectors in such a way that they can be unregistered later if necessary.
        private readonly Dictionary<Guid, ISideEffector<TRootState>> sideEffectorCollection = new();

        /// Stores SideEffectors in priority order.
        private readonly List<ISideEffector<TRootState>> sideEffectorsByPriority = new();

        /// Used to cancel the background task on which the actionQueue is consumed.
        private readonly CancellationTokenSource actionQueueCancellationTokenSource = new();

        /// Create a new store with a given initial state and reducer
        public Store(TRootState initialState, Func<TRootState, IAction, TRootState> rootReducer, IEnumerable<ISideEffector<TRootState>>? initialSideEffectors = null)
        {
            if (initialSideEffectors is not null)
            {
                foreach (var sideEffector in initialSideEffectors) RegisterSideEffector(sideEffector);
            }

            var actionStream = new BehaviorSubject<DispatchedAction>(new DispatchedAction(new InitializeAction<TRootState>(initialState)));
            stateStream = new BehaviorSubject<TRootState>(initialState);
            actionStreamHandle = actionStream
                .SelectMany(dispatchedAction =>
                {
                    foreach (var sideEffector in sideEffectorsByPriority)
                    {
                        var shouldAllow = sideEffector.PreEffect(dispatchedAction, this);
                        if (!shouldAllow) dispatchedAction = dispatchedAction with { IsCanceled = true };
                    }

                    return dispatchedAction.IsCanceled ? Observable.Never<DispatchedAction>() : Observable.Return(dispatchedAction);
                })
                .Select(dispatchedAction =>
                {
                    var (action, _, _, _) = dispatchedAction;
                    var state = stateStream.Value;
                    if (action is InitializeAction<TRootState> castAction) state = castAction.InitialState;

                    // return both the action and the state produced by the action for the post side effector
                    return new { dispatchedAction, state = rootReducer(state, action) };
                })
                .Do(actionState => stateStream.OnNext(actionState.state))
                .Do(actionState =>
                {
                    foreach (var sideEffector in sideEffectorCollection.Values) sideEffector.PostEffect(actionState.dispatchedAction, this);
                })
                .Subscribe();

            // Spin up a background task to blockingly consume from the actionQueue and feed new actions into the actionStream.
            Task.Run(() =>
            {
                var token = actionQueueCancellationTokenSource.Token;
                while (!token.IsCancellationRequested)
                {
                    var succeeded = actionQueue.TryTake(out var next, Timeout.Infinite, token);

                    if (next is null || !succeeded) continue;

                    actionStream.OnNext(next);
                }

                token.ThrowIfCancellationRequested();
            }, actionQueueCancellationTokenSource.Token);
        }

        /// <inheritdoc cref="IActionDispatcher{TRootState}.Dispatch"/>
        public void Dispatch(IAction action)
        {
            ValidateInitializeAction(action);
            actionQueue.Add(new DispatchedAction(action), actionQueueCancellationTokenSource.Token);
        }

        private readonly IComparer<ISideEffector<TRootState>> comparer = new SideEffectorPriorityComparer<TRootState>();

        /// <inheritdoc cref="IActionDispatcher{TRootState}.RegisterSideEffector"/>
        public Guid RegisterSideEffector(ISideEffector<TRootState> sideEffector)
        {
            var id = Guid.NewGuid();
            sideEffectorCollection.Add(id, sideEffector);

            var index = sideEffectorsByPriority.BinarySearch(sideEffector, comparer);

            if (index < 0)
            {
                // a side effector with the same priority was not found, so this one should be inserted at the bitwise complement of the returned index.
                // See <https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1.BinarySearch>
                index = ~index;
            }
            else
            {
                // a side effector with the same priority already exists in the list, insert this one after the existing one.
                while (++index < sideEffectorsByPriority.Count &&
                    sideEffectorsByPriority[index].Priority == sideEffector.Priority) { }
            }

            sideEffectorsByPriority.Insert(index, sideEffector);

            return id;
        }

        /// <inheritdoc cref="IActionDispatcher{TRootState}.UnregisterSideEffector"/>
        public void UnregisterSideEffector(Guid sideEffectorId)
        {
            var sideEffector = sideEffectorCollection[sideEffectorId];
            sideEffectorCollection.Remove(sideEffectorId);

            var index = sideEffectorsByPriority.BinarySearch(sideEffector, comparer);
            if (index < 0) return;

            sideEffectorsByPriority.RemoveAt(index);
        }

        /// <inheritdoc cref="IStateContainer{TRootState}.Select{TSelectedState}"/>
        public TSelectedState Select<TSelectedState>(Func<TRootState, TSelectedState> selector) => selector(State);

        /// <inheritdoc cref="IStateContainer{TRootState}.ObservableFor{TSelectedState}"/>
        public IObservable<TSelectedState> ObservableFor<TSelectedState>(Func<TRootState, TSelectedState> selector, bool notifyImmediately = false) =>
            Observable.Create<TRootState>(observer => stateStream.Subscribe(
                    onNext: next =>
                    {
                        try { observer.OnNext(next); }
                        catch (Exception e) { observer.OnError(e); }
                    },
                    observer.OnError,
                    observer.OnCompleted
                ))
                .StartWith(State)
                .Select(selector)
                .DistinctUntilChanged()
                .Skip(notifyImmediately ? 0 : 1);

        /// Throws an error if an incorrectly typed InitializeAction is dispatched to this store.
        private static void ValidateInitializeAction(IAction action)
        {
            var actionType = action.GetType();
            var isInitializeAction = actionType.IsGenericType && actionType.GetGenericTypeDefinition() == typeof(InitializeAction<>);
            var isInitializeActionCorrectType = isInitializeAction && action is InitializeAction<TRootState>;

            if (isInitializeAction && !isInitializeActionCorrectType) { throw new InitializeTypeMismatchException(actionType.GetGenericArguments()[0], typeof(TRootState)); }
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            actionQueue.Dispose();
            stateStream.Dispose();
            actionStreamHandle.Dispose();
            actionQueueCancellationTokenSource.Dispose();
        }
    }
}