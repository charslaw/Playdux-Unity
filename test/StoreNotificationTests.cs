using AReSSO.Store;
using NUnit.Framework;
using UniRx;

namespace AReSSO.Test
{
    public class StoreNotificationTests
    {
        
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
    }
}