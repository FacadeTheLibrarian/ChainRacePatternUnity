using NUnit.Framework;
using UnityEngine;

namespace ChainPattern.Tests {
    public class ChainWorkTests {

        [Test]
        public void Start_Invokes_OnStartCallback() {
            bool startCalled = false;
            var chain = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => startCalled = true));
            OneShotChainContext context = new OneShotChainContext();
            chain.StartWithCallback(context);
            Assert.IsTrue(startCalled);
        }

        [Test]
        public void Start_DoesNot_CompleteImmediately() {
            bool completed = false;
            var chain = new ChainWork(new ChainWorkLifeCycleMock());
            OneShotChainContext context = new OneShotChainContext(_ => completed = true, _ => { });
            chain.StartWithCallback(context);
            Assert.IsFalse(completed);
            chain.End();
            Assert.IsTrue(completed);
        }

        [Test]
        public void End_CompletesChain() {
            bool completed = false;
            var chain = new ChainWork(new ChainWorkLifeCycleMock());
            OneShotChainContext context = new OneShotChainContext(_ => completed = true, _ => { });
            chain.StartWithCallback(context);
            chain.End();
            Assert.IsTrue(completed);
        }

        [Test]
        public void End_BeforeStart_DoesNotComplete() {
            bool completed = false;
            var chain = new ChainWork(new ChainWorkLifeCycleMock());
            // NOTE: No context given to the chain, so it shouldn't complete, it doesn't know about the callback.
            chain.End();
            Assert.IsFalse(completed);
        }

        [Test]
        public void Skip_AfterStart_InvokesOnSkip() {
            bool chainCompleted = false;
            bool chainSkipped = false;
            bool skipCalledInternally = false;
            var chain = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => skipCalledInternally = true));
            OneShotChainContext context = new OneShotChainContext(_ => { chainCompleted = true; }, _ => { chainSkipped = true; });
            chain.StartWithCallback(context);
            chain.Skip();
            Assert.IsFalse(chainCompleted);
            Assert.IsTrue(chainSkipped);
            Assert.IsTrue(skipCalledInternally);
        }

        [Test]
        public void Skip_AfterStart_DoesNotInvoke_CompleteCallback() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainWork(new ChainWorkLifeCycleMock());
            OneShotChainContext context = new OneShotChainContext(_ => completed = true, _ => skipped = true);
            chain.StartWithCallback(context);
            chain.Skip();
            Assert.IsFalse(completed);
            Assert.IsTrue(skipped);
        }

        [Test]
        public void End_AfterSkip_DoesNotComplete() {
            bool completed = false;
            bool skipped = false;
            var chain = new ChainWork(new ChainWorkLifeCycleMock());
            OneShotChainContext context = new OneShotChainContext(_ => { completed = true; }, _ => { skipped = true; });
            chain.StartWithCallback(context);
            chain.Skip();
            chain.End();
            Assert.IsFalse(completed);
            Assert.IsTrue(skipped);
        }

        // NOTE: Fastforward property is deleted, so this test is no longer relevant. If we want to reintroduce it, we can add it back and then re-enable this test.
        //[Test]
        //public void IsWorkFastForward_ReflectsSetValue() {
        //    var chain = new ChainWork();
        //    chain.SetIsFastForward(true);
        //    Assert.IsTrue(chain.isWorkFastForward);
        //}
    }
}
