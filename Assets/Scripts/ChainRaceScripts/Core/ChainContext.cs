using System;
using System.Collections.Generic;

namespace ChainPattern {
    /// <summary>
    /// Provides context and control for managing the completion and skipping of a chain operation. Enables registration
    /// of callbacks to be invoked when a chain is completed or skipped, and ensures that only one of these actions is
    /// performed per context instance.
    /// </summary>
    /// <remarks>A ChainContext instance is typically used to coordinate the flow of a chain-based operation,
    /// such as in a pipeline or workflow. Once either Complete or Skip is called, subsequent calls to these methods
    /// have no effect (ignored). This class is not thread-safe, but 99% safe while using UniTask</remarks>
    public class ChainContext : IContextInvokable, IDisposable {
        Action<BaseChain> onChainComplete = default;
        Action<BaseChain> onChainSkip = default;
        bool hasAnyCalled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainContext"/> class with no callbacks.
        /// </summary>
        public ChainContext() {
            onChainComplete = null;
            onChainSkip = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainContext"/> class with the specified complete and skip callbacks.
        /// </summary>
        /// <param name="complete">The callback to invoke when the chain completes.</param>
        /// <param name="skip">The callback to invoke when the chain is skipped.</param>
        public ChainContext(Action<BaseChain> complete, Action<BaseChain> skip) {
            onChainComplete = complete;
            onChainSkip = skip;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="ChainContext"/>, clearing the complete callback.
        /// </summary>
        public void Dispose() {
            onChainComplete = null;
            onChainSkip = null;
        }

        /// <summary>
        /// Resets the state that allows Complete or Skip to be called again.
        /// </summary>
        public void Reset() {
            hasAnyCalled = false;
        }

        /// <summary>
        /// Clears both complete and skip callbacks, preventing further callback calls.
        /// </summary>
        public void ReleaseCallbacks() {
            onChainComplete = null;
            onChainSkip = null;
        }

        /// <summary>
        /// Invokes the complete callback if no action has been performed yet, and locks the state by a guard clause.
        /// </summary>
        /// <param name="chain">The chain that has completed.</param>
        public void Complete(BaseChain chain) {
            if (hasAnyCalled) {
                return;
            }
            hasAnyCalled = true;
            if (onChainComplete != null) {
                onChainComplete(chain);
            }
        }

        /// <summary>
        /// Invokes the skip callback if no action has been performed yet, and locks the state by a guard clause.
        /// </summary>
        /// <param name="chain">The chain that has been skipped.</param>
        public void Skip(BaseChain chain) {
            if (hasAnyCalled) {
                return;
            }
            hasAnyCalled = true;
            if (onChainSkip != null) {
                onChainSkip(chain);
            }
        }
    }
}