// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

namespace ChainPattern
{
    // Nop seems like "No operation "and never completes"", just like ChainFreeze(old -> ChainHalt)
    // But I think "Complete immediately" is easier to understand, so I renamed it.
    /// <summary>
    /// Chain class that completes immediately after starting (no operation)
    /// </summary>
    public class ChainImmidiateComplete : BaseChain
    {
        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            Complete();
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
        }
    }
}