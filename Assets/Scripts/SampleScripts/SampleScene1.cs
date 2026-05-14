using ChainPattern;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    /// <summary>
    /// Main script for Sample Scene 1
    /// </summary>
    public class SampleScene1 : MonoBehaviour
    {
        [SerializeField]
        TestButtons testButtons;
        [SerializeField]
        RectTransform moveRect;
        [SerializeField]
        TMPro.TextMeshProUGUI moveRectText;

        List<Button> sequenceButtons = new List<Button>();
        Button skipButton;
        ChainSequence chainSequence;

        const int MoveWidth = 400;
        const int MoveLeft = -MoveWidth / 2;
        const int MoveWidth3 = MoveWidth / 3;

        readonly Vector2 Position1 = new Vector2(MoveLeft + MoveWidth3 * 0, 0.0f);
        readonly Vector2 Position2 = new Vector2(MoveLeft + MoveWidth3 * 1, 0.0f);
        readonly Vector2 Position3 = new Vector2(MoveLeft + MoveWidth3 * 2, 0.0f);
        readonly Vector2 Position4 = new Vector2(MoveLeft + MoveWidth3 * 3, 0.0f);

        void Start()
        {
            sequenceButtons.Add(testButtons.AddButton("Sequence1", OnClickSequence1Button));
            sequenceButtons.Add(testButtons.AddButton("Sequence2", OnClickSequence2Button));
            sequenceButtons.Add(testButtons.AddButton("Sequence3", OnClickSequence3Button));
            skipButton = testButtons.AddButton("Skip");
            skipButton.interactable = false;
        }

        /// <summary>
        /// Sequence 1: Skip to the end
        /// Press skip button -> entire animation skips to Position4
        /// </summary>
        private async void OnClickSequence1Button()
        {
            chainSequence = new ChainSequence(
                ChainSetButtonsEnabled(false),
                ChainSetMoveRectPosition(Position1),
                ChainSetMoveRectText("1"),
                new ChainRace(
                    new ChainButton(skipButton),
                    new ChainSequence(
                        Utility.ChainMoveTween(moveRect, Position2, 1.0f),
                        Utility.ChainPlaySound(SoundType.Pong1),
                        ChainSetMoveRectText("2"),
                        new ChainDelay(0.2f),
                        Utility.ChainMoveTween(moveRect, Position3, 1.0f),
                        Utility.ChainPlaySound(SoundType.Pong1),
                        ChainSetMoveRectText("3"),
                        new ChainDelay(0.2f),
                        Utility.ChainMoveTween(moveRect, Position4, 1.0f),
                        Utility.ChainPlaySound(SoundType.Pong1),
                        ChainSetMoveRectText("4"),
                        new ChainDelay(0.2f)
                    )
                ),
                ChainSetButtonsEnabled(true)
            );

#if UNITY_EDITOR
            // Register the Chain with the debug window to visualize its state and tree structure at runtime
            ChainPattern.Editor.ChainDebugWindow.Watch(chainSequence);
#endif
            await chainSequence.Start();
        }

        /// <summary>
        /// Sequence 2: Skip each section independently
        /// Press skip button -> current section skips, next section starts
        /// </summary>
        private async void OnClickSequence2Button()
        {
            chainSequence = new ChainSequence(
                ChainSetButtonsEnabled(false),
                ChainSetMoveRectPosition(Position1),
                ChainSetMoveRectText("1"),
                new ChainRace(
                    new ChainButton(skipButton),
                    new ChainSequence(
                        Utility.ChainMoveTween(moveRect, Position2, 1.0f),
                        Utility.ChainPlaySound(SoundType.Pong1),
                        ChainSetMoveRectText("2"),
                        new ChainDelay(0.2f)
                    )
                ),
                new ChainRace(
                    new ChainButton(skipButton),
                    new ChainSequence(
                        Utility.ChainMoveTween(moveRect, Position3, 1.0f),
                        Utility.ChainPlaySound(SoundType.Pong1),
                        ChainSetMoveRectText("3"),
                        new ChainDelay(0.2f)
                    )
                ),
                new ChainRace(
                    new ChainButton(skipButton),
                    new ChainSequence(
                        Utility.ChainMoveTween(moveRect, Position4, 1.0f),
                        Utility.ChainPlaySound(SoundType.Pong1),
                        ChainSetMoveRectText("4"),
                        new ChainDelay(0.2f)
                    )
                ),
                ChainSetButtonsEnabled(true)
            );

#if UNITY_EDITOR
            // Register the Chain with the debug window to visualize its state and tree structure at runtime
            ChainPattern.Editor.ChainDebugWindow.Watch(chainSequence);
#endif
            await chainSequence.Start();
        }


        /// <summary>        
        /// Sequence 3: Section 2 cannot be skipped (important scene)
        /// Section 1: Skippable
        /// Section 2: NOT skippable (must watch)
        /// Section 3: Skippable 
        /// </summary>
        private async void OnClickSequence3Button()
        {
            chainSequence = new ChainSequence(
                ChainSetButtonsEnabled(false),
                ChainSetMoveRectPosition(Position1),
                ChainSetMoveRectText("1"),
                new ChainRace(
                    new ChainButton(skipButton),
                    new ChainSequence(
                        Utility.ChainMoveTween(moveRect, Position2, 1.0f),
                        Utility.ChainPlaySound(SoundType.Pong1),
                        ChainSetMoveRectText("2"),
                        new ChainDelay(0.2f)
                    )
                ),
                new ChainSequence(
                    Utility.ChainMoveTween(moveRect, Position3, 1.0f),
                    Utility.ChainPlaySound(SoundType.Pong1),
                    ChainSetMoveRectText("3"),
                    new ChainDelay(0.2f)
                ),
                new ChainRace(
                    new ChainButton(skipButton),
                    new ChainSequence(
                        Utility.ChainMoveTween(moveRect, Position4, 1.0f),
                        Utility.ChainPlaySound(SoundType.Pong1),
                        ChainSetMoveRectText("4"),
                        new ChainDelay(0.2f)
                    )
                ),
                ChainSetButtonsEnabled(true)
            );
#if UNITY_EDITOR
            // Register the Chain with the debug window to visualize its state and tree structure at runtime
            ChainPattern.Editor.ChainDebugWindow.Watch(chainSequence);
#endif
            await chainSequence.Start();
        }

        /// <summary>
        /// Enables or disables the sequence buttons        
        /// </summary>
        private BaseChain ChainSetButtonsEnabled(bool enabed)
        {
            return new ChainAction(() =>
            {
                foreach (Button button in sequenceButtons)
                {
                    button.interactable = enabed;
                }
            });
        }

        /// <summary>
        /// Sets the text of the moving rectangle
        /// </summary>
        private BaseChain ChainSetMoveRectText(string text)
        {
            return new ChainAction(() =>
            {
                moveRectText.text = text;
            });
        }

        /// <summary>
        /// Sets the position of the moving rectangle
        /// </summary>
        private BaseChain ChainSetMoveRectPosition(Vector2 position)
        {
            return new ChainAction(() =>
            {
                moveRect.anchoredPosition = position;
            });
        }
    }
}

