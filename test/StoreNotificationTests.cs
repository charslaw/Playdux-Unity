#nullable enable
using NUnit.Framework;
using Playdux.src.Store;
using UniRx;
using static Playdux.test.TestUtils.TestUtils;

namespace Playdux.test
{
    public class StoreNotificationTests
    {
        private const int DELAY = 1;

        private Store<SimpleTestState>? simpleStore;
        private Store<Point>? pointStore;

        [TearDown]
        public void Teardown()
        {
            simpleStore?.Dispose();
            pointStore?.Dispose();
        }

        [Test]
        public void ObserverNotNotifiedOnDispatchWhenReducerDoesNotChangeState()
        {
            SimpleTestState init = new(42);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            int notified = 0;
            simpleStore.ObservableFor(state => state).Subscribe(_ => notified++);

            simpleStore.Dispatch(new EmptyAction());

            BlockingWait(DELAY);
            Assert.AreEqual(0, notified, "The consumer was not notified the correct number of times.");
        }

        [Test]
        public void ObserverNotifiedOnDispatchWhenReducerDoesChangeState()
        {
            SimpleTestState init = new(42);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IncrementNSimpleTestStateReducer);

            int notified = 0;
            simpleStore.ObservableFor(state => state).Subscribe(_ => notified++);

            simpleStore.Dispatch(new EmptyAction());

            BlockingWait(DELAY);
            Assert.AreEqual(1, notified, "The consumer was not notified the correct number of times.");
        }


        [Test]
        public void ObserverNotNotifiedForChangeOutsideOfSelector()
        {
            Point init = new(4, 2);
            pointStore = new Store<Point>(init, TestReducers.IncrementYPointReducer);

            int notified = 0;
            pointStore.ObservableFor(state => state.X).Subscribe(_ => notified++);

            pointStore.Dispatch(new EmptyAction());

            BlockingWait(DELAY);
            Assert.AreEqual(0, notified, "The consumer was not notified the correct number of times.");
        }

        [Test]
        public void ObserverNotifiedForChangeInsideOfSelector()
        {
            Point init = new(4, 2);
            pointStore = new Store<Point>(init, TestReducers.IncrementYPointReducer);

            int notified = 0;
            pointStore.ObservableFor(state => state.Y).Subscribe(_ => notified++);

            pointStore.Dispatch(new EmptyAction());

            BlockingWait(DELAY);
            Assert.AreEqual(1, notified, "The consumer was not notified the correct number of times.");
        }
    }
}