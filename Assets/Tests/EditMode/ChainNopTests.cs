using NUnit.Framework;
using ChainPattern;

namespace ChainPattern.Tests
{
    public class ChainNopTests
    {
        [Test]
        public void Start_CompletesImmediately() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainImmidiateComplete();
            var context = new ChainContext(_ => completed = true, _ => skipped = true);
            chain.Start(context);
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void Skip_BeforeStart_DoesNotThrow() {
            var chain = new ChainImmidiateComplete();
            Assert.DoesNotThrow(() => chain.Skip());
        }

        [Test]
        public void Start_CalledTwice_DoesNotThrow() {
            var chain = new ChainImmidiateComplete();
            chain.StartWithoutCallback();
            Assert.DoesNotThrow(() => chain.StartWithoutCallback());
        }

        [Test]
        public void CompleteCallback_CalledExactlyOnce() {
            int callCount = 0;
            var chain = new ChainImmidiateComplete();
            var context = new ChainContext(_ => callCount++, _ => { });
            chain.Start(context);
            Assert.AreEqual(1, callCount);
        }
    }
}
