namespace AReSSO.Store
{
    /// <summary>
    /// The InitializeAction is a special reserved action which can be used to initialize a Store.
    /// </summary>
    public class InitializeAction<TRootState> : IAction
    {
        public TRootState InitialState { get; }

        public InitializeAction(TRootState initialState) => InitialState = initialState;
    }
}