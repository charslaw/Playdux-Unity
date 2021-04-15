#nullable enable
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Playdux.src.Store;
using static Playdux.test.TestUtils.TestUtils;

namespace Playdux.test
{
    public class SideEffectorTests
    {
        private const int Delay = 5;

        private Store<SimpleTestState>? simpleStore;

        [TearDown]
        public void Teardown() { simpleStore?.Dispose(); }

        [Test]
        public void IdentitySideEffectorHasNoEffect()
        {
            SimpleTestState init = new(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);
            simpleStore.RegisterSideEffector(new TestSideEffectors.DoesNothingSideEffector<SimpleTestState>());

            SimpleTestState newState = new(10);
            simpleStore.Dispatch(new InitializeAction<SimpleTestState>(newState));

            BlockingWait(Delay);
            Assert.AreEqual(newState with { }, simpleStore.State);
        }

        [Test]
        public void PreventativeSideEffectorPreventsStateChange()
        {
            SimpleTestState init = new(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);
            simpleStore.RegisterSideEffector(new TestSideEffectors.PreventsAllActionsSideEffector<SimpleTestState>());

            SimpleTestState newState = new(10);
            simpleStore.Dispatch(new InitializeAction<SimpleTestState>(newState));

            BlockingWait(Delay);
            Assert.AreEqual(init with { }, simpleStore.State);
        }

        [Test]
        public void PreSideEffectorCanProduceSideEffects()
        {
            SimpleTestState init = new(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var executeCount = 0;
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>((_, _) =>
            {
                executeCount++;
                return true;
            }));

            SimpleTestState newState = new(10);
            simpleStore.Dispatch(new InitializeAction<SimpleTestState>(newState));

            BlockingWait(Delay);
            Assert.AreEqual(1, executeCount);
        }

        [Test]
        public void PostSideEffectorCanProduceSideEffects()
        {
            SimpleTestState init = new(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var executeCount = 0;
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(post: (_, _, _) => executeCount++));

            SimpleTestState newState = new(10);
            simpleStore.Dispatch(new InitializeAction<SimpleTestState>(newState));

            BlockingWait(Delay);
            Assert.AreEqual(1, executeCount);
        }

        [Test, Timeout(1000)]
        public void PreSideEffectorCanInterceptAndInjectSeparateAction()
        {
            var init = new SimpleTestState(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.AcceptAddSimpleTestStateReducer);

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (dispatchedAction, dispatcher) =>
                {
                    var action = dispatchedAction.Action;
                    if (action is SimpleStateAdd (var value))
                    {
                        dispatcher.Dispatch(new BetterSimpleStateAdd(value));
                        return false;
                    }

                    return true;
                }
            ));

            simpleStore.Dispatch(new EmptyAction());
            simpleStore.Dispatch(new SimpleStateAdd(5));

            BlockingWait(Delay);
            Assert.AreEqual(6, simpleStore.State.N);
        }

        [Test]
        public void UnregisteredSideEffectorDoesNotGetCalled()
        {
            var init = new SimpleTestState(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var executeCount = 0;
            var sideEffectorId = simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    executeCount++;
                    return true;
                }
            ));

            simpleStore.UnregisterSideEffector(sideEffectorId);

            simpleStore.Dispatch(new EmptyAction());

            Assert.AreEqual(0, executeCount);
        }

        [Test]
        public void PreSideEffectorCantCancelOtherPreSideEffectors()
        {
            var init = new SimpleTestState(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var secondCalled = false;

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) => false));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    secondCalled = true;
                    return true;
                }
            ));

            simpleStore.Dispatch(new EmptyAction());

            BlockingWait(Delay);
            Assert.IsTrue(secondCalled);
        }

        [Test]
        public void SideEffectorsWithSamePriorityActivatedInOrderOfAddition()
        {
            var init = new SimpleTestState(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var order = new List<int>();

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(0);
                    return true;
                },
                priority: 0
            ));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(1);
                    return true;
                },
                priority: 0
            ));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(2);
                    return true;
                },
                priority: 0
            ));

            simpleStore.Dispatch(new EmptyAction());

            BlockingWait(Delay);
            CollectionAssert.AreEqual(Enumerable.Range(0, 3), order);
        }

        [Test]
        public void SideEffectorsOccurInPriorityOrder()
        {
            var init = new SimpleTestState(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var order = new List<int>();

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(1);
                    return true;
                },
                priority: 0
            ));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(0);
                    return true;
                },
                priority: 1
            ));

            simpleStore.Dispatch(new EmptyAction());

            BlockingWait(Delay);
            CollectionAssert.AreEqual(Enumerable.Range(0, 2), order);
        }

        [Test]
        public void SideEffectorInsertOrderIsCorrect()
        {
            var init = new SimpleTestState(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var order = new List<int>();

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(0);
                    return true;
                },
                priority: 0
            ));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(3);
                    return true;
                },
                priority: -1
            ));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(1);
                    return true;
                },
                priority: 0
            ));

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, _) =>
                {
                    order.Add(2);
                    return true;
                },
                priority: 0
            ));

            simpleStore.Dispatch(new EmptyAction());

            BlockingWait(Delay);
            CollectionAssert.AreEqual(Enumerable.Range(0, 4), order);
        }
    }
}