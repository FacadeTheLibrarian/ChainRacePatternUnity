using ChainPattern;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    /// <summary>
    /// a panel that fades in and out
    /// </summary>
    public class FadePanel : MonoBehaviour
    {
        [SerializeField]
        Image image;


        /// <summary>
        /// set the alpha value of the panel
        /// </summary>        
        public void SetAlpha(float alpha)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }

        /// <summary>
        /// Creates a chain that fades in and out
        /// the fade-in changes the alpha value from 0 to1
        /// </summary>
        public BaseChain ChainFade(bool fadeIn)
        {
            if (fadeIn)
            {
                return new ChainSequence(
                    new ChainAction(() =>
                    {
                        gameObject.SetActive(true);
                        SetAlpha(0.0f);
                    }),
                    Utility.ChainAlphaAnimation(image, 1.0f, 0.5f)
                );
            }
            else
            {
                return new ChainSequence(
                    new ChainAction(() =>
                    {
                        gameObject.SetActive(true);
                        SetAlpha(1.0f);
                    }),
                    Utility.ChainAlphaAnimation(image, 0.0f, 0.5f),
                    new ChainAction(() =>
                    {
                        gameObject.SetActive(false);
                    })
                );
            }
        }

    }
}
