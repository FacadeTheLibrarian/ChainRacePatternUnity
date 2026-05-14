// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace ChainPattern {
    //NOTE: せっかくChainParallelが存在するのにevent Actionでマルチキャストデリゲートするか？と思ったのでフックを外注製に
    /// <summary>
    /// Chain with onStart, onSkip, onUpdate hooks
    /// </summary>
    public class ChainWork : BaseChain {
        IChainWorkLifeCycle lifeCycle = default;
        CancellationTokenSource tokenSource = default;
        bool isDispatched = false;

        public ChainWork(IChainWorkLifeCycle lifeCycle = null) {
            this.lifeCycle = lifeCycle;
        }
        protected override void OnDispose() {
            tokenSource?.Cancel();
        }

        /// <summary>
        /// Ends the work
        /// </summary>
        public void End() {
            if (isDispatched) {
                tokenSource?.Cancel();
                isDispatched = false;
                Complete();
            }
        }

        /// <summary>
        /// Starts chain work by async operation
        /// If lifeCycle is not provided, no async operation will be performed
        /// Just waits for End() to be called to complete the chain
        /// </summary>
        protected override void StartInternal() {
            isDispatched = true;
            // NOTE: if _lifeCycle is null, it means the user doesn't want to call _lifeCycle.Update(), just waiting for End()
            //       so, no need to start FrameLoopAsync in that case
            if (lifeCycle != null) {
                tokenSource = new CancellationTokenSource();
                lifeCycle.BeforeStart();
                FrameLoopAsync(tokenSource.Token).Forget();
            }
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal() {
            isDispatched = false;
            tokenSource?.Cancel();
            lifeCycle?.AfterSkip();
        }

        /// <summary>
        /// Execute FrameLoop
        /// </summary>
        private async UniTask FrameLoopAsync(CancellationToken token) {
            try {
                while (!token.IsCancellationRequested) {
                    //NOTE: FrameLoopAsync should be called from StartInternal and _lifeCycle is guaranteed to be non-null in that case
                    //      so no need to check for null here
                    bool shouldEnd = lifeCycle.Update();
                    if (shouldEnd) {
                        End();
                    }
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException) {
                // Executed when canceled                
            }
            finally {
                // OnDispose of resources after completion or cancellation
                tokenSource?.Dispose();
                tokenSource = null;
            }
        }
    }
}

