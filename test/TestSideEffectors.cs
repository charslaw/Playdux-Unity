#nullable enable
using System;
using Playdux.src.Store;

namespace Playdux.test
{
    public static class TestSideEffectors
    {
        public class FakeSideEffector<T> : ISideEffector<T>
        {
            private readonly Func<DispatchedAction, IActionDispatcher, bool> pre;
            private readonly Action<DispatchedAction, T, IActionDispatcher> post;

            public FakeSideEffector(Func<DispatchedAction, IActionDispatcher, bool>? pre = null, Action<DispatchedAction, T, IActionDispatcher>? post = null)
            {
                this.pre = pre ?? ((_, _) => true);
                this.post = post ?? ((_, _, _) => { });
            }

            public bool PreEffect(DispatchedAction dispatchedAction, IActionDispatcher dispatcher) => pre(dispatchedAction, dispatcher);
            public void PostEffect(DispatchedAction dispatchedAction, T state, IActionDispatcher dispatcher) => post(dispatchedAction, state, dispatcher);
        }
        
        public class DoesNothingSideEffector<T> : FakeSideEffector<T>
        {
            public DoesNothingSideEffector() : base((_, _) => true) { }
        }

        public class PreventsAllActionsSideEffector<T> : FakeSideEffector<T>
        {
            public PreventsAllActionsSideEffector() : base((_, _) => false) { }
        }
    }
}