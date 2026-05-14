using NUnit.Framework;
using ChainPattern;

namespace ChainPattern.Tests
{
    public class ChainParallelTests
    {
        [Test]
        public void EmptyParallel_CompletesImmediately() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainParallel();
            var context = new ChainContext(_ => completed = true, _ => skipped = true);
            chain.StartWithCallback(context);
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void AllNops_CompletesImmediately() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainParallel(new ChainImmidiateComplete(), new ChainImmidiateComplete(), new ChainImmidiateComplete());
            var context = new ChainContext(_ => completed = true, _ => skipped = true);
            chain.StartWithCallback(context);
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void WaitsForAllChains_NotCompletedUntilLastDone() {
            bool completed = false;
            bool skipped = false;
            var work = new ChainWork();
            var chain = new ChainParallel(new ChainImmidiateComplete(), work);
            var context = new ChainContext(_ => completed = true, _ => skipped = true);
            chain.StartWithCallback(context);
            Assert.IsFalse(completed);
            work.End();
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void CompletesOnlyWhenAllDone() {
            bool completed = false;
            bool skipped = false;
            var work1 = new ChainWork();
            var work2 = new ChainWork();
            var chain = new ChainParallel(work1, work2);
            var context = new ChainContext(_ => completed = true, _ => skipped = true);
            chain.StartWithCallback(context);
            work1.End();
            Assert.IsFalse(completed);
            work2.End();
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void Add_ReturnsChainParallelForFluent() {
            var chain = new ChainParallel();
            var result = chain.Add(new ChainImmidiateComplete());
            Assert.AreSame(chain, result);
        }

        [Test]
        public void Skip_AfterStart_SkipsAllRunningChains() {
            bool chainComplete = false;
            bool chainSkipped = false;
            bool work1SkipCalled = false;
            bool work2SkipCalled = false;
            var work1 = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => work1SkipCalled = true));
            var work2 = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => work2SkipCalled = true));
            var chain = new ChainParallel(work1, work2);
            var context = new ChainContext(_ => chainComplete = true, _ => chainSkipped = true);
            chain.StartWithCallback(context);
            chain.Skip();

            Assert.IsFalse(chainComplete);
            Assert.IsTrue(chainSkipped);
            Assert.IsTrue(work1SkipCalled);
            Assert.IsTrue(work2SkipCalled);
        }

        [Test]
        public void Skip_AfterStart_DoesNotInvokeCompleteCallback() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainParallel(new ChainWork(), new ChainWork());
            var context = new ChainContext(_ => completed = true, _ => skipped = true);
            chain.StartWithCallback(context);
            chain.Skip();
            Assert.IsFalse(completed);
            Assert.IsTrue(skipped);
        }

        // Fastforward is deleted
        //[Test]
        //public void PropagatesFastForwardToChildren() {
        //    bool? childFastForward = null;
        //    var child = new ChainAction((ff) => childFastForward = ff);
        //    var chain = new ChainParallel(child);
        //    chain.SetIsFastForward(true);
        //    chain.Start();
        //    Assert.AreEqual(true, childFastForward);
        //}
    }
}
