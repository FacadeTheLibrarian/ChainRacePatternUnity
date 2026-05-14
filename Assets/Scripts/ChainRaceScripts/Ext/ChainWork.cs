// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace ChainPattern {
    //NOTE: せっかくChainParallelが存在するのにevent Actionでマルチキャストデリゲートするか？と思ったのでフックを外注製に
    /// <summary>
    /// Chain with onStart, onSkip, onUpdate events
    /// </summary>
    public class ChainWork : BaseChain {
        private IChainWorkLifeCycle _lifeCycle = default;
        private CancellationTokenSource _tokenSource = default;
        private bool _isDispatched = false;

        public ChainWork(IChainWorkLifeCycle lifeCycle) {
            _lifeCycle = lifeCycle;
        }

        /// <summary>
        /// Ends the work execution
        /// </summary>
        public void End() {
            if (_isDispatched) {
                _tokenSource?.Cancel();
                _isDispatched = false;
                Complete();
            }
        }

        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal() {
            _isDispatched = true;
            _tokenSource = new CancellationTokenSource();
            _lifeCycle?.BeforeStart();
            FrameLoopAsync(_tokenSource.Token).Forget();
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal() {
            _isDispatched = false;
            _tokenSource?.Cancel();
            _lifeCycle?.AfterSkip();
        }

        /// <summary>
        /// Execute FrameLoop
        /// </summary>
        private async UniTask FrameLoopAsync(CancellationToken token) {
            try {
                while (!token.IsCancellationRequested) {
                    bool shouldEnd = false;
                    if (_lifeCycle != null) {
                        shouldEnd = _lifeCycle.Update();
                    }
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
                // Dispose of resources after completion or cancellation
                _tokenSource?.Dispose();
                _tokenSource = null;
            }
        }
    }
}

