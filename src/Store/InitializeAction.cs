using System;

namespace AReSSO.Store
{
    /// <summary>
    /// The InitializeAction is a special reserved action which can be used to initialize a Store.
    /// </summary>
    public sealed class InitializeAction<TRootState> : IAction
    {
        public TRootState InitialState { get; }

        public InitializeAction(TRootState initialState) => InitialState = initialState;
    }

    internal static class InitializeHelper
    {
        /// Gets the type of state object in an InitializeAction
        /// Returns null if the action is not an InitializeAction
        internal static Type InitializeActionStateType(IAction action)
        {
            var actionType = action.GetType();
            var actionIsInitializeAction = actionType.IsGenericType &&
                actionType.GetGenericTypeDefinition() == typeof(InitializeAction<>);

            return actionIsInitializeAction ? actionType.GetGenericArguments()[0] : null;
        }
    }
}