#nullable enable
using System;
using NUnit.Framework;
using Playdux.src.Store;
using UniRx;

namespace Playdux.test
{
    public class StoreErrorHandlingTests
    {
        private static SimpleTestState MethodThatThrows(SimpleTestState _) => throw new Exception();

        private Store<SimpleTestState>? simpleStore;

        [TearDown]
        public void Teardown() { simpleStore?.Dispose(); }

        [Test]
        public void ErrorInSubscribeDoesNotBreakOtherObservers()
        {
            SimpleTestState init = new(42);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IncrementNSimpleTestStateReducer);

            int notified = 0;
            int errorSeen = 0;
            simpleStore.ObservableFor(state => state)
                .Subscribe(
                    onNext: _ => throw new Exception(),
                    onError: _ => { });
            simpleStore.ObservableFor(state => state)
                .Subscribe(
                    onNext: _ => notified++,
                    onError: _ => errorSeen++);

            simpleStore.Dispatch(new EmptyAction());
            simpleStore.Dispatch(new EmptyAction());

            Assert.AreEqual(0, errorSeen, $"Saw {errorSeen} errors");
            Assert.AreEqual(2, notified, $"Saw {notified} notifications");
        }

        [Test]
        public void ErrorInStreamDoesNotBreakOtherObservers()
        {
            SimpleTestState init = new(42);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IncrementNSimpleTestStateReducer);

            int notified = 0;
            int errorSeen = 0;
            simpleStore.ObservableFor(state => state)
                .Subscribe(
                    onNext: _ => notified++,
                    onError: _ => errorSeen++);
            simpleStore.ObservableFor(state => state)
                .Select(MethodThatThrows)
                .Subscribe(
                    onNext: _ => { },
                    onError: _ => { }
                );

            simpleStore.Dispatch(new EmptyAction());
            simpleStore.Dispatch(new EmptyAction());

            Assert.AreEqual(0, errorSeen, $"Saw {errorSeen} errors");
            Assert.AreEqual(2, notified, $"Saw {notified} notifications");
        }

        [Test]
        public void ErrorInSelectorDoesNotBreakOtherObservers()
        {
            SimpleTestState init = new(42);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IncrementNSimpleTestStateReducer);

            int notified = 0;
            int errors = 0;
            simpleStore.ObservableFor(state => state)
                .Subscribe(
                    onNext: _ => notified++,
                    onError: _ => errors++);
            simpleStore.ObservableFor(TestSelectors.ErrorSimpleTestStateSelector)
                .Subscribe(
                    onNext: _ => { },
                    onError: _ => { });

            simpleStore.Dispatch(new EmptyAction());
            simpleStore.Dispatch(new EmptyAction());

            Assert.AreEqual(0, errors, $"Saw {errors} errors");
            Assert.AreEqual(2, notified, $"Saw {notified} notifications");
        }
    }
}