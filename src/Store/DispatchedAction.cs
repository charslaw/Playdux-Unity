using System;
using System.Diagnostics;

namespace AReSSO.Store
{
    /// An IAction wrapped with some additional metadata for debugging purposes.
    public record DispatchedAction(IAction Action, DateTime DispatchTime, StackTrace DispatchStackTrace)
    {
        public DispatchedAction(IAction action) : this(action, DateTime.Now, new StackTrace(1)) { }
    }
}