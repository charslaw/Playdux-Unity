#nullable enable

using System;
using UniRx;

namespace AReSSO.Store
{
    /// <summary>
    /// An AReSSO state container. The core of AReSSO.
    /// Includes capability to dispatch actions to the store, get the current state, get the current state narrowed by
    /// a selector, and get an IObservable to the "selected" state.
    /// </summary>
    public class Store<TRootState> : IStore<TRootState> where TRootState : class
    {
        /// <summary>The current state within the store.</summary>
        public TRootState State => stateStream.Value;

        // The reducer used when an action is dispatched to the store.
        private readonly Func<TRootState, IAction, TRootState> rootReducer;

        // Used to prevent multiple threads from concurrently dispatching actions which modify state.
        private readonly object stateLock = new object();

        private readonly BehaviorSubject<TRootState> stateStream;

        // Used to prevent multiple threads from concurrently onNext-ing into the subject and subscribing to the subject.
        private readonly object notifyLock = new object();

        /// <summary>Create a new store with a given initial state and reducer</summary>
        public Store(TRootState initialState, Func<TRootState, IAction, TRootState> rootReducer)
        {
            this.rootReducer = rootReducer;
            stateStream = new BehaviorSubject<TRootState>(initialState);
            Dispatch(new InitializeAction<TRootState>(initialState));
        }

        /// <summary>
        /// Dispatch an action to the Store. Changes store state according to the reducer provided at creation.
        /// </summary>
        /// <remarks>If multiple clients call Dispatch concurrently, this call will block and actions will be consumed
        /// in FIFO order.</remarks>
        public void Dispatch(IAction action)
        {
            var dispatchedAction = new DispatchedAction(action);

            var actionAsInitializeAction = IsInitializeAction(action);

            lock (stateLock)
            {
                var state = State;
                if (actionAsInitializeAction is not null) state = actionAsInitializeAction.InitialState;
                stateStream.OnNext(rootReducer(state, dispatchedAction.Action));
            }
        }

        /// <summary>Returns the current state filtered by the given selector.</summary>
        public TSelectedState Select<TSelectedState>(Func<TRootState, TSelectedState> selector) => selector(State);

        /// <summary>Produces an IObservable looking at the state specified by the given selector.</summary>
        /// <remarks>The returned IObservable will only emit when the selected state changes.</remarks>
        public IObservable<TSelectedState> ObservableFor<TSelectedState>(Func<TRootState, TSelectedState> selector, bool notifyImmediately = false) =>
            Observable.CreateSafe<TRootState>(observer => stateStream.Subscribe(observer))
                .StartWith(State)
                .Select(selector)
                .DistinctUntilChanged()
                .Skip(notifyImmediately ? 0 : 1);

        private static InitializeAction<TRootState>? IsInitializeAction(IAction action)
        {
            var actionType = action.GetType();
            var isInitializeAction = actionType.IsGenericType && actionType.GetGenericTypeDefinition() == typeof(InitializeAction<>);
            var isInitializeActionCorrectType = isInitializeAction && action is InitializeAction<TRootState>;

            if (!isInitializeAction) return null;
            if (isInitializeActionCorrectType) return (InitializeAction<TRootState>) action;
            throw new InitializeTypeMismatchException(actionType.GetGenericArguments()[0], typeof(TRootState));
        }
    }
}