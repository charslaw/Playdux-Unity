#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Playdux.src.Utils;
using UniRx;
using UnityEngine;

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

        /// Reduces state according to actions dispatched to the store.
        private readonly Func<TRootState, IAction, TRootState> rootReducer;

        /// Holds actions in a defined FIFO order to ensure actions are processed in the order that they are received.
        private readonly ConcurrentQueue<DispatchedAction> actionQueue = new();

        /// A stream of the current state within the store.
        /// This stream is safely shared to consumers (via ObservableFor) in such a way that consumer cancellation and errors are isolated from the main stream.
        private readonly BehaviorSubject<TRootState> stateStream;

        /// Stores SideEffectors in such a way that they can be unregistered later if necessary.
        private readonly Dictionary<Guid, ISideEffector<TRootState>> sideEffectorCollection = new();

        /// Stores SideEffectors in priority order.
        private readonly List<ISideEffector<TRootState>> sideEffectorsByPriority = new();

        /// Create a new store with a given initial state and reducer
        public Store(TRootState initialState, Func<TRootState, IAction, TRootState> rootReducer, IEnumerable<ISideEffector<TRootState>>? initialSideEffectors = null)
        {
            if (initialSideEffectors is not null)
            {
                foreach (var sideEffector in initialSideEffectors) RegisterSideEffector(sideEffector);
            }

            this.rootReducer = rootReducer;
            stateStream = new BehaviorSubject<TRootState>(initialState);
        }

        /// Ensures that only one call to Dispatch will end up consuming the action queue at a time.
        private bool isConsumingActionQueue;

        /// <inheritdoc cref="IActionDispatcher{TRootState}.Dispatch"/>
        public void Dispatch(IAction action)
        {
            ValidateInitializeAction(action);

            actionQueue.Enqueue(new DispatchedAction(action));

            // If another call to Dispatch is already pulling items from the queue, just add this action to the queue and
            if (isConsumingActionQueue) return;

            using (new DisposableLatch(() => isConsumingActionQueue = true, () => isConsumingActionQueue = false))
            {
                while (actionQueue.TryDequeue(out var next)) { DispatchInternal(next); }
            }
        }

        /// Handles a single dispatched action from the queue, activating pre effects, reducing state, updating state, then activating post effects.
        private void DispatchInternal(DispatchedAction dispatchedAction)
        {
            // Pre Effects
            foreach (var sideEffector in sideEffectorsByPriority)
            {
                try
                {
                    var shouldAllow = sideEffector.PreEffect(dispatchedAction, this);
                    if (!shouldAllow) dispatchedAction = dispatchedAction with { IsCanceled = true };
                }
                catch (Exception e)
                {
                    Debug.LogError("Error encountered in side effector pre effect");
                    Debug.LogException(e);
                }
            }

            if (dispatchedAction.IsCanceled) return;

            // Reduce
            var action = dispatchedAction.Action;
            var state = State;
            if (action is InitializeAction<TRootState> castAction) state = castAction.InitialState;
            state = rootReducer(state, action);

            // Update State
            stateStream.OnNext(state);

            // Post Effects
            foreach (var sideEffector in sideEffectorCollection.Values)
            {
                try { sideEffector.PostEffect(dispatchedAction, this); }
                catch (Exception e)
                {
                    Debug.LogError("Error encountered in side effector post effect");
                    Debug.LogException(e);
                }
            }
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
        public void Dispose() { stateStream.Dispose(); }
    }
}