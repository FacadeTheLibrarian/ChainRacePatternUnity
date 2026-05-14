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

        [UnityTest]
        public IEnumerator Start_DoesNotCompleteImmediately() {
            bool completed = false;
            var chain = new ChainDelay(1.0f);
            var context = new OneShotChainContext(_ => completed = true, _ => { });
            chain.StartWithCallback(context);
            float elapsedTime = 0.0f;
            Assert.IsFalse(completed);
            while (true) {
                elapsedTime += Time.deltaTime;
                if (completed) {
                    break;
                }
                if(elapsedTime >= 20.0f) {
                    break;
                }
                yield return null;
            }
            Assert.IsTrue(completed);
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
