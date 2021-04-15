#nullable enable
using System.Collections.Generic;

namespace Playdux.src.Store
{
    public interface ISideEffector<in TRootState>
    {
        /// Determines the order in which SideEffectors will be executed.
        /// Higher priorities will run first.
        public int Priority { get; }

        /// Execute a side effect before the action is sent to the reducer.
        public bool PreEffect(DispatchedAction dispatchedAction, IActionDispatcher dispatcher);

        /// Execute a side effect after the action has been sent to the reducer.
        public void PostEffect(DispatchedAction dispatchedAction, TRootState state, IActionDispatcher dispatcher);
    }

    internal class SideEffectorPriorityComparer<TRootState> : IComparer<ISideEffector<TRootState>>
    {
        public int Compare(ISideEffector<TRootState> x, ISideEffector<TRootState> y) => y.Priority.CompareTo(x.Priority);
    }
}