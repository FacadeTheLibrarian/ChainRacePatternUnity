// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using System;

namespace ChainPattern
{
    /// <summary>
    /// Chain that executes a single action/function
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
        /// Sets the action to be executed
        /// </summary>
        public void SetAction(Action action)
        {
            actionToCall = action;
        }

        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            actionToCall?.Invoke();
            actionToCall = null;
            Complete();
        }

        // NOTE: 元スクリプトの Start() -> if(!complete) Skip() だと、Start でも Skip でも actionToCall を呼びたい
        // 違うのはCompleteするかどうかなので結局同じだと思われる
        // In the original script, Start() -> if(!complete) Skip() is called, and we want to call actionToCall in both Start and Skip.
        // The only difference is whether to call Complete or not, so it seems to be the same.
        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
            actionToCall?.Invoke();
            actionToCall = null;
        }
    }
}
