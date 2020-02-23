using NUnit.Framework;

namespace AReSSO.Test
{
    public class StateNodeTests
    {
        [Test, Repeat(10)]
        public void EachStateNodeGetsUniqueId()
        {
            var node1 = new SimpleTestStateNode(1);
            var node2 = new SimpleTestStateNode(1); // even if their values are the same
            
            Assert.AreNotEqual(node1.Id, node2.Id);
        }

        [Test]
        public void ProtectedCopyInstructorCopiesId()
        {
            var oldState = new SimpleTestStateNode(12);
            var oldId = oldState.Id;

            var newState = oldState.Copy();
            
            Assert.AreEqual(oldId, newState.Id, "Direct ID comparison");
            Assert.That(oldState.EqualsById(newState), "ID comparison via EqualsById");
        }

        [Test]
        public void CopyWithDeltaChangesProp()
        {
            var state = new SimpleTestStateNode(42);

            var newState = state.Copy(prop: 100);
            
            Assert.AreNotEqual(state.Prop, newState.Prop);
        }
    }
}