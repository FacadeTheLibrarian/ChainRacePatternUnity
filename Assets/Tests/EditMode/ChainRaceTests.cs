using NUnit.Framework;
using ChainPattern;

namespace ChainPattern.Tests
{
    public class ChainRaceTests
    {
        [Test]
        public void EmptyRace_CompletesImmediately() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainRace();
            ChainContext context = new ChainContext(_ => { completed = true; }, _ => { skipped = true; });
            chain.StartWithCallback(context);
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void SingleNop_CompletesImmediately() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainRace(new ChainImmidiateComplete());
            ChainContext context = new ChainContext(_ => { completed = true; }, _ => { skipped = true; });
            chain.StartWithCallback(context);
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void FirstToCompleteWins_OthersAreSkipped() {
            bool completed = false;
            bool skipped = false;
            bool workSkipCalled = false;
            var work = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => workSkipCalled = true));

            var chain = new ChainRace(new ChainImmidiateComplete(), work);
            ChainContext context = new ChainContext(_ => { completed = true; }, _ => { skipped = true; });
            chain.StartWithCallback(context);

            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
            Assert.IsTrue(workSkipCalled);
        }

        [Test]
        public void WaitsForFirstCompletion() {
            bool completed = false;
            bool skipped = false;
            var work1 = new ChainWork(new ChainWorkLifeCycleMock());
            var work2 = new ChainWork(new ChainWorkLifeCycleMock());
            var chain = new ChainRace(work1, work2);
            ChainContext context = new ChainContext(_ => { completed = true; }, _ => { skipped = true; });
            chain.StartWithCallback(context);
            Assert.IsFalse(completed);
            Assert.IsFalse(skipped);
            work1.End();
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void WhenFirstCompletes_RemainingAreSkipped() {
            bool work1SkipCalled = false;
            bool work2SkipCalled = false;
            bool work3SkipCalled = false;
            var work1 = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => work1SkipCalled = true));
            var work2 = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => work2SkipCalled = true));
            var work3 = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => work3SkipCalled = true));

            var chain = new ChainRace(work1, work2, work3);
            ChainContext context = new ChainContext();
            chain.StartWithCallback(context);
            work1.End();

            Assert.IsFalse(work1SkipCalled);
            Assert.IsTrue(work2SkipCalled);
            Assert.IsTrue(work3SkipCalled);
        }
        [Test]
        public void WhenSecondCompletes_RemainingAreSkipped() {
            bool work1SkipCalled = false;
            bool work2SkipCalled = false;
            bool work3SkipCalled = false;
            var work1 = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => work1SkipCalled = true));
            var work2 = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => work2SkipCalled = true));
            var work3 = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => work3SkipCalled = true));

            var chain = new ChainRace(work1, work2, work3);
            ChainContext context = new ChainContext();
            chain.StartWithCallback(context);
            work2.End();

            Assert.IsTrue(work1SkipCalled);
            Assert.IsFalse(work2SkipCalled);
            Assert.IsTrue(work3SkipCalled);
        }

        [Test]
        public void Add_ReturnsChainRaceForFluent() {
            var chain = new ChainRace();
            var result = chain.Add(new ChainImmidiateComplete());
            Assert.AreSame(chain, result);
        }

        [Test]
        public void Skip_AfterStart_SkipsAllRunningChains() {
            bool work1SkipCalled = false;
            bool work2SkipCalled = false;
            var work1 = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => work1SkipCalled = true));
            var work2 = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => work2SkipCalled = true));

            var chain = new ChainRace(work1, work2);
            ChainContext context = new ChainContext();
            chain.StartWithCallback(context);
            chain.Skip();

            Assert.IsTrue(work1SkipCalled);
            Assert.IsTrue(work2SkipCalled);
        }

        [Test]
        public void Skip_AfterStart_DoesNotInvokeCompleteCallback() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainRace(new ChainWork(new ChainWorkLifeCycleMock()), new ChainWork(new ChainWorkLifeCycleMock()));
            ChainContext context = new ChainContext(_ => { completed = true; }, _ => { skipped = true; });
            chain.StartWithCallback(context);
            chain.Skip();
            Assert.IsFalse(completed);
            Assert.IsTrue(skipped);
        }

        //NOTE: FastForward is deleted
        //[Test]
        //public void PropagatesFastForwardToChildren() {
        //    bool? childFastForward = null;
        //    var child = new ChainAction(() => childFastForward = true);
        //    var chain = new ChainRace(child);
        //    ChainContext context = new ChainContext();
        //    chain.Start(context);
        //    Assert.AreEqual(true, childFastForward);
        //}
    }
}
