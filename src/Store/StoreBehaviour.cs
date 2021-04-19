#nullable enable
using System;
using UnityEngine;

namespace Playdux.src.Store
{
    /// <summary>
    /// StoreBehaviour is a MonoBehaviour wrapper for Store. Its purpose is to provide a way for a Store to be injected
    /// into other MonoBehaviours through the Unity editor.
    /// </summary>
    /// <remarks>
    /// StoreBehaviour is abstract because you, the developer, must define a sublcass of it in order to initialize the
    /// store correctly.
    /// </remarks>
    [DefaultExecutionOrder(EXECUTION_PRIORITY)]
    public abstract class StoreBehaviour<TRootState> : MonoBehaviour, IStore<TRootState> where TRootState : class
    {
        private const int EXECUTION_PRIORITY = -100;
        
        /// The <see cref="Store{TRootState}" /> that this <see cref="StoreBehaviour{TRootState}"/> wraps.
        /// This is set in the base StoreBehaviour.Awake Unity event and should not be set elsewhere.
        public IStore<TRootState>? Store { get; private set; }

        /// <remarks>You if you implement your own awake on a subclass of StoreBehaviour, you *must*
        /// call base.Awake() to ensure that Store is initialized correctly.</remarks>
        protected void Awake() => Store = InitializeStore();

        /// <summary>InitializeStore must be overriden by subclasses of StoreBehaviour. This is where you can
        /// initialize your store with initial state, set up reducers, etc.</summary>
        protected abstract Store<TRootState> InitializeStore();

        public TRootState State => Store!.State;

        public void Dispatch(IAction action) => Store!.Dispatch(action);
        public Guid RegisterSideEffector(ISideEffector<TRootState> sideEffector) => Store!.RegisterSideEffector(sideEffector);
        public void UnregisterSideEffector(Guid sideEffectorId) => Store!.UnregisterSideEffector(sideEffectorId);
        
        public TSelectedState Select<TSelectedState>(Func<TRootState, TSelectedState> selector) => Store!.Select(selector);
        public IObservable<TSelectedState> ObservableFor<TSelectedState>(Func<TRootState, TSelectedState> selector, bool notifyImmediately = false) =>
            Store!.ObservableFor(selector, notifyImmediately);
    }
}