using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;

namespace ChainPattern.Tests
{
    public class ChainDelayTests
    {
        //NOTE: FastForward is deleted
        //[Test]
        //public void Start_WithFastForward_CompletesImmediately() {
        //    bool completed = false;
        //    bool skipped = false;
        //    var chain = new ChainDelay(10f);
        //    var context = new OneShotChainContext(_=> completed = true, _ => skipped = true);
        //    chain.Start(context);
        //    Assert.IsTrue(completed);
        //    Assert.IsFalse(skipped);
        //}

        [Test]
        public void Start_DoesNotCompleteImmediately() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainDelay(1.0f);
            var context = new OneShotChainContext(_ => completed = true, _ => skipped = true);
            chain.StartWithCallback(context);
            Assert.IsFalse(completed);
            chain.Skip();
            Assert.IsTrue(skipped);
        }

        [Test]
        public void Skip_AfterStart_DoesNotInvokeCompleteCallback() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainDelay(10f);
            var context = new OneShotChainContext(_ => completed = true, _ => skipped = true);
            chain.StartWithCallback(context);
            chain.Skip();
            Assert.IsFalse(completed);
            Assert.IsTrue(skipped);
        }
    }
}
