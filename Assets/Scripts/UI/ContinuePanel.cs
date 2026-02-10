using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using ComBoom.Core;
using ComBoom.Ads;

namespace ComBoom.UI
{
    public class ContinuePanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI continueButtonText;
        [SerializeField] private TextMeshProUGUI continueDescText;

        [Header("Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button skipButton;

        private float countdownTime;
        private bool isCountingDown;
        private Action onContinueSuccess;
        private Action onSkip;

        public void Show(int score, Action onContinue, Action onSkipCallback)
        {
            onContinueSuccess = onContinue;
            onSkip = onSkipCallback;

            if (titleText != null)
                titleText.text = LocalizationManager.Get("game_over_title");

            if (scoreText != null)
                scoreText.text = score.ToString("N0");

            if (continueButtonText != null)
                continueButtonText.text = LocalizationManager.Get("continue_watch_ad");

            if (continueDescText != null)
                continueDescText.text = LocalizationManager.Get("continue_desc");

            bool adReady = AdManager.Instance != null && AdManager.Instance.IsRewardedReady();
            if (continueButton != null)
                continueButton.interactable = adReady;

            if (panel != null)
                panel.SetActive(true);

            StartCoroutine(FadeInAndCountdown());
        }

        private IEnumerator FadeInAndCountdown()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                float fadeTime = 0.4f;
                float elapsed = 0f;
                while (elapsed < fadeTime)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeTime);
                    yield return null;
                }
                canvasGroup.alpha = 1f;
            }

            float countdown = AdManager.Instance != null && AdManager.Instance.Config != null
                ? AdManager.Instance.Config.continueCountdownSeconds
                : 5f;

            countdownTime = countdown;
            isCountingDown = true;

            while (countdownTime > 0f && isCountingDown)
            {
                if (countdownText != null)
                    countdownText.text = Mathf.CeilToInt(countdownTime).ToString();

                countdownTime -= Time.deltaTime;
                yield return null;
            }

            if (isCountingDown)
            {
                OnSkipButton();
            }
        }

        public void OnContinueButton()
        {
            if (AdManager.Instance == null || !AdManager.Instance.IsRewardedReady())
            {
                OnSkipButton();
                return;
            }

            isCountingDown = false;

            if (continueButton != null)
                continueButton.interactable = false;

            AdManager.Instance.ShowRewardedForContinue(
                onSuccess: () =>
                {
                    Hide();
                    onContinueSuccess?.Invoke();
                },
                onFail: () =>
                {
                    OnSkipButton();
                }
            );
        }

        public void OnSkipButton()
        {
            isCountingDown = false;
            Hide();
            onSkip?.Invoke();
        }

        public void Hide()
        {
            StopAllCoroutines();
            isCountingDown = false;

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;

            if (panel != null)
                panel.SetActive(false);
        }
    }
}
