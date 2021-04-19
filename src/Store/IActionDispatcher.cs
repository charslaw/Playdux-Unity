using System;

#nullable enable

namespace Playdux.src.Store
{
    public interface IActionDispatcher<out TRootState>
    {
        /// <summary>
        /// Dispatch an action to the Store. Changes store state according to the reducer provided at creation.
        /// </summary>
        /// <remarks>Actions will be consumed in FIFO order.</remarks>
        void Dispatch(IAction action);

        /// <summary>
        /// Registers a new Side Effector to observe this Store.
        /// </summary>
        Guid RegisterSideEffector(ISideEffector<TRootState> sideEffector);

        /// <summary>
        /// Unregisters a previously registered Side Effector.
        /// </summary>
        void UnregisterSideEffector(Guid sideEffectorId);
    }
}