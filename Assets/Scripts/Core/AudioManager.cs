using UnityEngine;
using ComBoom.Gameplay;

namespace ComBoom.Core
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Clips")]
        [SerializeField] private AudioClip pickClip;
        [SerializeField] private AudioClip placeClip;
        [SerializeField] private AudioClip clearClip;
        [SerializeField] private AudioClip comboClip;
        [SerializeField] private AudioClip gameOverClip;
        [SerializeField] private AudioClip clickClip;

        [Header("Music")]
        [SerializeField] private AudioClip musicClip;

        [Header("Settings")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        private bool isMuted;
        private bool isMusicMuted;

        public bool IsMuted => isMuted;
        public bool IsMusicMuted => isMusicMuted;

        private void Awake()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
                musicSource.volume = 0.25f;
            }

            if (pickClip == null) pickClip = AudioClipGenerator.CreatePickClip();
            if (placeClip == null) placeClip = AudioClipGenerator.CreatePlaceClip();
            if (clearClip == null) clearClip = AudioClipGenerator.CreateClearClip();
            if (comboClip == null) comboClip = AudioClipGenerator.CreateComboClip();
            if (gameOverClip == null) gameOverClip = AudioClipGenerator.CreateGameOverClip();
            if (clickClip == null) clickClip = AudioClipGenerator.CreateClickClip();
            if (musicClip == null) musicClip = AudioClipGenerator.CreateMusicLoop();

            musicSource.clip = musicClip;
            isMusicMuted = PlayerPrefs.GetInt("ComBoom_MusicEnabled", 1) != 1;
        }

        public void PlayPick()   => PlayClip(pickClip);
        public void PlayPlace()  => PlayClip(placeClip);
        public void PlayClear()  => PlayClip(clearClip);
        public void PlayCombo()  => PlayClip(comboClip);
        public void PlayGameOver() => PlayClip(gameOverClip);
        public void PlayClick()  => PlayClip(clickClip);

        private void PlayClip(AudioClip clip)
        {
            if (isMuted || clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip);
        }

        public void StartMusic()
        {
            if (musicSource == null || musicClip == null) return;
            if (!musicSource.isPlaying && !isMusicMuted)
                musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }

        public void SetMusicMute(bool mute)
        {
            isMusicMuted = mute;
            if (musicSource != null)
            {
                if (mute) musicSource.Pause();
                else if (!musicSource.isPlaying) musicSource.Play();
            }
        }

        public void ToggleMute()
        {
            isMuted = !isMuted;
        }

        public void SetMute(bool mute)
        {
            isMuted = mute;
        }
    }
}
