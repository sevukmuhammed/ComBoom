using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using ComBoom.Core;

namespace ComBoom.UI
{
    public class LevelProgressBar : MonoBehaviour
    {
        [Header("Progress Bar")]
        [SerializeField] private TextMeshProUGUI levelLabel;
        [SerializeField] private Image fillBar;
        [SerializeField] private Image fillBarGlow;

        [Header("Level Up Banner")]
        [SerializeField] private GameObject bannerPanel;
        [SerializeField] private RectTransform bannerContent;
        [SerializeField] private Image bannerOverlay;
        [SerializeField] private Image bannerGlow;
        [SerializeField] private TextMeshProUGUI bannerTitle;
        [SerializeField] private TextMeshProUGUI bannerLevel;
        [SerializeField] private TextMeshProUGUI bannerStarLeft;
        [SerializeField] private TextMeshProUGUI bannerStarRight;

        private LevelManager levelManager;
        private ScoreManager scoreManager;
        private int baseXP;
        private int displayedLevel;

        private readonly Color normalFillColor = new Color(0.545f, 0.361f, 0.965f, 1f); // purple #8B5CF6
        private readonly Color levelUpFillColor = new Color(0.961f, 0.620f, 0.043f, 1f); // gold #F59E0B

        public void Init(LevelManager lm, ScoreManager sm)
        {
            levelManager = lm;
            scoreManager = sm;
            baseXP = lm.TotalXP;
            displayedLevel = lm.CurrentLevel;

            sm.OnScoreChanged += OnScoreChanged;

            if (bannerPanel != null) bannerPanel.SetActive(false);
            UpdateBar(0);
        }

        private void OnScoreChanged(int currentScore)
        {
            UpdateBar(currentScore);
        }

        private void UpdateBar(int currentScore)
        {
            if (levelManager == null) return;

            int projectedXP = baseXP + currentScore;
            int projectedLevel = levelManager.CalculateLevel(projectedXP);

            int currentThreshold = levelManager.XPForLevel(projectedLevel);
            int nextThreshold = levelManager.XPForLevel(projectedLevel + 1);
            int range = nextThreshold - currentThreshold;

            float fill = range > 0 ? Mathf.Clamp01((float)(projectedXP - currentThreshold) / range) : 0f;

            if (fillBar != null)
                fillBar.rectTransform.anchorMax = new Vector2(fill, 1f);

            if (levelLabel != null)
                levelLabel.text = $"LV.{projectedLevel}";

            if (projectedLevel > displayedLevel)
            {
                displayedLevel = projectedLevel;
                StartCoroutine(LevelUpBannerAnimation(projectedLevel));
            }
        }

        private IEnumerator LevelUpBannerAnimation(int newLevel)
        {
            HapticManager.NotificationSuccess();

            // Flash progress bar to gold
            if (fillBar != null) fillBar.color = levelUpFillColor;
            if (fillBarGlow != null)
                fillBarGlow.color = new Color(levelUpFillColor.r, levelUpFillColor.g, levelUpFillColor.b, 0.6f);

            // Setup banner
            if (bannerPanel == null) yield break;
            bannerPanel.SetActive(true);

            if (bannerLevel != null)
                bannerLevel.text = $"LEVEL {newLevel}";

            // Start hidden
            if (bannerContent != null) bannerContent.localScale = Vector3.one * 0.3f;
            SetBannerAlpha(0f);

            // === Phase 1: Pop in (0.4s) ===
            float fadeInDur = 0.4f;
            float elapsed = 0f;
            while (elapsed < fadeInDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDur);

                if (bannerOverlay != null)
                    bannerOverlay.color = new Color(0f, 0f, 0f, t * 0.70f);

                float scale = Mathf.Lerp(0.3f, 1.15f, EaseOutBack(t));
                if (bannerContent != null)
                    bannerContent.localScale = Vector3.one * scale;

                float alpha = Mathf.Clamp01(t * 2f);
                SetBannerTextAlpha(alpha);

                // Glow fade in
                if (bannerGlow != null)
                    bannerGlow.color = new Color(0.545f, 0.361f, 0.965f, t * 0.35f);

                yield return null;
            }

            // === Phase 2: Settle + pulse (1.4s) ===
            float holdDur = 1.4f;
            float holdStart = elapsed;
            while (elapsed < holdStart + holdDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01((elapsed - holdStart) / 0.3f);

                float scale = Mathf.Lerp(1.15f, 1f, EaseOutQuad(t));
                float pulse = 1f + Mathf.Sin((elapsed - holdStart) * 4f) * 0.02f;
                if (bannerContent != null)
                    bannerContent.localScale = Vector3.one * scale * pulse;

                // Star twinkle
                float starAlpha = 0.6f + Mathf.Sin((elapsed - holdStart) * 6f) * 0.4f;
                if (bannerStarLeft != null) bannerStarLeft.alpha = starAlpha;
                if (bannerStarRight != null) bannerStarRight.alpha = starAlpha;

                yield return null;
            }

            // === Phase 3: Fade out (0.5s) ===
            float fadeDur = 0.5f;
            float fadeStart = elapsed;
            while (elapsed < fadeStart + fadeDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01((elapsed - fadeStart) / fadeDur);

                float alpha = 1f - t;
                if (bannerOverlay != null)
                    bannerOverlay.color = new Color(0f, 0f, 0f, alpha * 0.70f);

                SetBannerTextAlpha(alpha);

                float scale = Mathf.Lerp(1f, 0.8f, t);
                if (bannerContent != null)
                    bannerContent.localScale = Vector3.one * scale;

                if (bannerGlow != null)
                    bannerGlow.color = new Color(0.545f, 0.361f, 0.965f, alpha * 0.35f);

                yield return null;
            }

            bannerPanel.SetActive(false);

            // Restore progress bar color
            if (fillBar != null) fillBar.color = normalFillColor;
            if (fillBarGlow != null) fillBarGlow.color = Color.clear;
        }

        private void SetBannerAlpha(float alpha)
        {
            if (bannerOverlay != null)
                bannerOverlay.color = new Color(0f, 0f, 0f, alpha * 0.70f);
            SetBannerTextAlpha(alpha);
            if (bannerGlow != null)
                bannerGlow.color = new Color(0.545f, 0.361f, 0.965f, alpha * 0.35f);
        }

        private void SetBannerTextAlpha(float alpha)
        {
            if (bannerTitle != null) bannerTitle.alpha = alpha;
            if (bannerLevel != null) bannerLevel.alpha = alpha;
            if (bannerStarLeft != null) bannerStarLeft.alpha = alpha;
            if (bannerStarRight != null) bannerStarRight.alpha = alpha;
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

        private void OnDestroy()
        {
            if (scoreManager != null)
                scoreManager.OnScoreChanged -= OnScoreChanged;
        }
    }
}
