// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using Cysharp.Threading.Tasks;
using System;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace ChainPattern {
    /// <summary>
    /// Base class for all Chain types.
    /// Manages state transitions (Ready -> Started -> Completed/Skipped)
    /// and provides the UniTask-based async interface.
    /// </summary>
    public abstract class BaseChain : IDisposable {
        /// <summary>
        /// State of a Chain (Ready, Started, Skipped, Completed)
        /// implemented as a bitmask to allow combinations (e.g. Started | Skipped)
        /// </summary>
        public enum ChainState {
            Ready = 1,
            Started = 2,
            Skipped = 4,
            Completed = 8,
        }

        UniTaskCompletionSource<bool> currentUtcs = default;
        IContextInvokable upstreamContext = default;
        protected ChainState chainState = default;

#if UNITY_EDITOR
        // ---- Debug fields (editor-only) ----

        // Timestamps (Time.realtimeSinceStartup) recorded at start and completion/skip.
        // Used by DebugElapsedSeconds to compute how long the Chain has been running.
        // completedTime stays at -1 while the Chain is still running.
        float startedTime = -1f;
        float completedTime = -1f;
        bool hasSkipped = false;
#endif

        public BaseChain() {
            chainState = ChainState.Ready;
        }
        public virtual void Dispose() { }

        /// <summary>
        /// Starts Chain without any callbacks.
        /// </summary>
        /// <returns>Task completes when done</returns>
        public UniTask Start() {
            return StartWithCallback(null);
        }

        /// <summary>
        /// Starts Chain.
        /// Returns a UniTask that completes when the Chain finishes or is skipped.
        /// Implements should be just dispathcing the logic.
        /// </summary>
        /// <param name="upstreamContext">context that upstream callbacks are set</param>
        /// <returns>Task completes when done</returns>
        public UniTask StartWithCallback(IContextInvokable upstreamContext) {
            // If chain has already started, return the existing task to wait for it
            if (chainState != ChainState.Ready) {
                return currentUtcs?.Task ?? UniTask.CompletedTask;
            }
            this.upstreamContext = upstreamContext;
            currentUtcs = new UniTaskCompletionSource<bool>();
            chainState = ChainState.Started;
#if UNITY_EDITOR
            startedTime = Time.realtimeSinceStartup;
#endif
            StartInternal();
            return currentUtcs.Task;
        }

        /// <summary>
        /// Skips the Chain, transitioning it immediately to its final state.
        /// </summary>
        public void Skip() {
            // chainState should be Started or Ready to allow skip
            // Calling Skip directly instead of Start setting callbacks and fastforward
            if ((chainState & (ChainState.Started | ChainState.Ready)) == 0) {
                return;
            }

            chainState = ChainState.Skipped;
#if UNITY_EDITOR
            completedTime = Time.realtimeSinceStartup;
            hasSkipped = true;
#endif
            SkipInternal();
            upstreamContext?.Skip(this);
            currentUtcs?.TrySetResult(true);
        }

        /// <summary>
        /// Marks the Chain as completed. Must be called by derived classes when their work is done.
        /// </summary>
        protected void Complete() {
            if (chainState != ChainState.Started) {
                return;
            }
            chainState = ChainState.Completed;
#if UNITY_EDITOR
            completedTime = Time.realtimeSinceStartup;
#endif
            upstreamContext?.Complete(this);
            currentUtcs?.TrySetResult(true);
        }

        /// <summary>
        /// Implement the Chain's main logic here.
        /// if the Chain finishes normally, call Complete() to transition to Completed state.
        /// </summary>
        protected abstract void StartInternal();

        /// <summary>
        /// Implement immediate transition to the final state here.
        /// Skip never completes, co do NOT call Complete(), that has different actor
        /// </summary>
        protected abstract void SkipInternal();

#if UNITY_EDITOR
        // ---- Debug properties (editor-only) ----
        // These are used by ChainDebugWindow to display the Chain tree at runtime.

        /// <summary>The type name of this Chain instance.</summary>
        public string DebugTypeName => GetType().Name;

        /// <summary>This Chain was skipped?</summary>
        public bool HasSkipped => hasSkipped;

        /// <summary>The current state as a string: "Ready", "Started", "Skipped", or "Completed".</summary>
        public string DebugState => chainState.ToString();

        /// <summary>
        /// Seconds elapsed since this Chain started.
        /// Returns -1 if not yet started. Freezes at the completion/skip time once finished.
        /// </summary>
        public float DebugElapsedSeconds {
            get {
                if (startedTime < 0f) return -1f;
                float end = completedTime >= 0f ? completedTime : Time.realtimeSinceStartup;
                return end - startedTime;
            }
        }

        /// <summary>
        /// Child Chains for tree display. Composite Chains (Sequence/Parallel/Race) override this
        /// to return all registered children regardless of their state.
        /// </summary>
        public virtual BaseChain[] DebugChildren => System.Array.Empty<BaseChain>();
#endif
    }
}

