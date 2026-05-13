using System;
using System.Collections.Generic;

namespace ChainPattern {
    public class ChainContext : IDisposable {
        Action<BaseChain> onChainComplete = default;
        Action<BaseChain> onChainSkip = default;
        bool hasAnyCalled = false;
        public ChainContext() {
            onChainComplete = null;
            onChainSkip = null;
        }
        public ChainContext(Action<BaseChain> complete, Action<BaseChain> skip) {
            onChainComplete = complete;
            onChainSkip = skip;
        }
        public void Dispose() {
            onChainComplete = null;
        }
        public void Reset() {
            hasAnyCalled = false;
        }
        public void Complete(BaseChain chain) {
            if (hasAnyCalled) {
                return;
            }
            hasAnyCalled = true;
            if (onChainComplete != null) {
                onChainComplete(chain);
            }
        }
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