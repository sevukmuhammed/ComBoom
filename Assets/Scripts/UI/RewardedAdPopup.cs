using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using ComBoom.Core;
using ComBoom.Ads;

namespace ComBoom.UI
{
    public class RewardedAdPopup : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button watchButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform contentRT;

        private Action onReward;
        private Action onClose;

        public void Show(string powerUpName, Action onRewardCallback, Action onCloseCallback = null)
        {
            onReward = onRewardCallback;
            onClose = onCloseCallback;

            string title = $"{LocalizationManager.Get("free")} {powerUpName}!";
            string desc = LocalizationManager.Get("watch_ad_desc");

            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = desc;

            if (panel != null) panel.SetActive(true);
            StartCoroutine(ShowAnimation());
        }

        public void Hide()
        {
            StopAllCoroutines();
            if (panel != null) panel.SetActive(false);
        }

        public void OnWatchButton()
        {
            if (AdManager.Instance != null)
            {
                AdManager.Instance.ShowRewardedForContinue(
                    onSuccess: () =>
                    {
                        Hide();
                        onReward?.Invoke();
                        onReward = null;
                        onClose = null;
                    },
                    onFail: () =>
                    {
                        Hide();
                        onClose?.Invoke();
                        onReward = null;
                        onClose = null;
                    }
                );
            }
            else
            {
                // Mock: SDK yoksa direkt odul ver
                Hide();
                onReward?.Invoke();
                onReward = null;
                onClose = null;
            }
        }

        public void OnCloseButton()
        {
            Hide();
            onClose?.Invoke();
            onReward = null;
            onClose = null;
        }

        private IEnumerator ShowAnimation()
        {
            if (canvasGroup == null || contentRT == null) yield break;

            canvasGroup.alpha = 0f;
            contentRT.localScale = Vector3.one * 0.7f;

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                // Ease out back
                float s = 1.7f;
                float scale = 1f + (s + 1f) * Mathf.Pow(t - 1f, 3f) + s * Mathf.Pow(t - 1f, 2f);
                contentRT.localScale = Vector3.one * Mathf.Lerp(0.7f, 1f, scale);
                canvasGroup.alpha = Mathf.Clamp01(t * 2f);

                yield return null;
            }

            canvasGroup.alpha = 1f;
            contentRT.localScale = Vector3.one;
        }
    }
}
