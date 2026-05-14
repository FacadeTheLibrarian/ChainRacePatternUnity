using ChainPattern;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sample {
    public static class Utility {
        /// <summary>
        /// Creates a tween chain that animates RectTransform's anchoredPosition using the specified curve
        /// </summary>
        public static BaseChain ChainMoveTween(RectTransform rect, Vector2 endPosition, float duration, AnimationCurve curve) {
            Vector2 startPosition = Vector2.zero;

            float startTime = 0.0f;
            duration = Mathf.Max(duration, 0.01f);
            ChainWorkLifeCycle lifeCycle = new ChainWorkLifeCycle(
                onStart: () => {
                    startPosition = rect.anchoredPosition;
                    startTime = Time.time;
                },
                onUpdate: () => {
                    float t = curve.Evaluate((Time.time - startTime) / duration);
                    if (rect != null) {
                        rect.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
                    }
                    if (t >= 1.0f) {
                        return true;
                    }
                    return false;
                },
                onSkip: () => {
                    if (rect != null) {
                        rect.anchoredPosition = endPosition;
                    }
                }
                );

            ChainWork work = new ChainWork(lifeCycle);
            return work;
        }

        /// <summary>
        /// Creates a tween chain that animates RectTransform's anchoredPosition
        /// </summary>
        public static BaseChain ChainMoveTween(RectTransform rect, Vector2 endPosition, float duration) {
            return ChainMoveTween(rect, endPosition, duration, AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f));
        }

        /// <summary>
        /// Creates a chain that counts a numeric text from start to end over the specified duration using the given format
        /// </summary>
        public static BaseChain ChainTextUpdate(TextMeshProUGUI ugui, string format, int start, int end, float duration) {
            float startTime = 0.0f;
            duration = Mathf.Max(duration, 0.01f);
            ChainWorkLifeCycle lifeCycle = new ChainWorkLifeCycle(
                onStart: () => {
                    startTime = Time.time;
                },
                onUpdate: () => {
                    float t = (Time.time - startTime) / duration;
                    int n = (int)Mathf.Lerp(start, end, t);
                    if (t >= 1.0f) {
                        ugui.text = string.Format(format, n);
                        return true;
                    }
                    else {
                        ugui.text = string.Format(format, n);
                        return false;
                    }
                },
                onSkip: () => {
                    ugui.text = string.Format(format, end);
                }
                );
            ChainWork work = new ChainWork(lifeCycle);
            return work;
        }

        /// <summary>
        /// Creates a chain that plays a sound
        /// </summary>
        public static BaseChain ChainPlaySound(SoundType soundType) {
            return new ChainAction(() => {
                // Only play if not skipped immediately
                if (SoundPlayer.Get() != null) {
                    SoundPlayer.Get().PlaySound(soundType);
                }
            });
        }

        /// <summary>
        /// Creates a chain that animates alpha of a graphic 
        /// </summary>
        public static BaseChain ChainAlphaAnimation(Graphic graphic, float alphaEnd, float duration) {

            float alphaStart = 0.0f;
            AnimationCurve curve = null;
            ChainWorkLifeCycle lifeCycle = new ChainWorkLifeCycle(
                onStart: () => {
                    alphaStart = graphic.color.a;
                    curve = AnimationCurve.Linear(Time.time, 0, Time.time + duration, 1);
                },
                onUpdate: () => {
                    float t = curve.Evaluate(Time.time);
                    Color c = graphic.color;
                    c.a = Mathf.Lerp(alphaStart, alphaEnd, t);
                    graphic.color = c;
                    if (t >= 1.0f) {
                        return true;
                    }
                    return false;
                },
                onSkip: () => {
                    Color c = graphic.color;
                    c.a = alphaEnd;
                    graphic.color = c;
                }
            );
            ChainWork work = new ChainWork(lifeCycle);
            return work;
        }
    }
}

