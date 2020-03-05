using AReSSO.Store;
using NUnit.Framework;
using UniRx;

namespace AReSSO.Test
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
        public void CanUnsubscribeWithoutBreakingEverything()
        {
            Point init = new Point(4, 2);
            var store = new Store<Point>(init, TestReducers.IncrementYPointReducer);
            
            var disposable = store.ObservableFor(state => state.Y).Subscribe(_ => { });
            disposable.Dispose();
            
            Assert.DoesNotThrow(() => store.Dispatch(new EmptyAction()));
        }
    }
}