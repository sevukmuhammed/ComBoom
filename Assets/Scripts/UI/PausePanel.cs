using UnityEngine;
using UnityEngine.UI;
using ComBoom.Core;

namespace ComBoom.UI
{
    public class PausePanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Image soundIcon;
        [SerializeField] private Image musicIcon;
        [SerializeField] private Image vibeIcon;
        [SerializeField] private AudioManager audioManager;

        private bool soundEnabled = true;
        private bool musicEnabled = true;
        private bool vibeEnabled = true;
        private bool initialized;

        private readonly Color soundActiveColor = new Color(0.063f, 0.725f, 0.506f, 1f);   // #10B981 jewel-green
        private readonly Color musicActiveColor = new Color(0.925f, 0.282f, 0.600f, 1f);   // #EC4899 jewel-pink
        private readonly Color vibeActiveColor = new Color(0.231f, 0.510f, 0.965f, 1f);    // #3B82F6 jewel-blue
        private readonly Color inactiveColor = new Color(0.278f, 0.333f, 0.412f, 1f);      // #475569 slate-600

        public void Show()
        {
            if (!initialized)
            {
                soundEnabled = PlayerPrefs.GetInt("ComBoom_SoundEnabled", 1) == 1;
                musicEnabled = PlayerPrefs.GetInt("ComBoom_MusicEnabled", 1) == 1;
                vibeEnabled = PlayerPrefs.GetInt("ComBoom_VibeEnabled", 1) == 1;
                initialized = true;
            }
            if (panel != null) panel.SetActive(true);
            UpdateToggleVisuals();
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void OnResumeButton()
        {
            if (audioManager != null) audioManager.PlayClick();
            GameManager.Instance.ResumeGame();
        }

        public void OnRestartButton()
        {
            if (audioManager != null) audioManager.PlayClick();
            GameManager.Instance.RestartGame();
        }

        public void OnHomeButton()
        {
            if (audioManager != null) audioManager.PlayClick();
            GameManager.Instance.GoToMenu();
        }

        public void OnToggleSound()
        {
            soundEnabled = !soundEnabled;
            if (audioManager != null)
            {
                audioManager.SetMute(!soundEnabled);
                audioManager.PlayClick();
            }
            PlayerPrefs.SetInt("ComBoom_SoundEnabled", soundEnabled ? 1 : 0);
            PlayerPrefs.Save();
            UpdateToggleVisuals();
        }

        public void OnToggleMusic()
        {
            musicEnabled = !musicEnabled;
            if (audioManager != null)
            {
                audioManager.SetMusicMute(!musicEnabled);
                audioManager.PlayClick();
            }
            PlayerPrefs.SetInt("ComBoom_MusicEnabled", musicEnabled ? 1 : 0);
            PlayerPrefs.Save();
            UpdateToggleVisuals();
        }

        public void OnToggleVibe()
        {
            vibeEnabled = !vibeEnabled;
            if (audioManager != null) audioManager.PlayClick();
            HapticManager.SetEnabled(vibeEnabled);
            PlayerPrefs.SetInt("ComBoom_VibeEnabled", vibeEnabled ? 1 : 0);
            PlayerPrefs.Save();
            UpdateToggleVisuals();

            if (vibeEnabled)
                HapticManager.ImpactLight();
        }

        private void UpdateToggleVisuals()
        {
            if (soundIcon != null)
                soundIcon.color = soundEnabled ? soundActiveColor : inactiveColor;
            if (musicIcon != null)
                musicIcon.color = musicEnabled ? musicActiveColor : inactiveColor;
            if (vibeIcon != null)
                vibeIcon.color = vibeEnabled ? vibeActiveColor : inactiveColor;
        }
    }
}
