using UnityEngine;
using TMPro;
using ComBoom.Core;

namespace ComBoom.UI
{
    public class MainMenuPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private RanksPanel ranksPanel;
        [SerializeField] private SettingsPanel settingsPanel;

        public void Show(int level, int bestScore)
        {
            if (panel != null) panel.SetActive(true);
            if (levelText != null) levelText.text = $"{LocalizationManager.Get("level")} {level}";
            if (bestScoreText != null) bestScoreText.text = bestScore.ToString("N0");
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void OnPlayButton()
        {
            if (audioManager != null) audioManager.PlayClick();
            GameManager.Instance.StartGame();
        }

        public void OnRanksButton()
        {
            if (audioManager != null) audioManager.PlayClick();
            Hide();
            if (ranksPanel != null) ranksPanel.Show();
        }

        public void OnSettingsButton()
        {
            if (audioManager != null) audioManager.PlayClick();
            Hide();
            if (settingsPanel != null) settingsPanel.Show();
        }
    }
}
