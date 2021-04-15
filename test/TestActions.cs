#nullable enable
using Playdux.src.Store;

namespace Playdux.test
{
    internal record EmptyAction : IAction;

    internal record SimpleStateAddAction(int Value) : IAction;

    internal record BetterSimpleStateAddAction(int Value) : IAction;
}