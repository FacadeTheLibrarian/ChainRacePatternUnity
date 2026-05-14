// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using System.Collections.Generic;

namespace ChainPattern {
    /// <summary>
    /// Chain that executes multiple chains sequentially
    /// </summary>
    public class ChainSequence : BaseChain {
        Queue<BaseChain> chainQueue = default;
        BaseChain currentChain = default;
        ChainContext downstreamContext = default;

#if UNITY_EDITOR
        // Full list of all registered children, preserved in definition order for the debug view.
        // Unlike chainQueue, entries are never removed from this list even after execution.
        List<BaseChain> debugChainList = new List<BaseChain>();
#endif

        public ChainSequence(params BaseChain[] chains) {
            chainQueue = new Queue<BaseChain>();
            downstreamContext = new ChainContext(_ => NextChain(), _ => NextChain());
#if UNITY_EDITOR
            debugChainList.AddRange(chains);
#endif
            if (chains.Length > 0) {
                foreach (BaseChain chain in chains) {
                    chainQueue.Enqueue(chain);
                }
            }
        }

        /// <summary>
        /// Adds a chain to the sequence.
        /// Chains added after the sequence has finished are ignored.
        /// </summary>
        public ChainSequence Add(BaseChain chain) {
            if (chainState == ChainState.Ready) {
                chainQueue.Enqueue(chain);
#if UNITY_EDITOR
                debugChainList.Add(chain);
#endif
            }
            return this;
        }

        /// <summary>
        /// Starts chains in sequence. Completes when all chains have completed, or when skipped.
        /// </summary>
        protected override void StartInternal() {
            NextChain();
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal() {
            downstreamContext.Release();
            if (currentChain != null) {
                currentChain.Skip();
            }
            while (chainQueue.Count > 0) {
                BaseChain c = chainQueue.Dequeue();
                c.Skip();
            }
        }

        /// <summary>
        /// Executes the next chain in the sequence
        /// </summary>
        private void NextChain() {
            if (chainQueue.Count <= 0) {
                Complete();
                return;
            }
            currentChain = chainQueue.Dequeue();
            downstreamContext.Reset();
            currentChain.StartWithCallback(downstreamContext);
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