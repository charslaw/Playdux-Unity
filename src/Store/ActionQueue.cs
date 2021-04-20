#nullable enable
using System;
using System.Collections.Concurrent;
using Playdux.src.Utils;

namespace Playdux.src.Store
{
    /// Holds actions and ensures that they get handled in FIFO order.
    public class ActionQueue
    {
        private readonly ConcurrentQueue<DispatchedAction> q = new();
        private readonly Action<DispatchedAction> actionHandler;

        public ActionQueue(Action<DispatchedAction> actionHandler) { this.actionHandler = actionHandler; }

        private bool isBeingConsumed;

        /// Adds an action to the action queue. The action will be sent to the provided action handler when it is at the head of the queue.
        public void Dispatch(DispatchedAction action)
        {
            q.Enqueue(action);
            if (isBeingConsumed) return;

            using (new DisposableLatch(() => isBeingConsumed = true, () => isBeingConsumed = false))
            {
                while (q.TryDequeue(out var next)) actionHandler(next);
            }
        }
    }
}