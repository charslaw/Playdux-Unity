#nullable enable
using System;

namespace Playdux.test
{
    internal static class TestSelectors
    {
        public static SimpleTestState ErrorSimpleTestStateSelector(SimpleTestState state) => throw new Exception();
    }
}