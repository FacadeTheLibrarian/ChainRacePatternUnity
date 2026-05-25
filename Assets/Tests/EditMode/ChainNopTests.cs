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
            var chain = new ChainImmediateComplete();
            var context = new OneShotChainContext(_ => completed = true, _ => skipped = true);
            chain.StartWithCallback(context);
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void Skip_BeforeStart_DoesNotThrow() {
            var chain = new ChainImmediateComplete();
            Assert.DoesNotThrow(() => chain.Skip());
        }

        [Test]
        public void Start_CalledTwice_DoesNotThrow() {
            var chain = new ChainImmediateComplete();
            chain.Start(System.Threading.CancellationToken.None);
            Assert.DoesNotThrow(() => chain.Start(System.Threading.CancellationToken.None));
        }

        [Test]
        public void CompleteCallback_CalledExactlyOnce() {
            int callCount = 0;
            var chain = new ChainImmediateComplete();
            var context = new OneShotChainContext(_ => callCount++, _ => { });
            chain.StartWithCallback(context);
            Assert.AreEqual(1, callCount);
        }
    }
}
