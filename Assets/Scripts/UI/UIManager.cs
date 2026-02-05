using UnityEngine;
using ComBoom.Core;

namespace ComBoom.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameOverPanel gameOverPanel;
        [SerializeField] private MainMenuPanel mainMenuPanel;

        [Header("Displays")]
        [SerializeField] private ScoreDisplay scoreDisplay;

        [Header("References")]
        [SerializeField] private ScoreManager scoreManager;

        private void Start()
        {
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged += OnScoreChanged;
                scoreManager.OnHighScoreChanged += OnHighScoreChanged;
            }
        }

        private void OnDestroy()
        {
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged -= OnScoreChanged;
                scoreManager.OnHighScoreChanged -= OnHighScoreChanged;
            }
        }

        private void OnScoreChanged(int score)
        {
            if (scoreDisplay != null)
                scoreDisplay.UpdateScore(score);
        }

        private void OnHighScoreChanged(int highScore)
        {
            if (scoreDisplay != null)
                scoreDisplay.UpdateHighScore(highScore);
        }

        public void ShowMainMenu(int level, int bestScore)
        {
            if (gameUI != null) gameUI.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.Hide();
            if (mainMenuPanel != null) mainMenuPanel.Show(level, bestScore);
        }

        public void ShowGameUI()
        {
            if (mainMenuPanel != null) mainMenuPanel.Hide();
            if (gameUI != null) gameUI.SetActive(true);
            if (gameOverPanel != null) gameOverPanel.Hide();
        }

        public void ShowGameOver(int score, int highScore)
        {
            if (gameOverPanel != null)
                gameOverPanel.Show(score, highScore);
        }

        public void OnRestartButton()
        {
            GameManager.Instance.RestartGame();
        }

        public void OnSettingsButton()
        {
            // Ayarlar menusunu ac (ileride eklenecek)
        }
    }
}
