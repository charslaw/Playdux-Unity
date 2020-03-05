using System;

namespace AReSSO.Test
{
    internal static class TestReducers
    {
        public static Func<SimpleTestState, IAction, SimpleTestState> GenerateSetNSimpleTestStateReducer(int value) =>
            (state, _) => state.Copy(value);
        
        public static SimpleTestState IdentitySimpleTestStateReducer(SimpleTestState state, IAction _) => state;

        public static SimpleTestState IncrementNSimpleTestStateReducer(SimpleTestState state, IAction _) =>
            state.Copy(state.N + 71);

        public static Point IncrementYPointReducer(Point state, IAction _) => state.Copy(y: state.Y + 1);

        public static Point IdentityPointReducer(Point state, IAction _) => state;
    }
}