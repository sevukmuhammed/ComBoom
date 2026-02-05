using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ComBoom.Core;

namespace ComBoom.UI
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private AudioManager audioManager;

        [Header("Sound Toggle")]
        [SerializeField] private Image soundToggleTrack;
        [SerializeField] private RectTransform soundToggleThumb;

        [Header("Music Toggle")]
        [SerializeField] private Image musicToggleTrack;
        [SerializeField] private RectTransform musicToggleThumb;

        [Header("Vibration Toggle")]
        [SerializeField] private Image vibeToggleTrack;
        [SerializeField] private RectTransform vibeToggleThumb;

        [Header("Language")]
        [SerializeField] private TextMeshProUGUI languageLabel;

        private bool soundEnabled = true;
        private bool musicEnabled = true;
        private bool vibeEnabled = true;
        private int currentLanguageIndex;
        private bool initialized;

        private readonly Color activeTrackColor = new Color(0.545f, 0.361f, 0.965f, 1f); // purple #8B5CF6
        private readonly Color inactiveTrackColor = new Color(0.118f, 0.161f, 0.231f, 1f); // slate-800
        private readonly Color activeThumbColor = Color.white;
        private readonly Color inactiveThumbColor = new Color(0.580f, 0.639f, 0.722f, 1f); // slate-400
        private readonly Color activeGlowColor = new Color(0.545f, 0.361f, 0.965f, 0.55f); // purple glow

        private const float THUMB_ON_X = 18f;
        private const float THUMB_OFF_X = -18f;

        public void Show()
        {
            if (!initialized)
            {
                soundEnabled = PlayerPrefs.GetInt("ComBoom_SoundEnabled", 1) == 1;
                musicEnabled = PlayerPrefs.GetInt("ComBoom_MusicEnabled", 1) == 1;
                vibeEnabled = PlayerPrefs.GetInt("ComBoom_VibeEnabled", 1) == 1;
                currentLanguageIndex = LocalizationManager.GetLanguageIndex();
                initialized = true;
            }
            if (panel != null) panel.SetActive(true);
            UpdateToggleVisuals();
            UpdateLanguageLabel();
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void OnBackButton()
        {
            if (audioManager != null) audioManager.PlayClick();
            Hide();
            GameManager.Instance.ShowMainMenu();
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

        public void OnShareButton()
        {
            if (audioManager != null) audioManager.PlayClick();
        }

        public void OnLanguageButton()
        {
            if (audioManager != null) audioManager.PlayClick();
            currentLanguageIndex = (currentLanguageIndex + 1) % LocalizationManager.LanguageCodes.Length;
            LocalizationManager.SetLanguage(LocalizationManager.LanguageCodes[currentLanguageIndex]);
            UpdateLanguageLabel();
        }

        public void OnTermsButton()
        {
            if (audioManager != null) audioManager.PlayClick();
        }

        public void OnContactButton()
        {
            if (audioManager != null) audioManager.PlayClick();
        }

        private void UpdateLanguageLabel()
        {
            if (languageLabel != null)
                languageLabel.text = LocalizationManager.LanguageDisplayNames[currentLanguageIndex];
        }

        private void UpdateToggleVisuals()
        {
            SetToggle(soundToggleTrack, soundToggleThumb, soundEnabled);
            SetToggle(musicToggleTrack, musicToggleThumb, musicEnabled);
            SetToggle(vibeToggleTrack, vibeToggleThumb, vibeEnabled);
        }

        private void SetToggle(Image track, RectTransform thumb, bool isOn)
        {
            if (track != null)
                track.color = isOn ? activeTrackColor : inactiveTrackColor;
            if (thumb != null)
            {
                thumb.anchoredPosition = new Vector2(isOn ? THUMB_ON_X : THUMB_OFF_X, 0f);

                // White circle is child "ThumbCircle"
                Transform circle = thumb.Find("ThumbCircle");
                if (circle != null)
                {
                    Image circleImg = circle.GetComponent<Image>();
                    if (circleImg != null)
                        circleImg.color = isOn ? activeThumbColor : inactiveThumbColor;
                }

                // Purple glow behind thumb
                Transform glow = thumb.Find("ThumbGlow");
                if (glow != null)
                {
                    Image glowImg = glow.GetComponent<Image>();
                    if (glowImg != null)
                        glowImg.color = isOn ? activeGlowColor : Color.clear;
                }
            }
        }
    }
}
