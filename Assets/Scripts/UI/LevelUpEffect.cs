using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using ComBoom.Core;

namespace ComBoom.UI
{
    public class LevelUpEffect : MonoBehaviour
    {
        public static void Spawn(int newLevel)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameObject obj = new GameObject("LevelUpEffect");
            obj.transform.SetParent(canvas.transform, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            LevelUpEffect effect = obj.AddComponent<LevelUpEffect>();
            effect.StartCoroutine(effect.AnimateRoutine(newLevel));
        }

        private IEnumerator AnimateRoutine(int level)
        {
            // --- OVERLAY ---
            Image overlay = gameObject.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0f);
            overlay.raycastTarget = true;

            // --- CONTENT CONTAINER ---
            GameObject content = new GameObject("Content");
            content.transform.SetParent(transform, false);
            RectTransform contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.1f, 0.3f);
            contentRT.anchorMax = new Vector2(0.9f, 0.7f);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            // --- LEVEL UP! TEXT ---
            GameObject titleObj = new GameObject("LevelUpTitle");
            titleObj.transform.SetParent(content.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 0.55f);
            titleRT.anchorMax = new Vector2(1f, 0.95f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;

            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "LEVEL UP!";
            titleTMP.fontSize = 72;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(0.961f, 0.620f, 0.043f, 1f); // gold #F59E0B
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.enableAutoSizing = false;

            // --- LEVEL NUMBER ---
            GameObject levelObj = new GameObject("LevelNumber");
            levelObj.transform.SetParent(content.transform, false);
            RectTransform levelRT = levelObj.AddComponent<RectTransform>();
            levelRT.anchorMin = new Vector2(0f, 0.30f);
            levelRT.anchorMax = new Vector2(1f, 0.58f);
            levelRT.offsetMin = Vector2.zero;
            levelRT.offsetMax = Vector2.zero;

            TextMeshProUGUI levelTMP = levelObj.AddComponent<TextMeshProUGUI>();
            levelTMP.text = $"LEVEL {level}";
            levelTMP.fontSize = 48;
            levelTMP.alignment = TextAlignmentOptions.Center;
            levelTMP.color = Color.white;
            levelTMP.fontStyle = FontStyles.Bold;
            levelTMP.characterSpacing = 8f;

            // --- REWARDS TEXT ---
            GameObject rewardsObj = new GameObject("Rewards");
            rewardsObj.transform.SetParent(content.transform, false);
            RectTransform rewardsRT = rewardsObj.AddComponent<RectTransform>();
            rewardsRT.anchorMin = new Vector2(0f, 0.05f);
            rewardsRT.anchorMax = new Vector2(1f, 0.28f);
            rewardsRT.offsetMin = Vector2.zero;
            rewardsRT.offsetMax = Vector2.zero;

            TextMeshProUGUI rewardsTMP = rewardsObj.AddComponent<TextMeshProUGUI>();
            rewardsTMP.text = "+1 UNDO   +1 BOMB   +1 SHUFFLE";
            rewardsTMP.fontSize = 24;
            rewardsTMP.alignment = TextAlignmentOptions.Center;
            rewardsTMP.color = new Color(0.063f, 0.725f, 0.506f, 1f); // green #10B981
            rewardsTMP.fontStyle = FontStyles.Bold;

            // Start hidden
            content.transform.localScale = Vector3.one * 0.3f;
            SetAllAlpha(0f, overlay, titleTMP, levelTMP, rewardsTMP);

            // Haptic
            HapticManager.NotificationSuccess();

            float totalDuration = 2.5f;
            float elapsed = 0f;

            // Phase 1: Fade in + pop (0.4s)
            float fadeInDur = 0.4f;
            while (elapsed < fadeInDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDur);

                overlay.color = new Color(0f, 0f, 0f, t * 0.70f);

                float scale = Mathf.Lerp(0.3f, 1.15f, EaseOutBack(t));
                content.transform.localScale = Vector3.one * scale;

                float alpha = Mathf.Clamp01(t * 2f);
                SetTextAlpha(alpha, titleTMP, levelTMP, rewardsTMP);

                yield return null;
            }

            // Phase 2: Settle + hold (1.4s)
            float holdDur = 1.4f;
            float holdStart = elapsed;
            while (elapsed < holdStart + holdDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01((elapsed - holdStart) / 0.3f);

                float scale = Mathf.Lerp(1.15f, 1f, EaseOutQuad(t));
                content.transform.localScale = Vector3.one * scale;

                // Hafif pulsasyon
                float pulse = 1f + Mathf.Sin((elapsed - holdStart) * 4f) * 0.02f;
                content.transform.localScale *= pulse;

                yield return null;
            }

            // Phase 3: Fade out (0.7s)
            float fadeDur = 0.7f;
            float fadeStart = elapsed;
            while (elapsed < fadeStart + fadeDur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01((elapsed - fadeStart) / fadeDur);

                float alpha = 1f - t;
                overlay.color = new Color(0f, 0f, 0f, alpha * 0.70f);
                SetTextAlpha(alpha, titleTMP, levelTMP, rewardsTMP);

                float scale = Mathf.Lerp(1f, 0.8f, t);
                content.transform.localScale = Vector3.one * scale;

                yield return null;
            }

            Destroy(gameObject);
        }

        private void SetAllAlpha(float alpha, Image overlay, params TextMeshProUGUI[] texts)
        {
            if (overlay != null)
                overlay.color = new Color(0f, 0f, 0f, alpha * 0.70f);
            SetTextAlpha(alpha, texts);
        }

        private void SetTextAlpha(float alpha, params TextMeshProUGUI[] texts)
        {
            foreach (var tmp in texts)
            {
                if (tmp == null) continue;
                Color c = tmp.color;
                c.a = alpha;
                tmp.color = c;
            }
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
