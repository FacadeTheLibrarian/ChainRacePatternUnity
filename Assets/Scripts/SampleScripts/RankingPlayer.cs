using ChainPattern;
using TMPro;
using UnityEngine;

namespace Sample
{
    /// <summary>
    /// A class representing a player in the ranking
    /// </summary>
    public class RankingPlayer : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI textName;
        [SerializeField]
        TextMeshProUGUI textPoint;
        [SerializeField]
        Animator animator;
        int playerPoint = 0;

        private void Awake()
        {
            textPoint.text = "0pt";
        }

        /// <summary>
        /// set player name to display
        /// </summary>
        public void SetPlayerName(string playerName)
        {
            textName.text = playerName;
        }

        /// <summary>
        /// show the bonus animation and update the point text area to end
        /// </summary>        
        public BaseChain ChainBonus(int totalPointAfterBonus)
        {
            return new ChainParallel(
                new ChainAnimator(animator, "ResultPlayerAnimBonus"),
                new ChainSequence(
                    new ChainDelay(0.3f),
                    Utility.ChainPlaySound(SoundType.Pong2),
                    ChainPointAnimation(totalPointAfterBonus)
                )
            );
        }

        /// <summary>
        /// Creates a chain that animates the point text area from start to end
        /// </summary>
        public BaseChain ChainPointAnimation(int end)
        {
            int start = playerPoint;
            playerPoint = end;
            return Utility.ChainTextUpdate(textPoint, "{0}pt", start, end, 0.5f);
        }
    }
}
