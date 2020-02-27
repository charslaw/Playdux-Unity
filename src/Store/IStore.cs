namespace AReSSO.Store
{
    public interface IStore<out TRootState> : IActionDispatcher, IStateContainer<TRootState> { }
}