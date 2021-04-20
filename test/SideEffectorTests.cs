#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Playdux.src.Store;
using UnityEngine.TestTools;

namespace Playdux.test
{
    public class SideEffectorTests
    {
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

            Assert.AreEqual(init with { }, simpleStore.State, "State was modified by InitializeAction");
        }

        [Test]
        public void PostSideEffectorGetsUpdatedStateFromAction()
        {
            SimpleTestState init = new(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            int? actualValue = null;
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(post: (_, store) => { actualValue = store.State.N; }));

            SimpleTestState newState = new(10);
            simpleStore.Dispatch(new InitializeAction<SimpleTestState>(newState));

            Assert.AreEqual(10, actualValue, "Side effector did not get new state created from dispatched action");
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

            Assert.AreEqual(1, executeCount, "Post side effect did not trigger");
        }

        [Test]
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

            Assert.AreNotEqual(5, simpleStore.State.N, "Effect of SimpleStateAddAction was applied when it shouldn't have been");
            Assert.AreEqual(6, simpleStore.State.N, "Effect of BetterSimpleStateAddAction was not applied");
        }

        [Test]
        public void PreSideEffectorInjectedActionWaitsForInitialActionCompletion()
        {
            var init = new SimpleTestState(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var firstRun = true;
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(pre: (_, store) =>
                {
                    if (!firstRun) return true;

                    firstRun = false;
                    store.Dispatch(new SimpleStateAddAction(7));

                    return true;
                }
            ));

            var order = new List<int>();
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>(post: (dispatchedAction, _) =>
                {
                    var actionAsSimpleAdd = dispatchedAction.Action as SimpleStateAddAction;

                    order.Add(actionAsSimpleAdd!.Value);
                }
            ));

            simpleStore.Dispatch(new SimpleStateAddAction(13));

            CollectionAssert.AreEqual(new[] { 13, 7 }, order, $"Actions were not received in correct order: [ {string.Join(",", order)} ]");
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

            CollectionAssert.AreEqual(Enumerable.Range(0, 4), order, $"Side effectors were not inserted in the correct order: [ {string.Join(",", order)} ]");
        }

        [Test]
        public void UnregisteringSideEffectorRemovesCorrectSideEffector()
        {
            var init = new SimpleTestState(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            var firstCalled = false;
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>((_, _) => {
                firstCalled = true;
                return true;
            }, priority: 0));
            
            var secondCalled = false;
            var secondID = simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>((_, _) => {
                secondCalled = true;
                return true;
            }, priority: 0));
            
            simpleStore.UnregisterSideEffector(secondID);
            
            simpleStore.Dispatch(new EmptyAction());
            
            Assert.AreEqual(false, secondCalled, "Second side effector was called despite being unregistered!");
            Assert.AreEqual(true, firstCalled, "First was not called despite remaining registered");
        }

        [Test]
        public void SideEffectorThrowingDoesNotPreventExecutionOfOtherSideEffectors()
        {
            // Tell unity to ignore the Debug.LogError and Debug.LogException that will happen when a side effector throws an error
            LogAssert.ignoreFailingMessages = true;

            var init = new SimpleTestState(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);
            
            simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>((_, _) => throw new Exception()));
            
            var secondCalled = false;
            var secondID = simpleStore.RegisterSideEffector(new TestSideEffectors.FakeSideEffector<SimpleTestState>((_, _) => {
                secondCalled = true;
                return true;
            }));
            
            simpleStore.UnregisterSideEffector(secondID);
            
            simpleStore.Dispatch(new EmptyAction());
            
            Assert.AreEqual(false, secondCalled, "Second side effector was not called after the first threw an exception.");
        }

        [Test]
        public void UnregisteringNonexistantSideEffectorThrows()
        {
            var init = new SimpleTestState(0);
            simpleStore = new Store<SimpleTestState>(init, TestReducers.IdentitySimpleTestStateReducer);

            Assert.Throws<ArgumentException>(() => simpleStore.UnregisterSideEffector(Guid.NewGuid()));
        }
    }
}