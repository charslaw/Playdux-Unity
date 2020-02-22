using NUnit.Framework;

namespace AReSSO.Test
{
    public class StoreTests
    {
        [Test]
        public void GetStateOnNewStoreReturnsInitialState()
        {
            TestStateNode init = new TestStateNode(42);
            var store = new Store(init);

            Assert.AreEqual(init, store.State);
        }
    }
}