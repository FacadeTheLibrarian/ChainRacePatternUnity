using NUnit.Framework;

namespace ChainPattern.Tests {

    public class ChainCompositeTests {
        [Test]
        public void Race_Then_Race_Sequence() {
            bool work1SkipCalled = false;
            bool work1Started = false;
            bool work2SkipCalled = false;
            bool work2Started = false;
            bool work3SkipCalled = false;
            bool work3Started = false;
            bool work4SkipCalled = false;
            bool work4Started = false;

            var work1 = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => work1Started = true, onSkip: () => work1SkipCalled = true));
            var work2 = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => work2Started = true, onSkip: () => work2SkipCalled = true));

            var work3 = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => work3Started = true, onSkip: () => work3SkipCalled = true));
            var work4 = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => work4Started = true, onSkip: () => work4SkipCalled = true));

            var chain = new ChainSequence(
                new ChainRace(
                        work1,
                        work2
                ),
                new ChainRace(
                        work3,
                        work4
                )
            );
            OneShotChainContext context = new OneShotChainContext();
            chain.StartWithCallback(context);

            work1.End();
            Assert.IsTrue(work1Started);
            Assert.IsFalse(work1SkipCalled);
            Assert.IsTrue(work2Started);
            Assert.IsTrue(work2SkipCalled);
            work3.Skip();
            Assert.IsTrue(work3Started);
            Assert.IsTrue(work3SkipCalled);
            Assert.IsTrue(work4Started);
            Assert.IsTrue(work4SkipCalled);
        }

        [Test]
        public void Race_Two_Parallels() {
            bool work1End = false;
            bool work2SkipCalled = false;
            bool work2Started = false;
            bool work4SkipCalled = false;
            bool work4Started = false;
            var work1 = new ChainAction(() => { work1End = true; });
            var work2 = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => work2Started = true, onSkip: () => work2SkipCalled = true));
            var work3 = new ChainImmediateComplete();
            var work4 = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => work4Started = true, onSkip: () => work4SkipCalled = true));
            var chain = new ChainRace(
                new ChainParallel(
                        work1,
                        work2
                ),
                new ChainParallel(
                        work3,
                        work4
                )
            );
            OneShotChainContext context = new OneShotChainContext();
            chain.StartWithCallback(context);
            Assert.IsTrue(work1End);
            Assert.IsTrue(work2Started);
            Assert.IsFalse(work2SkipCalled);
            Assert.IsTrue(work4Started);
            work2.End();
            Assert.IsTrue(work4SkipCalled);
        }

        // Tests written by Gemini
        [Test]
        public void SequenceInRace_CancelsRunningSequence() {
            bool seq1Started = false;
            bool seq1SkipCalled = false;
            bool seq2Started = false;
            bool raceOpponentStarted = false;

            var seqWork1 = new ChainWork(new ChainWorkLifeCycleMock(
                onStart: () => seq1Started = true,
                onSkip: () => seq1SkipCalled = true
            ));
            var seqWork2 = new ChainWork(new ChainWorkLifeCycleMock(
                onStart: () => seq2Started = true
            ));
            var sequence = new ChainSequence(seqWork1, seqWork2);

            var raceOpponent = new ChainWork(new ChainWorkLifeCycleMock(
                onStart: () => raceOpponentStarted = true
            ));

            var chain = new ChainRace(sequence, raceOpponent);
            OneShotChainContext context = new OneShotChainContext();
            chain.StartWithCallback(context);

            Assert.IsTrue(seq1Started);
            Assert.IsFalse(seq2Started);
            Assert.IsTrue(raceOpponentStarted);

            // Raceの相手が先に勝つ
            raceOpponent.End();

            // Sequenceの中の実行中だったものがSkipされ、次のはStartされないこと
            Assert.IsTrue(seq1SkipCalled);
            Assert.IsFalse(seq2Started);
        }

        [Test]
        public void RaceInSequence_ContinuesAfterWinnerIsDetermined() {
            bool raceAStarted = false;
            bool raceASkipCalled = false;
            bool raceBStarted = false;
            bool seqNextStarted = false;

            var raceWorkA = new ChainWork(new ChainWorkLifeCycleMock(
                onStart: () => raceAStarted = true,
                onSkip: () => raceASkipCalled = true
            ));
            var raceWorkB = new ChainWork(new ChainWorkLifeCycleMock(
                onStart: () => raceBStarted = true
            ));
            var seqNextWork = new ChainWork(new ChainWorkLifeCycleMock(
                onStart: () => seqNextStarted = true
            ));

            var race = new ChainRace(raceWorkA, raceWorkB);
            var sequence = new ChainSequence(race, seqNextWork);
            OneShotChainContext context = new OneShotChainContext();

            sequence.StartWithCallback(context);

            Assert.IsTrue(raceAStarted);
            Assert.IsTrue(raceBStarted);
            Assert.IsFalse(seqNextStarted);

            // Bが勝つ
            raceWorkB.End();

            // AがSkipされ、次のSequenceが開始されること
            Assert.IsTrue(raceASkipCalled);
            Assert.IsTrue(seqNextStarted);
        }

        [Test]
        public void SequenceInParallel_CompletesWhenAllSequencesEnd() {
            bool seq1BStarted = false;
            bool seq2BStarted = false;
            bool parallelCompleted = false;

            var seq1A = new ChainWork(new ChainWorkLifeCycleMock());
            var seq1B = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => seq1BStarted = true));
            var seq2A = new ChainWork(new ChainWorkLifeCycleMock());
            var seq2B = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => seq2BStarted = true));

            var sequence1 = new ChainSequence(seq1A, seq1B);
            var sequence2 = new ChainSequence(seq2A, seq2B);

            var parallel = new ChainParallel(sequence1, sequence2);
            OneShotChainContext context = new OneShotChainContext((_) => parallelCompleted = true, null);

            parallel.StartWithCallback(context);

            Assert.IsFalse(seq1BStarted);
            Assert.IsFalse(seq2BStarted);

            // Sequence1のAを終わらせる
            seq1A.End();
            Assert.IsTrue(seq1BStarted);
            Assert.IsFalse(parallelCompleted);

            // Sequence2のA, Bを終わらせる
            seq2A.End();
            Assert.IsTrue(seq2BStarted);
            seq2B.End();
            Assert.IsFalse(parallelCompleted); // まだseq1Bが終わっていない

            // 最後にseq1Bを終わらせると全て完了
            seq1B.End();
            Assert.IsTrue(parallelCompleted);
        }

        [Test]
        public void DeepNestingSkipPropagation_SkipsRunningDoesNotStartPending() {
            bool bStarted = false, bSkipped = false;
            bool cStarted = false, cSkipped = false;
            bool dStarted = false, dSkipped = false;
            bool eStarted = false;

            var a = new ChainImmediateComplete(); // Synchronous to proceed to next instantly
            var b = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => bStarted = true, onSkip: () => bSkipped = true));
            var c = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => cStarted = true, onSkip: () => cSkipped = true));
            var d = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => dStarted = true, onSkip: () => dSkipped = true));
            var e = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => eStarted = true));

            var deepSequence = new ChainSequence(d, e);
            var race = new ChainRace(c, deepSequence);
            var parallel = new ChainParallel(b, race);
            var rootSequence = new ChainSequence(a, parallel);

            OneShotChainContext context = new OneShotChainContext();
            rootSequence.StartWithCallback(context);

            // B, C, D are running in parallel/race
            Assert.IsTrue(bStarted);
            Assert.IsTrue(cStarted);
            Assert.IsTrue(dStarted);
            Assert.IsFalse(eStarted);

            // Cancel the entire root
            rootSequence.Skip();

            // Running tasks should be skipped. Pending task (E) should not start.
            Assert.IsTrue(bSkipped);
            Assert.IsTrue(cSkipped);
            Assert.IsTrue(dSkipped);
            Assert.IsFalse(eStarted);
        }

        [Test]
        public void SynchronousEmptyChainResolution_DoesNotStackOverflow() {
            bool completed = false;
            var chain = new ChainSequence(
                new ChainRace(), 
                new ChainParallel(), 
                new ChainSequence()
            );

            OneShotChainContext context = new OneShotChainContext((_) => completed = true, null);
            chain.StartWithCallback(context);

            Assert.IsTrue(completed);
        }
    }
}