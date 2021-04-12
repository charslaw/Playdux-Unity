#nullable enable
using NUnit.Framework;
using Playdux.src.Store;
using UniRx;

namespace Playdux.test
{
    public class StoreTests
    {
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
        public void InitializeActionSetsState()
        {
            Point init = new Point(0, 1);
            var store = new Store<Point>(init, TestReducers.IdentityPointReducer);

            Point newState = new Point(10, 11);
            store.Dispatch(new InitializeAction<Point>(newState));

            Assert.AreEqual(newState with {}, store.State);
        }

        [Test]
        public void InitializeActionWithWrongStateTypeThrows()
        {
            Point init = new Point(default, default);
            var store = new Store<Point>(init, TestReducers.IdentityPointReducer);
            
            Assert.Throws<InitializeTypeMismatchException>(
                () => store.Dispatch(new InitializeAction<SimpleTestState>(new SimpleTestState(default)))
            );
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
        

        [Test]
        public void CanUnsubscribeWithoutBreakingOtherSubscribers()
        {
            Point init = new Point(4, 2);
            var store = new Store<Point>(init, TestReducers.IncrementYPointReducer);

            var notified = 0;
            var disposable = store.ObservableFor(state => state.Y).Subscribe(_ => { });
            store.ObservableFor(state => state.Y).Subscribe(_ => notified++ );
            disposable.Dispose();

            store.Dispatch(new EmptyAction());

            Assert.AreEqual(1, notified);
        }
    }
}