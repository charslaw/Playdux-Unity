using System;
using UniRx;

namespace AReSSO
{
    /// <summary>
    /// An AReSSO state container. The core of AReSSO.
    /// Includes capability to dispatch actions to the store, get the current state, get the current state narrowed by
    /// a selector, and get an IObservable to the "selected" state.
    /// </summary>
    public class Store<TRootState>
    {
        /// <summary>The current state within the store.</summary>
        public TRootState State { get; private set; }
        
        // The reducer used when an action is dispatched to the store.
        private readonly Func<TRootState, IAction, TRootState> rootReducer;
        // An IObservable to allow client code to observe changes to store state.
        private readonly BehaviorSubject<TRootState> stateSubject;
        // Used to prevent multiple threads from concurrently modifying state via reducers.
        private readonly object reduceLock = new object();
        // Used to prevent multiple threads from concurrently emitting into stateSubject.
        private readonly object stateSubjectLock = new object();
        
        /// <summary>Create a new store with a given initial state and reducer</summary>
        public Store(TRootState initialState, Func<TRootState, IAction, TRootState> rootReducer)
        {
            State = initialState;
            stateSubject = new BehaviorSubject<TRootState>(initialState);
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

            if (!newState.Equals(State))
            {
                lock (stateSubjectLock)
                {
                    stateSubject.OnNext(newState);
                }
            }

            State = newState;
        }

        /// <summary>Returns the current state filtered by the given selector.</summary>
        public TSelectedState Select<TSelectedState>(Func<TRootState, TSelectedState> selector) => selector(State);

        /// <summary>Produces an IObservable looking at the object specified by the given selector.</summary>
        /// <remarks>The returned IObservable will emit immediately on subscribe, and will subsequently only notify
        /// when the selected object has changed.</remarks>
        public IObservable<TSelectedState> ObservableFor<TSelectedState>(Func<TRootState, TSelectedState> selector)
        {
            lock (stateSubjectLock)
            {
                return stateSubject.AsObservable().CatchIgnore().Select(selector).DistinctUntilChanged().Share();
            }
        }
    }
}