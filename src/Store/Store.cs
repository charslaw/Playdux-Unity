using System;
using System.Collections.Generic;
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
        private TRootState state;

        /// <summary>The current state within the store.</summary>
        public TRootState State => state ?? throw new StoreNotInitializedException();

        // The reducer used when an action is dispatched to the store.
        private readonly Func<TRootState, IAction, TRootState> rootReducer;

        // Used to prevent multiple threads from concurrently dispatching actions which modify state.
        private readonly object stateLock = new object();

        // Observable source for State.ObservableFor
        private readonly BehaviorSubject<TRootState> subject = new BehaviorSubject<TRootState>(null);

        // Used to prevent multiple threads from concurrently onNext-ing into the subject and subscribing to the subject.
        private readonly object subjectLock = new object();

        /// <summary>Create a new store with a given initial state and reducer</summary>
        public Store(TRootState initialState, Func<TRootState, IAction, TRootState> rootReducer)
        {
            this.rootReducer = rootReducer;

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

            lock (stateLock)
            {
                switch (action)
                {
                    case InitializeAction<TRootState> initializer:
                        state = initializer.InitialState;
                        break;
                    case InitializeAction<IAction> mismatchedInitializer:
                        throw new InitialStateTypeMismatchException(
                            mismatchedInitializer.InitialState.GetType(),
                            state.GetType()
                        );
                }

                state = rootReducer(state, dispatchedAction.Action);
            }

            lock (subjectLock)
            {
                subject.OnNext(state);
            }
        }

        /// <summary>Returns the current state filtered by the given selector.</summary>
        public TSelectedState Select<TSelectedState>(Func<TRootState, TSelectedState> selector) => selector(State);

        /// <summary>Produces an IObservable looking at the state specified by the given selector.</summary>
        /// <remarks>The returned IObservable will only emit when the selected state changes.</remarks>
        public IObservable<TSelectedState> ObservableFor<TSelectedState>(
            Func<TRootState, TSelectedState> selector,
            bool notifyImmediately = false)
        {
            lock (subjectLock)
            {
                return subject.Select(selector).DistinctUntilChanged().Skip(notifyImmediately ? 0 : 1);
            }
        }
    }
}