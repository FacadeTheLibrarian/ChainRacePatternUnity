// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using UnityEngine.UI;

namespace ChainPattern
{
    /// <summary>
    /// Chain that completes when a button is clicked
    /// </summary>
    public class ChainButton : BaseChain
    {
        Button targetButton;

        public ChainButton(Button button)
        {
            targetButton = button;
        }

        /// <summary>
        /// Starts chain, enabling the button and adding a click listener. 
        /// Completes when the button is clicked.
        /// Do nothing if the button is null, completing immediately.
        /// </summary>
        protected override void StartInternal()
        {
            if (targetButton == null)
            {
                // Do nothing if it will be skipped immediately
                Complete();
                return;
            }
            targetButton.interactable = true;
            targetButton.onClick.AddListener(OnClickButton);
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
            if (targetButton != null)
            {
                targetButton.onClick.RemoveListener(OnClickButton);
                targetButton.interactable = false;
            }
        }

        /// <summary>
        /// Called when the button is clicked
        /// </summary>
        private void OnClickButton()
        {
            if (targetButton != null)
            {
                targetButton.onClick.RemoveListener(OnClickButton);
                targetButton.interactable = false;
            }
            Complete();
        }
    }
}
