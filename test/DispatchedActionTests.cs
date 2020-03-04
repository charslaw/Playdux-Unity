using System;
using System.Diagnostics;
using AReSSO.Store;
using NUnit.Framework;

namespace AReSSO.Test
{
    public class DispatchedActionTests
    {
        private class TestAction : IAction {}
        
        [Test]
        public void HasCorrectDispatchedTime()
        {
            var now = DateTime.Now;
            var da = new DispatchedAction(new TestAction());
            
            Assert.That(da.DispatchTime, Is.EqualTo(now).Within(TimeSpan.FromMilliseconds(10)));
        }

        [Test]
        public void HasCorrectStackFrame()
        {
           var da = new DispatchedAction(new TestAction());
           var thisMethodInfo = new StackTrace().GetFrame(0).GetMethod();
           
           Assert.AreEqual(thisMethodInfo, da.DispatchStackTrace.GetFrame(0).GetMethod());
        }
    }
}