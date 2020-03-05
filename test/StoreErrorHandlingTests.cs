using System;
using AReSSO.Store;
using NUnit.Framework;
using UniRx;

namespace AReSSO.Test
{
    public class StoreErrorHandlingTests
    {
        [Test]
        public void ErrorInOneObserverDoesNotBreakOtherObservers()
        {
            SimpleTestState init = new SimpleTestState(42);
            var store = new Store<SimpleTestState>(init, TestReducers.IncrementNSimpleTestStateReducer);

            int notified = 0;
            int errorSeen = 0;
            store.ObservableFor(state => state).Subscribe(
                state => notified++,
                error => errorSeen++);
            store.ObservableFor(state => state).Subscribe(
                state => throw new Exception(),
                error => { });

            try { store.Dispatch(new EmptyAction()); }
            catch { /* ignored */ }
            try { store.Dispatch(new EmptyAction()); }
            catch { /* ignored */ }
            

            Assert.AreEqual(0, errorSeen, $"Saw {errorSeen} errors");
            Assert.AreEqual(2, notified, $"Saw {notified} notifications");
        }
        
        [Test]
        public void ErrorInSelectorDoesNotBreakOtherObservers()
        {
            SimpleTestState init = new SimpleTestState(42);
            var store = new Store<SimpleTestState>(init, TestReducers.IncrementNSimpleTestStateReducer);

            int notified1 = 0;
            int errors1 = 0;
            store.ObservableFor(state => state).Subscribe(
                state => notified1++,
                error => errors1++);
            store.ObservableFor(TestSelectors.ErrorSimpleTestStateSelector).Subscribe(
                _ => { },
                _ => { }
            );

            store.Dispatch(new EmptyAction());
            store.Dispatch(new EmptyAction());
            

            Assert.AreEqual(0, errors1, $"1 Saw {errors1} errors");
            Assert.AreEqual(2, notified1, $"1 Saw {notified1} notifications");
        }
    }
}