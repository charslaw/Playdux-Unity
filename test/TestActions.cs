#nullable enable
using Playdux.src.Store;

namespace Playdux.test
{
    internal record EmptyAction : IAction;

    internal record SimpleStateAdd(int Value) : IAction;

    internal record BetterSimpleStateAdd(int Value) : IAction;
}