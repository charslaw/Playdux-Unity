#nullable enable
using System;
using Playdux.src.Store;

namespace Playdux.test
{
    internal static class TestReducers
    {
        public static Func<SimpleTestState, IAction, SimpleTestState> GenerateSetNSimpleTestStateReducer(int value) => (state, _) => state with { N = value };

        public static SimpleTestState IdentitySimpleTestStateReducer(SimpleTestState state, IAction _) => state;

        public static SimpleTestState IncrementNSimpleTestStateReducer(SimpleTestState state, IAction _) => state with { N = state.N + 71 };

        public static SimpleTestState AcceptAddSimpleTestStateReducer(SimpleTestState state, IAction action)
        {
            return action switch
            {
                SimpleStateAddAction (var value) => state with { N = state.N + value },
                BetterSimpleStateAddAction (var value) => state with { N = state.N + value + 1 },
                _ => state
            };
        }

        public static Point IncrementYPointReducer(Point state, IAction _) => state with { Y = state.Y + 1 };

        public static Point IdentityPointReducer(Point state, IAction _) => state;
    }
}