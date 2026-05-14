using ChainPattern;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    /// <summary>
    /// Main script for Sample Scene 2
    /// </summary>
    public class SampleScene2 : MonoBehaviour
    {
        [SerializeField]
        PlayerInfo[] playerInfos;
        [SerializeField]
        ResultDialog resultDialog;
        [SerializeField]
        GameObject touchScreen;
        [SerializeField]
        Button screenButton;

        [SerializeField]
        FadePanel fadePanel;
        [SerializeField]
        TestButtons testButtons;

        Button startButton;

        void Start()
        {
            touchScreen.SetActive(false);
            startButton = testButtons.AddButton("Start", async () =>
            {
                screenButton.interactable = false;
                resultDialog.SetPlayerInfos(playerInfos);
                fadePanel.gameObject.SetActive(true);
                fadePanel.SetAlpha(1.0f);
                resultDialog.SetPanelInitialPosition();
                startButton.interactable = false;
                Debug.Log("START");
                BaseChain chain = ChainResult();
#if UNITY_EDITOR
                ChainPattern.Editor.ChainDebugWindow.Watch(chain);
#endif  
                await chain.Start();
                Debug.Log("END");
                startButton.interactable = true;
            });            
        }

        /// <summary>
        /// shows the result dialog with ranking player animation and fade in effect, 
        /// then hides the dialog with fade out effect. The skip button can be used to skip the animations and effects.
        /// </summary>
        public BaseChain ChainResult()
        {
            return new ChainSequence(
                new ChainDelay(0.5f),
                new ChainRace(
                    new ChainButton(screenButton),
                    new ChainParallel(
                        fadePanel.ChainFade(false),
                        new ChainSequence(
                            new ChainDelay(0.1f),
                            resultDialog.ChainShowDialog()
                       )
                    )
                ),
                new ChainRace(
                    new ChainButton(screenButton),
                    resultDialog.ChainShowBonus()
                ),
                ChainTouchScreen(),
                new ChainParallel(
                    resultDialog.ChainHideDialog(),
                    new ChainSequence(
                        new ChainDelay(0.3f + playerInfos.Length * 0.1f),
                        fadePanel.ChainFade(true)
                    )
                )
            );
        }

        /// <summary>
        /// show the touch screen and wait until the screen button is pressed, then hide the touch screen
        /// </summary>
        private BaseChain ChainTouchScreen()
        {
            return new ChainSequence(
                new ChainAction(() =>
                {
                    touchScreen.SetActive(true);
                }),
                new ChainButton(screenButton),
                new ChainAction(() =>
                {
                    touchScreen.SetActive(false);
                })
            );
        }
    }
}

