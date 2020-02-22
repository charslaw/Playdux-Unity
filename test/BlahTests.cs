using NUnit.Framework;

namespace AReSSO.Tests
{
    public class BlahTests
    {
        [Test]
        public void AddWorks()
        {
            var blah = new Blah();
            Assert.AreEqual(5, blah.Add(2, 3));
        }
    }
}