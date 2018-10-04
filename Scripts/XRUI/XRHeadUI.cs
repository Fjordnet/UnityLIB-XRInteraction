using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Fjord.XRInteraction.XRUI
{
    public class XRHeadUI : MonoBehaviour
    {
        [SerializeField]
        private Text _popupText;

        [SerializeField]
        private Image _flashColorImage;

        [SerializeField]
        private Image _fadeImage;

        private Coroutine _fadeViewCoroutine;
        private Coroutine _flashColorFadeCoroutine;
        private Coroutine _temporaryMessageFadeCoroutine;

        public void FadeOutView(float fadeOutMultiplier = 4)
        {
            if (null != _fadeViewCoroutine) StopCoroutine(_fadeViewCoroutine);
            _fadeViewCoroutine = StartCoroutine(FadeGaphicAlphaCoroutine(_flashColorImage, 1, fadeOutMultiplier));
        }

        public void FadeInView(float fadeOutMultiplier = 4)
        {
            if (null != _fadeViewCoroutine) StopCoroutine(_fadeViewCoroutine);
            _fadeViewCoroutine = StartCoroutine(FadeGaphicAlphaCoroutine(_flashColorImage, 0, fadeOutMultiplier));
        }

        public void FlashColor(Color color, float holdTime = 0, float fadeOutMultiplier = 4)
        {
            _flashColorImage.color = color;
            if (null != _flashColorFadeCoroutine) StopCoroutine(_flashColorFadeCoroutine);
            _flashColorFadeCoroutine =
                StartCoroutine(FlashGaphicAlphaCoroutine(_flashColorImage, holdTime, fadeOutMultiplier));
        }

        /// <summary>
        /// Displays a message that will fade out depending on configuration of HUDUI.
        /// </summary>
        public void FlashTemporaryMessage(string message, float holdTime = 2, float fadeOutMultiplier = 2)
        {
            _popupText.text = message;
            _popupText.enabled = true;
            if (null != _temporaryMessageFadeCoroutine) StopCoroutine(_temporaryMessageFadeCoroutine);
            _temporaryMessageFadeCoroutine =
                StartCoroutine(FlashGaphicAlphaCoroutine(_popupText, holdTime, fadeOutMultiplier));
        }

        private IEnumerator FlashGaphicAlphaCoroutine(Graphic graphic, float holdTime, float fadeOutMultiplier)
        {
            graphic.enabled = true;
            graphic.color = graphic.color.ChangeAlpha(1);
            yield return new WaitForSeconds(holdTime);
            float alpha = 1;
            while (alpha > 0)
            {
                alpha -= Time.deltaTime * fadeOutMultiplier;
                graphic.color = graphic.color.ChangeAlpha(alpha);
                yield return null;
            }
            graphic.enabled = false;
        }

        private IEnumerator FadeGaphicAlphaCoroutine(Graphic graphic, float targetAlpha, float fadeOutMultiplier)
        {
            graphic.enabled = true;
            float alpha = graphic.color.a;
            float direction = targetAlpha > alpha ? 1 : -1;
            while ((direction < 0 && alpha > targetAlpha) || (direction > 0 && alpha < targetAlpha))
            {
                alpha += Time.deltaTime * fadeOutMultiplier * direction;
                graphic.color = graphic.color.ChangeAlpha(alpha);
                yield return null;
            }
            graphic.color = graphic.color.ChangeAlpha(targetAlpha);
            if (graphic.color.a == 0) graphic.enabled = false;
        }
    }
}