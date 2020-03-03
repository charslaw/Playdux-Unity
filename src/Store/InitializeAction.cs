namespace AReSSO.Store
{
    /// <summary>
    /// The InitializeAction is a special reserved action which must be used to initialize a Store.
    /// </summary>
    /// <remarks>
    /// If a store is not initialized with this action prior to being used,
    /// it will throw a StoreNotInitializedException.
    /// </remarks>
    public class InitializeAction<TRootState> : IAction
    {
        public TRootState InitialState { get; }

        public InitializeAction(TRootState initialState) => InitialState = initialState;
    }
}