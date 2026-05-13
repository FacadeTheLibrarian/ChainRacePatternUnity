using System.Collections.Generic;
using NUnit.Framework;
using ChainPattern;

namespace ChainPattern.Tests
{
    public class ChainSequenceTests
    {
        [Test]
        public void EmptySequence_CompletesImmediately() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainSequence();
            ChainContext context = new ChainContext(_ => { completed = true; }, _ => { skipped = true; });
            chain.Start(context);
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void SingleNop_CompletesImmediately() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainSequence(new ChainImmidiateComplete());
            ChainContext context = new ChainContext(_ => { completed = true; }, _ => { skipped = true; });
            chain.Start(context);
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void MultipleNops_CompletesImmediately() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainSequence(new ChainImmidiateComplete(), new ChainImmidiateComplete(), new ChainImmidiateComplete());
            ChainContext context = new ChainContext(_ => { completed = true; }, _ => { skipped = true; });
            chain.Start(context);
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void ChainsExecuteInOrder() {
            var order = new List<int>();
            var chain = new ChainSequence(
                new ChainAction(() => order.Add(1)),
                new ChainAction(() => order.Add(2)),
                new ChainAction(() => order.Add(3))
            );
            ChainContext context = new ChainContext();
            chain.Start(context);
            Assert.AreEqual(new[] { 1, 2, 3 }, order.ToArray());
        }

        [Test]
        public void Add_BeforeStart_AddsToSequence_InOrder() {
            var order = new List<int>();
            var chain = new ChainSequence();
            chain.Add(new ChainAction(() => order.Add(1)));
            chain.Add(new ChainAction(() => order.Add(2)));
            ChainContext context = new ChainContext();
            chain.Start(context);
            Assert.AreEqual(new[] { 1, 2 }, order.ToArray());
        }

        [Test]
        public void Add_BeforeStart_AddsToSequence_WithOneChainAlready() {
            var order = new List<int>();
            var chain = new ChainSequence(new ChainAction(() => { order.Add(1); }));
            chain.Add(new ChainAction(() => order.Add(2)));
            chain.Add(new ChainAction(() => order.Add(3)));
            ChainContext context = new ChainContext();
            chain.Start(context);
            Assert.AreEqual(new[] { 1, 2, 3 }, order.ToArray());
        }

        [Test]
        public void Add_ReturnsChainSequenceForFluent() {
            var chain = new ChainSequence();
            var result = chain.Add(new ChainImmidiateComplete());
            Assert.AreSame(chain, result);
        }

        [Test]
        public void WaitsForChainToComplete_BeforeStartingNext() {
            bool firstStarted = false;
            bool secondStarted = false;
            var work = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => { firstStarted = true; }));
            var chain = new ChainSequence(work, new ChainAction(() => secondStarted = true));
            ChainContext context = new ChainContext();
            chain.Start(context);
            Assert.IsTrue(firstStarted);
            Assert.IsFalse(secondStarted);
            work.End();
            Assert.IsTrue(secondStarted);
        }

        [Test]
        public void Skip_AfterStart_DoesNotInvoke_CompleteCallback() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainSequence(new ChainFreeze());
            ChainContext context = new ChainContext(_ => completed = true, _ => skipped = true);
            chain.Start(context);
            chain.Skip();
            Assert.IsFalse(completed);
            Assert.IsTrue(skipped);
        }

        // NOTE: Fastforward is deleted
        //[Test]
        //public void PropagatesFastForwardToChildren() {
        //    bool? childFastForward = null;
        //    var child = new ChainAction(() => childFastForward = true);
        //    var chain = new ChainSequence(child);
        //    ChainContext context = new ChainContext();
        //    chain.Start(context);
        //    Assert.AreEqual(true, childFastForward);
        //}
    }
}
