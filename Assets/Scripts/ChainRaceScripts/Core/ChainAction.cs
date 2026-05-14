// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using System;

namespace ChainPattern
{
    /// <summary>
    /// Chain that its final action is a single action/function
    /// </summary>
    public class ChainAction : BaseChain
    {
        Action actionToCall;

        public ChainAction()
        {
        }

        /// <summary>
        /// Creates a Chain with a specified action/function
        /// </summary>        
        public ChainAction(Action action)
        {
            actionToCall = action;
        }

        /// <summary>
        /// Replace Action to call
        /// </summary>
        public void SetAction(Action action)
        {
            actionToCall = action;
        }

        /// <summary>
        /// Starts calling the action and completes the chain
        /// </summary>
        protected override void StartInternal()
        {
            actionToCall?.Invoke();
            actionToCall = null;
            Complete();
        }

        // NOTE: 元スクリプトの Start() -> if(!complete) Skip() だと、Start でも Skip でも actionToCall は呼びたい(呼ばれる)
        // 違うのはCompleteするかどうかなのでそれなら結局同じ
        // In the original script, Start() -> if(!complete) Skip() is called, and we want to call actionToCall in both Start and Skip.
        // The only difference is whether to call Complete or not, so it seems to be the same.
        /// <summary>
        /// Skip with calling the action, but does NOT complete the chain
        /// </summary>
        protected override void SkipInternal()
        {
            actionToCall?.Invoke();
            actionToCall = null;
        }
    }
}
