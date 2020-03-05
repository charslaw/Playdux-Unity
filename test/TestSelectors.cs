using System;

namespace AReSSO.Test
{
    internal static class TestSelectors
    {
        public static SimpleTestState ErrorSimpleTestStateSelector(SimpleTestState state) => throw new Exception();
    }
}