using System;
using AReSSO.Store;

namespace AReSSO.Test
{
    internal static class TestReducers
    {
        public static Func<SimpleTestState, IAction, SimpleTestState> GenerateSetNSimpleTestStateReducer(int value) => (state, _) => state with { N = value };

        public static SimpleTestState IdentitySimpleTestStateReducer(SimpleTestState state, IAction _) => state;

        public static SimpleTestState IncrementNSimpleTestStateReducer(SimpleTestState state, IAction _) => state with { N = state.N + 71 };

        public static Point IncrementYPointReducer(Point state, IAction _) => state with { Y = state.Y + 1 };

        public static Point IdentityPointReducer(Point state, IAction _) => state;
    }
}