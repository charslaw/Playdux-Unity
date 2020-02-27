using System;
using UnityEngine;

namespace AReSSO.Store
{
    /// <summary>
    /// StoreBehaviour is a MonoBehaviour wrapper for Store. Its purpose is to provide a way for a Store to be injected
    /// into other MonoBehaviours through the Unity editor.
    /// </summary>
    /// <remarks>
    /// StoreBehaviour is abstract because you, the developer, must define a sublcass of it in order to initialize the
    /// store correctly.
    /// </remarks>
    public abstract class StoreBehaviour<TRootState> : MonoBehaviour, IStore<TRootState>
    {
        public Store<TRootState> Store { get; private set; }
        
        /// <remarks>You if you implement your own awake on a subclass of StoreBehaviour, you *must*
        /// call base.Awake() to ensure that Store is initialized correctly.</remarks>
        protected void Awake()
        {
            Store = InitializeStore();
        }

        /// <summary>InitializeStore must be overriden by subclasses of StoreBehaviour. This is where you can
        /// initialize your store with initial state, set up reducers, etc.</summary>
        protected abstract Store<TRootState> InitializeStore();

        /// <see cref="IStateContainer{TRootState}.State"/>
        public TRootState State => Store.State;

        /// <see cref="IActionDispatcher.Dispatch"/>
        public void Dispatch(IAction action) => Store.Dispatch(action);

        /// <see cref="IStateContainer{TRootState}.Select{TSelectedState}"/>
        public TSelectedState Select<TSelectedState>(Func<TRootState, TSelectedState> selector) =>
            Store.Select(selector);

        /// <see cref="IStateContainer{TRootState}.ObservableFor{TSelectedState}"/>
        public IObservable<TSelectedState> ObservableFor<TSelectedState>(
            Func<TRootState, TSelectedState> selector, bool notifyImmediately = false) =>
            Store.ObservableFor(selector, notifyImmediately);
    }
}