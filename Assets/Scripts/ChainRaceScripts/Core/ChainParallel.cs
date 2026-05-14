// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using System.Collections.Generic;

namespace ChainPattern {
    /// <summary>
    /// Chain that executes multiple chains in parallel
    /// </summary>
    public class ChainParallel : BaseChain {
        Queue<BaseChain> chainQueue = default;
        List<BaseChain> dispatchedChainList = default;
        ChainContext downstreamContext = default;

        enum ParallelState {
            Ready,
            Dispatching,
            Dispatched,
            Skipping,
            Finished,
        }
        ParallelState parallelState;

#if UNITY_EDITOR
        // Full list of all registered children, preserved in definition order for the debug view.
        // Unlike chainQueue/dispatchedChainList, entries are never removed even after completion.
        List<BaseChain> debugChainList = new List<BaseChain>();
#endif

        public ChainParallel(params BaseChain[] chains) {
            parallelState = ParallelState.Ready;
            chainQueue = new Queue<BaseChain>();
            dispatchedChainList = new List<BaseChain>();
            foreach (BaseChain chain in chains) {
                chainQueue.Enqueue(chain);
            }
            downstreamContext = new ChainContext(OnChainComplete, OnChainComplete);
#if UNITY_EDITOR
            debugChainList.AddRange(chains);
#endif
        }

        /// <summary>
        /// Adds a chain to the parallel execution.
        /// If already Started, the chain begins immediately.
        /// Chains added after the parallel has finished are ignored.
        /// </summary>
        public ChainParallel Add(BaseChain chain) {
#if UNITY_EDITOR
            debugChainList.Add(chain);
#endif
            if (parallelState == ParallelState.Finished) {
                // Ignore
            }
            else if (parallelState == ParallelState.Dispatched) {
                dispatchedChainList.Add(chain);
                chain.StartWithCallback(downstreamContext);
            }
            else {
                // For all states except Started/Finished, queue into pending list
                // During Consuming, Add() may still happen reentrantly from chains being skipped.
                // Queue it into chainQueue so it will also be consumed in this pass.
                chainQueue.Enqueue(chain);
            }
            return this;
        }

        /// <summary>
        /// Starts chains in parallel. Completes when all chains have completed.
        /// </summary>
        protected override void StartInternal() {
            if (chainQueue.Count <= 0) {
                parallelState = ParallelState.Finished;
                Complete();
                return;
            }
            parallelState = ParallelState.Dispatching;
            while (chainQueue.Count > 0 && parallelState == ParallelState.Dispatching) {
                BaseChain c = chainQueue.Dequeue();
                dispatchedChainList.Add(c);
                c.StartWithCallback(downstreamContext);
            }
            if (parallelState == ParallelState.Dispatching) {
                parallelState = ParallelState.Dispatched;
            }
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal() {
            downstreamContext.ReleaseCallbacks();
            parallelState = ParallelState.Skipping;
            ConsumeStartedAndPendingChains();
            parallelState = ParallelState.Finished;
        }

        /// <summary>
        /// Consumes (completes or skips) all started and pending chains
        /// </summary>
        private void ConsumeStartedAndPendingChains() {
            while (dispatchedChainList.Count > 0) {
                BaseChain c = dispatchedChainList[0];
                dispatchedChainList.RemoveAt(0);
                c.Skip();
            }
            while (chainQueue.Count > 0) {
                BaseChain c = chainQueue.Dequeue();
                c.Skip();
            }
        }

        /// <summary>
        /// Callback invoked when a chain completes
        /// </summary>        
        private void OnChainComplete(BaseChain chain) {
            if (dispatchedChainList.Contains(chain)) {
                dispatchedChainList.Remove(chain);
                if (chainQueue.Count <= 0 && dispatchedChainList.Count <= 0) {
                    parallelState = ParallelState.Finished;
                    Complete();
                    return;
                }
            }
        }
#if UNITY_EDITOR
        /// <summary>
        /// Returns all registered children in definition order for the debug tree view.
        /// Includes chains regardless of their current state (Ready/Started/Completed/Skipped).
        /// </summary>
        public override BaseChain[] DebugChildren => debugChainList.ToArray();
#endif
    }
}
