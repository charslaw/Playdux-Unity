namespace Playdux.src.Store
{
    public interface ISideEffector<in TRootState>
    {
        /// Execute a side effect before the action is sent to the reducer.
        public bool PreEffect(DispatchedAction dispatchedAction, IActionDispatcher dispatcher);
        
        /// Execute a side effect after the action has been sent to the reducer.
        public void PostEffect(DispatchedAction dispatchedAction, TRootState state, IActionDispatcher dispatcher);
    }
}