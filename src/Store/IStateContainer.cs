using System;

namespace AReSSO.Store
{
    public interface IStateContainer<out TRootState>
    {
        /// <summary>The current state within the store.</summary>
        TRootState State { get; }

        /// <summary>Returns the current state filtered by the given selector.</summary>
        TSelectedState Select<TSelectedState>(Func<TRootState, TSelectedState> selector);

        /// <summary>Produces an IObservable looking at the state specified by the given selector.</summary>
        /// <remarks>The returned IObservable will only emit when the selected state changes.</remarks>
        IObservable<TSelectedState> ObservableFor<TSelectedState>(
            Func<TRootState, TSelectedState> selector,
            bool notifyImmediately = false);
    }
}