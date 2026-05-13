using NUnit.Framework;
using ChainPattern;

namespace ChainPattern.Tests
{
    public class ChainActionTests
    {
        [Test]
        public void Start_InvokesAction() {
            bool called = false;
            var chain = new ChainAction(() => called = true);
            chain.StartIgnoreCallback();
            Assert.IsTrue(called);
        }

        [Test]
        public void Start_CompletesAfterAction() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainAction(() => { });
            var context = new ChainContext(_ => completed = true, _ => skipped = true);
            chain.Start(context);
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        [Test]
        public void Start_WithNullAction_StillCompletes() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainAction();
            var context = new ChainContext(_ => completed = true, _ => skipped = true);
            chain.Start(context);
            Assert.IsTrue(completed);
            Assert.IsFalse(skipped);
        }

        //NOTE: FastForward is deleted
        //[Test]
        //public void Start_WithFastForwardAction_ReceivesTrueWhenFastForward() {
        //    bool? received = null;
        //    var chain = new ChainAction((ff) => received = ff);
        //    chain.SetIsFastForward(true);
        //    chain.Start();
        //    Assert.AreEqual(true, received);
        //}

        //NOTE: FastForward is deleted
        //[Test]
        //public void Start_WithFastForwardAction_ReceivesFalseWhenNotFastForward() {
        //    bool? received = null;
        //    var chain = new ChainAction((ff) => received = ff);
        //    chain.Start();
        //    Assert.AreEqual(false, received);
        //}

        [Test]
        public void SetAction_ReplacesActionSetByConstructor() {
            bool firstCalled = false;
            bool secondCalled = false;
            var chain = new ChainAction(() => firstCalled = true);
            chain.SetAction(() => secondCalled = true);
            chain.StartIgnoreCallback();
            Assert.IsFalse(firstCalled);
            Assert.IsTrue(secondCalled);
        }

        [Test]
        public void Skip_BeforeStart_DoesNotThrow() {
            var chain = new ChainAction(() => { });
            Assert.DoesNotThrow(() => chain.Skip());
        }
    }
}
