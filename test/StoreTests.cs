using NUnit.Framework;

namespace AReSSO.Test
{
    public class StoreTests
    {
        [Test]
        public void GetStateOnNewStoreReturnsInitialState()
        {
            SimpleTestStateNode init = new SimpleTestStateNode(42);
            var store = new Store(init);

            Assert.AreEqual(init, store.State);
        }
    }
}