// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace ChainPattern
{
    /// <summary>
    /// Chain that waits for a specified duration
    /// </summary>
    public class ChainDelay : BaseChain
    {
        private readonly float DELAY_SECONDS = 0.0f;
        private CancellationTokenSource _tokenSource = default;

        /// <summary>
        /// Creates a delay chain with duration in seconds
        /// </summary>
        public ChainDelay(float seconds)
        {
            DELAY_SECONDS = seconds;
        }

        public override void Dispose() {
            
        }

        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            _tokenSource = new CancellationTokenSource();
            DelayAsync(_tokenSource.Token).Forget();
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
            if (_tokenSource != null && !_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel();
            }
        }

        /// <summary>
        /// Execute UniTask.Delay
        /// </summary>
        private async UniTask DelayAsync(CancellationToken token)
        {
            try
            {
                await UniTask.Delay((int)(DELAY_SECONDS * 1000), cancellationToken: token);
                Complete();
            }
            catch (OperationCanceledException)
            {
                // Executed when canceled                
            }
            finally
            {
                // Dispose of resources after completion or cancellation
                _tokenSource?.Dispose();
                _tokenSource = null;
            }
        }
    }
}
