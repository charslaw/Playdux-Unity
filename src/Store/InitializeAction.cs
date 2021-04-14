#nullable enable

namespace Playdux.src.Store
{
    /// <summary>
    /// The InitializeAction is a special reserved action which can be used to initialize a Store.
    /// </summary>
    public sealed record InitializeAction<TRootState>(TRootState InitialState) : IAction;
}