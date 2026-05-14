// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using System.Collections.Generic;

namespace ChainPattern {
    /// <summary>
    /// Chain that completes when any of its child chains completes first
    /// </summary>
    public class ChainRace : BaseChain {
        Queue<BaseChain> chainQueue = new Queue<BaseChain>();
        List<BaseChain> dispatchedChainList = new List<BaseChain>();
        ChainContext downstreamContext = default;

        enum RaceState {
            Ready,
            Dispatching,
            Dispatched,
            Skipped,
            Finished,
        }
        private RaceState raceState;

#if UNITY_EDITOR
        // Full list of all registered children, preserved in definition order for the debug view.
        // Unlike chainQueue/dispatchedChainList, entries are never removed even after the race ends.
        List<BaseChain> debugChainList = new List<BaseChain>();
#endif

        public ChainRace(params BaseChain[] chains) {
            raceState = RaceState.Ready;
            chainQueue = new Queue<BaseChain>(chains);
#if UNITY_EDITOR
            debugChainList.AddRange(chains);
#endif
        }

        /// <summary>
        /// Adds a chain to the race.
        /// If already Started, the chain begins immediately.
        /// Chains added after the race has finished are ignored.
        /// </summary>
        public ChainRace Add(BaseChain chain) {
#if UNITY_EDITOR
            debugChainList.Add(chain);
#endif
            if (raceState == RaceState.Finished) {
                // Ignore
            }
            else if (raceState == RaceState.Dispatched) {
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
        /// Starts execution
        /// </summary>
        protected override void StartInternal() {
            if (chainQueue.Count <= 0) {
                raceState = RaceState.Finished;
                Complete();
                return;
            }

            raceState = RaceState.Dispatching;
            downstreamContext = new ChainContext(OnChainComplete, OnChainComplete);

            // If chain starts and immidiately completes in a single frame, raceState should be skipping and then finished
            // so the condition never meets, and while breaks
            while (chainQueue.Count > 0 && raceState == RaceState.Dispatching) {
                BaseChain chain = chainQueue.Dequeue();
                dispatchedChainList.Add(chain);
                chain.StartWithCallback(downstreamContext);
            }
            if (raceState == RaceState.Dispatching) {
                raceState = RaceState.Dispatched;
            }
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal() {
            downstreamContext.Release();
            raceState = RaceState.Skipped;
            SkipAll();
            raceState = RaceState.Finished;
        }

        /// <summary>
        /// Callback invoked when a chain completes
        /// </summary>        
        private void OnChainComplete(BaseChain chain) {
            if (dispatchedChainList.Contains(chain)) {
                dispatchedChainList.Remove(chain);
                raceState = RaceState.Skipped;
                SkipAll();
                raceState = RaceState.Finished;
                Complete();
            }
        }

        /// <summary>
        /// Consumes (completes or skips) all started and pending chains
        /// </summary>
        private void SkipAll() {
            while (dispatchedChainList.Count > 0) {
                BaseChain chain = dispatchedChainList[0];
                dispatchedChainList.RemoveAt(0);
                chain.Skip();
            }
            while (chainQueue.Count > 0) {
                BaseChain chain = chainQueue.Dequeue();
                chain.Skip();
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
