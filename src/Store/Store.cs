#nullable enable
using System;
using System.Collections.Generic;
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
        private readonly ActionQueue actionQueue;

        /// A stream of the current state within the store.
        /// This stream is safely shared to consumers (via ObservableFor) in such a way that consumer cancellation and errors are isolated from the main stream.
        private readonly BehaviorSubject<TRootState> stateStream;

        /// Holds side effectors in a collection that preserves priority while also providing fast addition and removal.
        private readonly SideEffectorCollection<TRootState> sideEffectors = new();

        /// Create a new store with a given initial state and reducer
        public Store(TRootState initialState, Func<TRootState, IAction, TRootState> rootReducer, IEnumerable<ISideEffector<TRootState>>? initialSideEffectors = null)
        {
            this.rootReducer = rootReducer;
            stateStream = new BehaviorSubject<TRootState>(initialState);
            actionQueue = new ActionQueue(DispatchInternal);

            if (initialSideEffectors is null) return;

            foreach (var sideEffector in initialSideEffectors) RegisterSideEffector(sideEffector);
        }

        /// <inheritdoc cref="IActionDispatcher{TRootState}.Dispatch"/>
        public void Dispatch(IAction action)
        {
            ValidateInitializeAction(action);
            actionQueue.Dispatch(new DispatchedAction(action));
        }

        /// Handles a single dispatched action from the queue, activating pre effects, reducing state, updating state, then activating post effects.
        private void DispatchInternal(DispatchedAction dispatchedAction)
        {
            // Pre Effects
            foreach (var sideEffector in sideEffectors.ByPriority)
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
            foreach (var sideEffector in sideEffectors.ByPriority)
            {
                try { sideEffector.PostEffect(dispatchedAction, this); }
                catch (Exception e)
                {
                    Debug.LogError("Error encountered in side effector post effect");
                    Debug.LogException(e);
                }
            }
        }

        /// <inheritdoc cref="IActionDispatcher{TRootState}.RegisterSideEffector"/>
        public Guid RegisterSideEffector(ISideEffector<TRootState> sideEffector) => sideEffectors.Register(sideEffector);

        /// <inheritdoc cref="IActionDispatcher{TRootState}.UnregisterSideEffector"/>
        public void UnregisterSideEffector(Guid sideEffectorId) => sideEffectors.Unregister(sideEffectorId);

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

            if (isInitializeAction && !isInitializeActionCorrectType) throw new InitializeTypeMismatchException(actionType.GetGenericArguments()[0], typeof(TRootState));
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose() => stateStream.Dispose();
    }
}