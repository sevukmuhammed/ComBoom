using UnityEngine;
using TMPro;
using System.Collections;
using ComBoom.Core;

namespace ComBoom.UI
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;

        public void Show(int score, int highScore)
        {
            if (finalScoreText != null)
                finalScoreText.text = score.ToString();

            if (bestScoreText != null)
                bestScoreText.text = $"{LocalizationManager.Get("best_prefix")} {highScore}";

            if (panel != null) panel.SetActive(true);
            StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;
            canvasGroup.alpha = 0f;

            float duration = 0.6f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        public void Hide()
        {
            StopAllCoroutines();
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            if (panel != null) panel.SetActive(false);
        }

        public void OnRestartButton()
        {
            Hide();
            GameManager.Instance.RestartGame();
        }
    }
}
