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
    public class Store<TRootState> : IStore<TRootState>
    {
        /// <summary>The current state within the store.</summary>
        public TRootState State { get; private set; }

        // The reducer used when an action is dispatched to the store.
        private readonly Func<TRootState, IAction, TRootState> rootReducer;
        // Used to prevent multiple threads from concurrently modifying state via reducers.
        private readonly object reduceLock = new object();
        
        // A list of observers paying attention to the state in this store
        private readonly List<IStoreObserver> observers = new List<IStoreObserver>();
        // Used to prevent multiple threads from concurrently adding to observers while notifying observers.
        private readonly object observersLock = new object();

        /// <summary>Create a new store with a given initial state and reducer</summary>
        public Store(TRootState initialState, Func<TRootState, IAction, TRootState> rootReducer)
        {
            State = initialState;
            this.rootReducer = rootReducer;
        }

        /// <summary>
        /// Dispatch an action to the Store. Changes store state according to the reducer provided at creation.
        /// </summary>
        /// <remarks>If multiple clients call Dispatch concurrently, this call will block and actions will be consumed
        /// in FIFO order.</remarks>
        public void Dispatch(IAction action)
        {
            TRootState newState;
            lock (reduceLock)
            {
                newState = rootReducer(State, action);
            }

            if (newState.Equals(State)) return;
            
            State = newState;
            foreach (var observer in observers)
            {
                observer.Notify(State);
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
            var observer = new StoreObserver<TSelectedState>(selector, State);
            observers.Add(observer);
            return observer.Subject.DistinctUntilChanged().Skip(notifyImmediately ? 0 : 1);
        }

        private interface IStoreObserver
        {
            void Notify(TRootState state);
        }
        
        /// <summary>Represents a subscription to the store state.</summary>
        private class StoreObserver<TSelectedState> : IStoreObserver
        {
            private readonly Func<TRootState, TSelectedState> selector;
            public readonly BehaviorSubject<TSelectedState> Subject;

            public StoreObserver(Func<TRootState, TSelectedState> selector, TRootState initial)
            {
                this.selector = selector;
                Subject = new BehaviorSubject<TSelectedState>(this.selector(initial));
            }

            public void Notify(TRootState state)
            {
                Subject.OnNext(selector(state));
            }
        }
    }
}