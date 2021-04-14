#nullable enable
using NUnit.Framework;
using Playdux.src.Store;
using UniRx;
using static Playdux.test.TestUtils.TestUtils;

namespace Playdux.test
{
    public class StoreTests
    {
        private const int Delay = 1;
        
        private Store<SimpleTestState>? simpleStore;
        private Store<Point>? pointStore;

        [TearDown]
        public void Teardown()
        {
            simpleStore?.Dispose();
            pointStore?.Dispose();
        }
        
        [Test]
        public void GetStateOnNewStoreReturnsInitialState()
        {
            SimpleTestState init = new(42);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            Assert.AreEqual(init with {}, simpleStore.State, "State does not match initial state.");
        }

        [Test]
        public void StateNotChangedAfterDispatchWhenReducerDoesNotChangeState()
        {
            SimpleTestState init = new(42);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            simpleStore.Dispatch(new EmptyAction());

            BlockingWait(Delay);
            Assert.AreEqual(init, simpleStore.State, "State does not match initial state after applying identity reducer.");
        }

        [Test]
        public void StateChangedAfterDispatchWhenReducerDoesChangeState()
        {
            SimpleTestState init = new(42);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.GenerateSetNSimpleTestStateReducer(243));

            simpleStore.Dispatch(new EmptyAction());

            BlockingWait(Delay);
            Assert.AreEqual(243, simpleStore.State.N, "State does not match expected value produced by reducer");
        }

        [Test]
        public void InitializeActionSetsState()
        {
            Point init = new(0, 1);
            pointStore = new Store<Point>(init, TestReducers.IdentityPointReducer);

            Point newState = new(10, 11);
            pointStore.Dispatch(new InitializeAction<Point>(newState));

            BlockingWait(Delay);
            Assert.AreEqual(newState with {}, pointStore.State, "State does not match expected value from InitializeAction");
        }

        [Test]
        public void InitializeActionWithWrongStateTypeThrows()
        {
            Point init = new(default, default);
            pointStore = new Store<Point>(init, TestReducers.IdentityPointReducer);
            
            Assert.Throws<InitializeTypeMismatchException>(
                () => pointStore.Dispatch(new InitializeAction<SimpleTestState>(new SimpleTestState(default))),
                "Did not throw InitializeTypeMismatchException when incorrect state type was given."
            );
        }

        [Test]
        public void CanUnsubscribeWithoutBreakingEverything()
        {
            Point init = new(4, 2);
            pointStore = new Store<Point>(init, TestReducers.IncrementYPointReducer);

            var disposable = pointStore.ObservableFor(state => state.Y).Subscribe(_ => { });
            disposable.Dispose();

            Assert.DoesNotThrow(() => pointStore.Dispatch(new EmptyAction()), "Dispatching an action threw after consumer unsubscribed.");
        }
        

        [Test]
        public void CanUnsubscribeWithoutBreakingOtherSubscribers()
        {
            Point init = new(4, 2);
            pointStore = new Store<Point>(init, TestReducers.IncrementYPointReducer);

            var notified = 0;
            var disposable = pointStore.ObservableFor(state => state.Y).Subscribe(_ => { });
            pointStore.ObservableFor(state => state.Y).Subscribe(_ => notified++ );
            disposable.Dispose();

            pointStore.Dispatch(new EmptyAction());

            BlockingWait(Delay);
            Assert.AreEqual(1, notified, "Disposing a subscription broke another subscriber.");
        }
    }
}