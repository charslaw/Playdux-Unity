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
            var store = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            Assert.AreEqual(init, store.State);
        }

        [Test]
        public void StateNotChangedAfterDispatchWhenReducerDoesNotChangeState()
        {
            SimpleTestState init = new SimpleTestState(42);
            var store = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);
            
            store.Dispatch(new EmptyAction());
            
            Assert.AreEqual(init, store.State);
        }

        [Test]
        public void StateChangedAfterDispatchWhenReducerDoesChangeState()
        {
            SimpleTestState init = new SimpleTestState(42);
            var store = new Store<SimpleTestState>(init, TestReducers.GenerateSetNSimpleTestStateReducer(243));
            
            store.Dispatch(new EmptyAction());
            
            Assert.AreEqual(243, store.State.N);
        }

        [Test]
        public void ObserverNotNotifiedOnDispatchWhenReducerDoesNotChangeState()
        {
            SimpleTestState init = new SimpleTestState(42);
            var store = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            int notified = 0;
            store.ObservableFor(state => state).Subscribe(_ => notified++);
            
            store.Dispatch(new EmptyAction());

            Assert.AreEqual(0, notified);
        }

        [Test]
        public void ObserverNotifiedOnDispatchWhenReducerDoesChangeState()
        {
            SimpleTestState init = new SimpleTestState(42);
            var store = new Store<SimpleTestState>(init, TestReducers.IncrementNSimpleTestStateReducer);

            int notified = 0;
            store.ObservableFor(state => state).Subscribe(_ => notified++);
            
            store.Dispatch(new EmptyAction());

            Assert.AreEqual(1, notified);
        }

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
        public void ObserverNotNotifiedForChangeOutsideOfSelector()
        {
            Point init = new Point(4, 2);
            var store = new Store<Point>(init, TestReducers.IncrementYPointReducer);
            
            int notified = 0;
            store.ObservableFor(state => state.X).Subscribe(_ => notified++);
            
            store.Dispatch(new EmptyAction());

            Assert.AreEqual(0, notified);
        }

        [Test]
        public void ObserverNotifiedForChangeInsideOfSelector()
        {
            Point init = new Point(4, 2);
            var store = new Store<Point>(init, TestReducers.IncrementYPointReducer);
            
            int notified = 0;
            store.ObservableFor(state => state.Y)
                .Subscribe(_ => notified++);
            
            store.Dispatch(new EmptyAction());

            // One for Initialize, one for Empty action.
            Assert.AreEqual(1, notified);
        }

        [Test]
        public void CanUnsubscribeWithoutBreakingEverything()
        {
            Point init = new Point(4, 2);
            var store = new Store<Point>(init, TestReducers.IncrementYPointReducer);
            
            var disposable = store.ObservableFor(state => state.Y).Subscribe(_ => { });
            disposable.Dispose();
            
            Assert.DoesNotThrow(() => store.Dispatch(new EmptyAction()));
        }
    }

    internal static class TestReducers
    {
        public static Func<SimpleTestState, IAction, SimpleTestState> GenerateSetNSimpleTestStateReducer(int value) =>
            (SimpleTestState state, IAction __) => state.Copy(value);
        
        public static SimpleTestState IdentitySimpleTestStateReducer(SimpleTestState state, IAction _) => state;

        public static SimpleTestState IncrementNSimpleTestStateReducer(SimpleTestState state, IAction _) =>
            state.Copy(state.N + 71);

        public static Point IncrementYPointReducer(Point state, IAction _) => state.Copy(y: state.Y + 1);
    }
}