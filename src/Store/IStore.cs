#nullable enable

namespace Playdux.src.Store
{
    public interface IStore<out TRootState> : IActionDispatcher<TRootState>, IStateContainer<TRootState> { }
}