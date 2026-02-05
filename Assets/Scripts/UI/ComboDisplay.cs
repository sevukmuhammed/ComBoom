using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace ComBoom.UI
{
    public class ComboDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private RectTransform contentRT;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI multiplierText;
        [SerializeField] private Image glowImage;

        private Coroutine activeRoutine;

        // Combo renkleri (seviyeye gore)
        private static readonly Color combo2Color = new Color(1f, 0.85f, 0.1f, 1f);   // altin
        private static readonly Color combo3Color = new Color(1f, 0.5f, 0.1f, 1f);    // turuncu
        private static readonly Color combo4Color = new Color(1f, 0.2f, 0.2f, 1f);    // kirmizi
        private static readonly Color glowCombo2 = new Color(1f, 0.85f, 0.1f, 0.25f);
        private static readonly Color glowCombo3 = new Color(1f, 0.5f, 0.1f, 0.30f);
        private static readonly Color glowCombo4 = new Color(1f, 0.2f, 0.2f, 0.35f);

        public void ShowCombo(int comboCount)
        {
            if (comboCount < 2) return;

            int multiplier = 1 << (comboCount - 1);

            Color textColor;
            Color glow;
            if (comboCount >= 4)
            {
                textColor = combo4Color;
                glow = glowCombo4;
            }
            else if (comboCount >= 3)
            {
                textColor = combo3Color;
                glow = glowCombo3;
            }
            else
            {
                textColor = combo2Color;
                glow = glowCombo2;
            }

            if (comboText != null)
            {
                comboText.text = "COMBO";
                comboText.color = textColor;
            }
            if (multiplierText != null)
            {
                multiplierText.text = $"x{multiplier}!";
                multiplierText.color = textColor;
            }

            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            activeRoutine = StartCoroutine(AnimateCombo(comboCount, textColor, glow));
        }

        private IEnumerator AnimateCombo(int comboCount, Color textColor, Color glowColor)
        {
            if (panel != null) panel.SetActive(true);

            // Baslangic: kucuk + transparan
            if (contentRT != null) contentRT.localScale = Vector3.one * 0.4f;
            SetAlpha(0f);
            if (glowImage != null) glowImage.color = Color.clear;

            float peakScale = comboCount >= 4 ? 1.35f : (comboCount >= 3 ? 1.25f : 1.15f);
            float shakeMagnitude = comboCount >= 4 ? 8f : (comboCount >= 3 ? 5f : 2f);

            // === Phase 1: Pop in (0.25s) ===
            float popDur = 0.25f;
            float elapsed = 0f;
            while (elapsed < popDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / popDur);

                float scale = Mathf.Lerp(0.4f, peakScale, EaseOutBack(t));
                if (contentRT != null) contentRT.localScale = Vector3.one * scale;

                float alpha = Mathf.Clamp01(t * 3f);
                SetAlpha(alpha);

                if (glowImage != null)
                    glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, glowColor.a * t);

                yield return null;
            }

            // === Phase 2: Settle + shake (0.7s) ===
            float holdDur = 0.7f;
            float holdStart = elapsed;
            Vector2 basePos = contentRT != null ? contentRT.anchoredPosition : Vector2.zero;

            while (elapsed < holdStart + holdDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01((elapsed - holdStart) / 0.2f);

                float scale = Mathf.Lerp(peakScale, 1f, EaseOutQuad(t));
                if (contentRT != null) contentRT.localScale = Vector3.one * scale;

                // Shake (combo seviyesine gore)
                if (contentRT != null && shakeMagnitude > 0f)
                {
                    float shakeDecay = 1f - Mathf.Clamp01((elapsed - holdStart) / holdDur);
                    float shakeX = Mathf.Sin((elapsed - holdStart) * 35f) * shakeMagnitude * shakeDecay;
                    float shakeY = Mathf.Cos((elapsed - holdStart) * 28f) * shakeMagnitude * 0.5f * shakeDecay;
                    contentRT.anchoredPosition = basePos + new Vector2(shakeX, shakeY);
                }

                yield return null;
            }

            if (contentRT != null) contentRT.anchoredPosition = basePos;

            // === Phase 3: Fade out (0.25s) ===
            float fadeDur = 0.25f;
            float fadeStart = elapsed;
            while (elapsed < fadeStart + fadeDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01((elapsed - fadeStart) / fadeDur);

                float alpha = 1f - t;
                SetAlpha(alpha);

                float scale = Mathf.Lerp(1f, 0.7f, t);
                if (contentRT != null) contentRT.localScale = Vector3.one * scale;

                if (glowImage != null)
                    glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, glowColor.a * alpha);

                yield return null;
            }

            if (panel != null) panel.SetActive(false);
            activeRoutine = null;
        }

        private void SetAlpha(float alpha)
        {
            if (comboText != null) comboText.alpha = alpha;
            if (multiplierText != null) multiplierText.alpha = alpha;
        }

        private static float EaseOutBack(float t)
        {
            float c = 1.7f;
            return 1f + (c + 1f) * Mathf.Pow(t - 1f, 3f) + c * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
    }
}
