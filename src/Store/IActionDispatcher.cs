#nullable enable

namespace AReSSO.Store
{
    public interface IActionDispatcher
    {
        /// <summary>
        /// Dispatch an action to the Store. Changes store state according to the reducer provided at creation.
        /// </summary>
        /// <remarks>If multiple clients call Dispatch concurrently, this call will block and actions will be consumed
        /// in FIFO order.</remarks>
        void Dispatch(IAction action);
    }
}