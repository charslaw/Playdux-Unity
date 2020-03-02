using System;
using System.Diagnostics;

namespace AReSSO.Store
{
    
    public class DispatchedAction
    {
        /// The action that was dispatched
        public IAction Action { get; }

        /// The time at which the Action was dispatched. Note that this is different from the time at which the Action
        /// object was created.
        public DateTime DispatchTime { get; }

        /// Trace of the stack when the Action was dispatched.
        public StackTrace DispatchStackTrace { get; }

        public DispatchedAction(IAction action)
        {
            Action = action;
            DispatchTime = DateTime.Now;
            DispatchStackTrace = new StackTrace(1);
        }
    }
}