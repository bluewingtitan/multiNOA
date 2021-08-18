using MultiNOA.Middleware;
using NUnit.Framework;

namespace MultiNOA.Tests
{
    public class LoggingTest
    {

        [SetUp]
        public void Setup()
        {
            MultiNoaLoggingManager.TestMsg();
        }
        
        [Test]
        public void Log()
        {
            Assert.AreEqual(2,2);
        }
    }
}