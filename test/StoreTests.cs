using System;
using NUnit.Framework;
using UniRx;
using AReSSO.Store;

namespace AReSSO.Test
{
    public class StoreTests
    {
        public class EmptyAction : IAction {}
        
        [Test]
        public void GetStateOnNewStoreReturnsInitialState()
        {
            SimpleTestState init = new SimpleTestState(42);
            var store = new Store<SimpleTestState>(init, (state, _) => state);

            Assert.AreEqual(init, store.State);
        }

        [Test]
        public void StateNotChangedAfterDispatchWhenReducerDoesNotChangeState()
        {
            SimpleTestState init = new SimpleTestState(42);
            var store = new Store<SimpleTestState>(init, (state, _) => state);
            
            store.Dispatch(new EmptyAction());
            
            Assert.AreEqual(init, store.State);
        }

        [Test]
        public void StateChangedAfterDispatchWhenReducerDoesChangeState()
        {
            SimpleTestState init = new SimpleTestState(42);
            var store = new Store<SimpleTestState>(init, (state, _) => state.Copy(243));
            
            store.Dispatch(new EmptyAction());
            
            Assert.AreEqual(243, store.State.N);
        }

        [Test]
        public void ObserverNotNotifiedOnDispatchWhenReducerDoesNotChangeState()
        {
            SimpleTestState init = new SimpleTestState(42);
            var store = new Store<SimpleTestState>(init, (state, _) => state);

            int notified = 0;
            store.ObservableFor(state => state).Subscribe(_ => notified++);
            
            store.Dispatch(new EmptyAction());

            Assert.AreEqual(0, notified);
        }

        [Test]
        public void ObserverNotifiedOnDispatchWhenReducerDoesChangeState()
        {
            SimpleTestState init = new SimpleTestState(42);
            var store = new Store<SimpleTestState>(init, (state, _) => state.Copy(567412));

            int notified = 0;
            store.ObservableFor(state => state).Subscribe(_ => notified++);
            
            store.Dispatch(new EmptyAction());

            Assert.AreEqual(1, notified);
        }

        [Test]
        public void ErrorInOneObserverDoesNotBreakOtherObservers()
        {
            SimpleTestState init = new SimpleTestState(42);
            var store = new Store<SimpleTestState>(init, (state, _) => state.Copy(state.N + 1));

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
        public void ObserverNotNotifiedForChangeOutsideOfSelector()
        {
            Point init = new Point(4, 2);
            var store = new Store<Point>(init, (state, _) => state.Copy(y: 8947));
            
            int notified = 0;
            store.ObservableFor(state => state.X).Subscribe(_ => notified++);
            
            store.Dispatch(new EmptyAction());

            Assert.AreEqual(0, notified);
        }

        [Test]
        public void ObserverNotifiedForChangeInsideOfSelector()
        {
            Point init = new Point(4, 2);
            var store = new Store<Point>(init, (state, _) => state.Copy(y: 8947));
            
            int notified = 0;
            store.ObservableFor(state => state.Y).Subscribe(_ => notified++);
            
            store.Dispatch(new EmptyAction());

            Assert.AreEqual(1, notified);
        }

        [Test]
        public void CanUnsubscribeWithoutBreakingEverything()
        {
            Point init = new Point(4, 2);
            var store = new Store<Point>(init, (state, _) => state.Copy(y: 8947));
            
            var disposable = store.ObservableFor(state => state.Y).Subscribe(_ => { });
            disposable.Dispose();
            
            Assert.DoesNotThrow(() => store.Dispatch(new EmptyAction()));
        }
    }
}