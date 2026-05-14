// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

namespace ChainPattern
{
    /* NOTE:
     *  Halt means like "stop" so I thought "ChainHalt" is halting the chain; skipping myself or something to stop all the chains. 
     *  So I named it "ChainFreeze" to make it more clear that it just stops itself; "freezes" and does not affect other chains.
     */
    /// <summary>
    /// Chain class that freezes execution after starting
    /// </summary>
    public class ChainFreeze : BaseChain
    {
        /// <summary>
        /// Starts Chain logics, but does not complete (freezes indefinitely)
        /// </summary>
        protected override void StartInternal()
        {
            // Does not call Complete()
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
        }
    }
}

