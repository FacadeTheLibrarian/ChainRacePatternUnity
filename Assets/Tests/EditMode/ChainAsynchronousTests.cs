using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
namespace ChainPattern.Tests {
    public class ChainAsynchronousTests {

        [UnityTest]
        public IEnumerator CancellationToken_Should_Throw_Exception() {
            yield return CancellationTokenTest().ToCoroutine();
        }

        private async UniTask CancellationTokenTest() {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            bool isStarted = false;
            bool isSkipped = false;
            bool isCompleted = false;

            var freezeChain1 = new ChainFreeze();
            var workChain1 = new ChainWork(new ChainWorkLifeCycleMock(
                onStart: () => isStarted = true,
                onSkip: () => isSkipped = true
                ));
            var actionChain1 = new ChainAction(() => isCompleted = true);
            var sequence = new ChainSequence(freezeChain1, workChain1, actionChain1);

            UniTask chainTask = sequence.Start(token);
            UniTask cancel = UniTask.Create(() => { cts.Cancel(); return UniTask.CompletedTask; });

            try {
                await UniTask.WhenAll(chainTask, cancel);
            }
            catch {
                UnityEngine.Debug.Log("Exception thrown as expected when cancellation is requested.");
            }
            Assert.IsTrue(token.IsCancellationRequested);
            Assert.IsFalse(isStarted);
            Assert.IsFalse(isSkipped);
            Assert.IsFalse(isCompleted);
        }

        // Written by Gemini
        [UnityTest]
        public IEnumerator AsyncAdd_To_RunningSequence_ExecutesProperly() => UniTask.ToCoroutine(async () => {
            bool firstEnded = false;
            bool addedEnded = false;

            var sequence = new ChainSequence();
            var firstWork = new ChainWork();
            sequence.Add(firstWork);

            var context = new OneShotChainContext();
            var sequenceTask = sequence.StartWithCallback(context);

            await UniTask.Yield(); // 1フレーム待機してfirstWorkを実行状態にする

            // 実行途中で新しいアイテムをAddする
            var addedWork = new ChainWork();
            sequence.Add(addedWork);

            firstWork.End();
            firstEnded = true;

            await UniTask.Yield(); // 次のノードへ移行させる

            addedWork.End();
            addedEnded = true;

            await sequenceTask; // Sequenceの完了を待機

            Assert.IsTrue(firstEnded);
            Assert.IsTrue(addedEnded);
        });

        [UnityTest]
        public IEnumerator AsyncAdd_To_RunningParallel_ExecutesImmidiately() => UniTask.ToCoroutine(async () => {
            bool firstEnded = false;
            bool addedEnded = false;
            bool addedStarted = false;

            var parallel = new ChainParallel();
            var firstWork = new ChainWork();
            parallel.Add(firstWork);

            var context = new OneShotChainContext();
            var parallelTask = parallel.StartWithCallback(context);

            await UniTask.Yield();

            // 実行中のParallelにAddする
            var addedWork = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => addedStarted = true));
            parallel.Add(addedWork);

            // ParallelにAddした場合、その瞬間にStartが呼ばれるはず
            Assert.IsTrue(addedStarted);

            firstWork.End();
            firstEnded = true;

            var isCompleted = parallelTask.Status == UniTaskStatus.Succeeded;
            Assert.IsFalse(isCompleted, "Parallel shouldn't be completed since added work is still running");

            addedWork.End();
            addedEnded = true;

            await parallelTask;

            Assert.IsTrue(firstEnded);
            Assert.IsTrue(addedEnded);
        });

        [UnityTest]
        public IEnumerator AsyncAdd_To_RunningRace_ExecutesImmidiately_AndWins() => UniTask.ToCoroutine(async () => {
            bool firstEnded = false;
            bool firstSkipped = false;
            bool addedEnded = false;
            bool addedStarted = false;

            var race = new ChainRace();
            var firstWork = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => firstSkipped = true));
            race.Add(firstWork);

            var context = new OneShotChainContext();
            var raceTask = race.StartWithCallback(context);

            await UniTask.Yield();

            // 実行中のRaceにAddする
            var addedWork = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => addedStarted = true));
            race.Add(addedWork);

            // RaceにAddした場合、その瞬間にStartが呼ばれるはず
            Assert.IsTrue(addedStarted);

            // 後から追加された方が先に勝つ
            addedWork.End();
            addedEnded = true;

            await raceTask;

            Assert.IsFalse(firstEnded, "First work shouldn't end normally");
            Assert.IsTrue(firstSkipped, "First work should be skipped because added work won");
            Assert.IsTrue(addedEnded);
        });

        [UnityTest]
        public IEnumerator AsyncAdd_To_NestedParallel_UnderSequenceAndRace_ExecutesProperly() => UniTask.ToCoroutine(async () => {
            bool initialParWorkEnded = false;
            bool addedParWorkStarted = false;
            bool addedParWorkEnded = false;
            bool raceOpponentSkipped = false;
            bool seqNextWorkStarted = false;

            // Structure: Sequence( Race( Parallel(initialParWork), raceOpponent ), seqNextWork )

            var initialParWork = new ChainWork();
            var parallel = new ChainParallel(initialParWork);

            var raceOpponent = new ChainWork(new ChainWorkLifeCycleMock(onSkip: () => raceOpponentSkipped = true));
            var race = new ChainRace(parallel, raceOpponent);

            var seqNextWork = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => seqNextWorkStarted = true));
            var sequence = new ChainSequence(race, seqNextWork);

            var context = new OneShotChainContext();
            var sequenceTask = sequence.StartWithCallback(context);

            await UniTask.Yield();
            // At this point, initialParWork and raceOpponent are both running

            // 1. 実行中の深い層にある Parallel に対して動的に Add() を行う
            var addedParWork = new ChainWork(new ChainWorkLifeCycleMock(onStart: () => addedParWorkStarted = true));
            parallel.Add(addedParWork);

            // Parallelに追加された直後にStart()が呼ばれているはず
            Assert.IsTrue(addedParWorkStarted);

            // 2. 元々あった initialParWork を終わらせる
            initialParWork.End();
            initialParWorkEnded = true;

            await UniTask.Yield();

            // この時点では、追加された addedParWork がまだ終わっていないため、
            // Parallel 全体は完了せず、当然 Race も決着していない。なので Sequence の次にも進まない。
            Assert.IsFalse(raceOpponentSkipped);
            Assert.IsFalse(seqNextWorkStarted);

            // 3. 追加された addedParWork を終わらせる -> Parallel 完了 -> Race決着へ
            addedParWork.End();
            addedParWorkEnded = true;

            await UniTask.Yield();

            // Parallelが勝ったので、Raceの相手側(raceOpponent)はSkipされているはず
            Assert.IsTrue(raceOpponentSkipped);

            // Raceが終わったので、Sequenceの次の処理が開始されているはず
            Assert.IsTrue(seqNextWorkStarted);

            // 4. 最後に Sequence の残りの処理を終わらせて全体の完走を待つ
            seqNextWork.End();
            await sequenceTask;

            Assert.IsTrue(initialParWorkEnded);
            Assert.IsTrue(addedParWorkEnded);
        });
    }
}
