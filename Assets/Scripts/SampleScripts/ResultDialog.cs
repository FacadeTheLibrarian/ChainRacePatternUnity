using ChainPattern;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    /// <summary>
    /// a dialog that shows results
    /// </summary>
    public class ResultDialog : MonoBehaviour
    {
        /// <summary>
        /// VerticalLayoutGroup that holds ranking entries
        /// </summary>
        [SerializeField]
        VerticalLayoutGroup rankingVerticalLayoutGroup;
        /// <summary>
        /// RankingPlayer that serves as a source for instantiation
        /// </summary>
        [SerializeField]
        RankingPlayer sourceRankingPlayer;
        /// <summary>
        /// Animator for animating the dialog
        /// </summary>
        [SerializeField]
        public Animator animator;

        /// <summary>
        /// Instances of RankingPlayer
        /// </summary>
        public List<RankingPlayer> rankingPlayers = new List<RankingPlayer>();
        /// <summary>
        /// Initial position of the instantiated RankingPlayer
        /// </summary>
        List<Vector2> rankingPlayerAnchoredPositions = new List<Vector2>();
        /// <summary>
        /// Offset amount for the starting position when the RankingPlayer is displayed
        /// </summary>
        readonly Vector2 rankingPlayerOffset1 = new Vector2(0.0f, -600.0f);
        readonly Vector2 rankingPlayerOffset2 = new Vector2(0.0f, 600.0f);

        PlayerInfo[] playerInfos;

        private void Awake()
        {
            sourceRankingPlayer.gameObject.SetActive(false);
        }

        /// <summary>
        /// set player information to display in the dialog. 
        /// The number of playerInfos should be the same as the number of RankingPlayer instances created by UpdatePlayerNumber
        /// </summary>
        /// <param name="playerInfos"></param>
        public void SetPlayerInfos(PlayerInfo[] playerInfos)
        {
            this.playerInfos = playerInfos;
            UpdatePlayerNumber(playerInfos.Length);
            // Set player names
            for (int i = 0; i < playerInfos.Length; ++i)
            {
                rankingPlayers[i].SetPlayerName(playerInfos[i].name);
            }
        }

        /// <summary>
        /// Updates the number of players disdplayed in the ranking area
        /// </summary>
        public void UpdatePlayerNumber(int number)
        {
            // Destroy old RankingPlayer instances
            rankingVerticalLayoutGroup.enabled = true;
            foreach (RankingPlayer player in rankingPlayers)
            {
                player.gameObject.SetActive(false);
                Destroy(player.gameObject);
            }
            rankingPlayers.Clear();
            rankingPlayerAnchoredPositions.Clear();

            // Create the specified number of RankingPlayer instances
            for (int i = 0; i < number; ++i)
            {
                var go = Instantiate<GameObject>(sourceRankingPlayer.gameObject, rankingVerticalLayoutGroup.transform);
                go.gameObject.SetActive(true);
                rankingPlayers.Add(go.GetComponent<RankingPlayer>());
            }

            // Force update the layout to get the correct positions
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rankingVerticalLayoutGroup.GetComponent<RectTransform>());

            // Save positions
            foreach (RankingPlayer r in rankingPlayers)
            {
                Vector2 position = r.GetComponent<RectTransform>().anchoredPosition;
                // Debug.Log($"{rankingPlayerAnchoredPositions.Count}:({position.x}, {position.y})");
                rankingPlayerAnchoredPositions.Add(r.GetComponent<RectTransform>().anchoredPosition);
                // add offset
                r.GetComponent<RectTransform>().anchoredPosition += rankingPlayerOffset1;
            }
            rankingVerticalLayoutGroup.enabled = false;
        }

        /// <summary>
        /// set the initial position of the dialog and RankingPlayers according to the animation. 
        /// This should be called before starting the animation chain to prevent the dialog from jumping at the beginning of the animation.
        /// </summary>
        public void SetPanelInitialPosition()
        {
            animator.Play("ResultDialogShowAnim", 0, 0.0f);
            animator.Update(0.0f);
            animator.speed = 0.0f; // Pause the animator to prevent it from playing automatically
        }

        /// <summary>
        /// Show the dialog with animation and displaying RankingPlayers
        /// </summary>
        public BaseChain ChainShowDialog()
        {
            return new ChainParallel(
                ChainShowRankingPlayers(),
                new ChainAnimator(animator, "ResultDialogShowAnim")
            );
        }

        /// <summary>
        /// show bonus point animation for each player
        /// </summary>
        /// <returns></returns>
        public BaseChain ChainShowBonus()
        {
            ChainParallel parallel = new ChainParallel();
            int index = 0;
            for (int i = 0; i < playerInfos.Length; ++i)
            {
                if (playerInfos[i].bonus <= 0)
                {
                    continue;
                }
                parallel.Add(
                    new ChainSequence(
                        new ChainDelay(0.25f * index),
                        rankingPlayers[i].ChainBonus(playerInfos[i].score + playerInfos[i].bonus)
                    )
                );
                ++index;
            }
            return parallel;
        }

        /// <summary>
        /// Show the dialog with animation and displaying RankingPlayers
        /// </summary>
        public BaseChain ChainHideDialog()
        {
            return new ChainParallel(
                new ChainAnimator(animator, "ResultDialogHideAnim"),
                new ChainSequence(
                    new ChainDelay(0.25f),
                    ChainHideRankingPlayers()
                )
            );
        }

        /// <summary>
        /// Create a chain to display RankingPlayers
        /// </summary>
        public BaseChain ChainShowRankingPlayers()
        {
            var parallel = new ChainParallel();
            var curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
            for (int i = 0; i < rankingPlayers.Count; ++i)
            {
                RankingPlayer rankingPlayer = rankingPlayers[i];
                RectTransform rectTransform = rankingPlayer.GetComponent<RectTransform>();
                Vector2 endPosition = rankingPlayerAnchoredPositions[i];
                int score = playerInfos[i].score;
                parallel.Add(
                    new ChainSequence(
                        new ChainDelay(0.25f * i),
                        new ChainParallel(
                            Utility.ChainMoveTween(rectTransform, endPosition, 0.7f, curve),
                            new ChainSequence(
                                new ChainDelay(0.4f),
                                Utility.ChainPlaySound(SoundType.Gao1)
                            ),
                            new ChainSequence(
                                new ChainDelay(0.5f),
                                rankingPlayer.ChainPointAnimation(score)
                            )
                        )
                    )
                );
            }
            return parallel;
        }


        /// <summary>
        /// Create a chain to hide RankingPlayers
        /// </summary>
        public BaseChain ChainHideRankingPlayers()
        {
            var parallel = new ChainParallel();
            var curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
            for (int i = 0; i < rankingPlayers.Count; ++i)
            {
                RankingPlayer rankingPlayer = rankingPlayers[i];
                RectTransform rectTransform = rankingPlayer.GetComponent<RectTransform>();
                Vector2 endPosition = rankingPlayerAnchoredPositions[i] + rankingPlayerOffset2;
                parallel.Add(
                    new ChainSequence(
                        new ChainDelay(0.1f * i),
                        Utility.ChainMoveTween(rectTransform, endPosition, 0.7f, curve)
                    )
                );
            }
            return parallel;
        }


    }
}

