#nullable enable

namespace Playdux.src.Store
{
    public interface IStore<out TRootState> : IActionDispatcher, IStateContainer<TRootState> { }
}