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
            Assert.AreEqual(newState with { }, simpleStore.State, "State was not modified by InitializeAction");
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
            Assert.AreEqual(init with { }, simpleStore.State, "State was modified by InitializeAction");
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
            Assert.AreEqual(1, executeCount, "Pre side effect did not trigger");
        }

        [Test]
        public void PostSideEffectorCanProduceSideEffects()
        {
            SimpleTestState init = new(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var executeCount = 0;
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(post: (_, _) => executeCount++));

            SimpleTestState newState = new(10);
            simpleStore.Dispatch(new InitializeAction<SimpleTestState>(newState));

            BlockingWait(Delay);
            Assert.AreEqual(1, executeCount, "Post side effect did not trigger");
        }

        [Test, Timeout(1000)]
        public void PreSideEffectorCanInterceptAndInjectSeparateAction()
        {
            var init = new SimpleTestState(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.AcceptAddSimpleTestStateReducer);

            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (dispatchedAction, dispatcher) =>
                {
                    var action = dispatchedAction.Action;
                    if (action is SimpleStateAddAction (var value))
                    {
                        dispatcher.Dispatch(new BetterSimpleStateAddAction(value));
                        return false;
                    }

                    return true;
                }
            ));

            simpleStore.Dispatch(new SimpleStateAddAction(5));

            BlockingWait(Delay);
            Assert.AreNotEqual(5, simpleStore.State.N, "Effect of SimpleStateAddAction was applied when it shouldn't have been");
            Assert.AreEqual(6, simpleStore.State.N, "Effect of BetterSimpleStateAddAction was not applied");
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

            Assert.AreEqual(0, executeCount, "Unregistered side effector was called");
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
            Assert.IsTrue(secondCalled, "The second side effector was prevented from being called");
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
            CollectionAssert.AreEqual(Enumerable.Range(0, 3), order, $"Side effectors were not called in order of registration: [ {string.Join(",", order)} ]");
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
            CollectionAssert.AreEqual(Enumerable.Range(0, 2), order, $"Side effectors were not called in priority order: [ {string.Join(",", order)} ]");
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
            CollectionAssert.AreEqual(Enumerable.Range(0, 4), order, $"Side effectors were not inserted in the correct order: [ {string.Join(",", order)} ]");
        }
    }
}