using NUnit.Framework;
using ChainPattern;

namespace ChainPattern.Tests
{
    public class ChainHaltTests
    {
        [Test]
        public void Start_DoesNotComplete() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainFreeze();
            var context = new OneShotChainContext(_ => completed = true, _ => skipped = true);
            chain.StartWithCallback(context);
            Assert.IsFalse(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void Skip_BeforeStart_DoesNotThrow() {
            var chain = new ChainFreeze();
            Assert.DoesNotThrow(() => chain.Skip());
        }

        [Test]
        public void Skip_AfterStart_DoesNotInvokeCompleteCallback() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainFreeze();
            var context = new OneShotChainContext(_ => completed = true, _ => skipped = true);
            chain.StartWithCallback(context);
            chain.Skip();
            Assert.IsFalse(completed);
            Assert.IsTrue(skipped);
        }

        [Test]
        public void Skip_AfterStart_DoesNotThrow() {
            var chain = new ChainFreeze();
            chain.Start(System.Threading.CancellationToken.None);
            Assert.DoesNotThrow(() => chain.Skip());
        }
    }
}
