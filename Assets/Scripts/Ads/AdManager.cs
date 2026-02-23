using UnityEngine;
using UnityEngine.UI;
using System;
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

namespace ComBoom.Ads
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        [SerializeField] private AdConfig config;

        private BannerHandler bannerHandler;
        private InterstitialHandler interstitialHandler;
        private RewardedHandler rewardedHandler;

        private int gamesPlayedThisSession;
        private int interstitialsShownThisSession;
        private float lastInterstitialTime;
        private bool isInitialized;

        // Mock banner for testing without SDK
        private GameObject mockBannerObj;

        public bool IsInitialized => isInitialized;
        public AdConfig Config => config;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAds();
        }

        private void InitializeAds()
        {
#if GOOGLE_MOBILE_ADS
            MobileAds.Initialize(initStatus =>
            {
                Debug.Log("[AdManager] Google Mobile Ads initialized");
                isInitialized = true;

                bannerHandler = new BannerHandler(config);
                interstitialHandler = new InterstitialHandler(config);
                rewardedHandler = new RewardedHandler(config);

                interstitialHandler.LoadAd();
                rewardedHandler.LoadAd();
            });
#else
            Debug.LogWarning("[AdManager] Google Mobile Ads SDK not installed. Ads disabled.");
            isInitialized = false;
#endif
        }

        public void ShowBanner()
        {
#if GOOGLE_MOBILE_ADS
            if (!isInitialized || bannerHandler == null) return;
            bannerHandler.ShowBanner();
#else
            ShowMockBanner();
#endif
        }

        public void HideBanner()
        {
#if GOOGLE_MOBILE_ADS
            if (bannerHandler == null) return;
            bannerHandler.HideBanner();
#else
            HideMockBanner();
#endif
        }

#if !GOOGLE_MOBILE_ADS
        private void ShowMockBanner()
        {
            if (mockBannerObj != null)
            {
                mockBannerObj.SetActive(true);
                return;
            }

            // Find or create canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Create mock banner
            mockBannerObj = new GameObject("MockBanner");
            mockBannerObj.transform.SetParent(canvas.transform, false);

            // Banner background
            Image bg = mockBannerObj.AddComponent<Image>();
            bg.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            // Position at bottom
            RectTransform rt = mockBannerObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, 90); // Banner height

            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(mockBannerObj.transform, false);

            Text text = textObj.AddComponent<Text>();
            text.text = "MOCK BANNER - Test Mode";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.gray;
            text.alignment = TextAnchor.MiddleCenter;

            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            Debug.Log("[AdManager] Mock banner shown for testing");
        }

        private void HideMockBanner()
        {
            if (mockBannerObj != null)
            {
                mockBannerObj.SetActive(false);
            }
        }
#endif

        public void OnGameStarted()
        {
            gamesPlayedThisSession++;
        }

        public bool ShouldShowInterstitial()
        {
#if GOOGLE_MOBILE_ADS
            if (!isInitialized) return false;
            if (config == null) return false;
            if (interstitialsShownThisSession >= config.maxInterstitialsPerSession) return false;
            if (gamesPlayedThisSession < config.gamesBeforeFirstInterstitial) return false;
            if (gamesPlayedThisSession % config.interstitialFrequency != 0) return false;
            if (Time.realtimeSinceStartup - lastInterstitialTime < config.interstitialCooldown) return false;

            return interstitialHandler != null && interstitialHandler.IsReady();
#else
            return false;
#endif
        }

        public void TryShowInterstitial(Action onComplete)
        {
#if GOOGLE_MOBILE_ADS
            if (!ShouldShowInterstitial())
            {
                onComplete?.Invoke();
                return;
            }

            interstitialHandler.ShowAd(() =>
            {
                interstitialsShownThisSession++;
                lastInterstitialTime = Time.realtimeSinceStartup;
                interstitialHandler.LoadAd();
                onComplete?.Invoke();
            });
#else
            onComplete?.Invoke();
#endif
        }

        public bool IsRewardedReady()
        {
#if GOOGLE_MOBILE_ADS
            return isInitialized && rewardedHandler != null && rewardedHandler.IsReady();
#else
            return false;
#endif
        }

        public void ShowRewardedForContinue(Action onSuccess, Action onFail)
        {
#if GOOGLE_MOBILE_ADS
            if (!IsRewardedReady())
            {
                Debug.LogWarning("[AdManager] Rewarded ad not ready");
                onFail?.Invoke();
                return;
            }

            rewardedHandler.ShowAd(
                onRewardEarned: () =>
                {
                    rewardedHandler.LoadAd();
                    onSuccess?.Invoke();
                },
                onAdClosed: () =>
                {
                    rewardedHandler.LoadAd();
                },
                onAdFailed: (error) =>
                {
                    Debug.LogError($"[AdManager] Rewarded ad failed: {error}");
                    rewardedHandler.LoadAd();
                    onFail?.Invoke();
                }
            );
#else
            onFail?.Invoke();
#endif
        }

        public void ShowRewardedForPowerUp(Action<bool> onComplete)
        {
#if GOOGLE_MOBILE_ADS
            if (!IsRewardedReady())
            {
                Debug.LogWarning("[AdManager] Rewarded ad not ready for power-up");
                onComplete?.Invoke(false);
                return;
            }

            bool rewardEarned = false;

            rewardedHandler.ShowAd(
                onRewardEarned: () =>
                {
                    rewardEarned = true;
                },
                onAdClosed: () =>
                {
                    rewardedHandler.LoadAd();
                    onComplete?.Invoke(rewardEarned);
                },
                onAdFailed: (error) =>
                {
                    Debug.LogError($"[AdManager] Rewarded ad failed: {error}");
                    rewardedHandler.LoadAd();
                    onComplete?.Invoke(false);
                }
            );
#else
            onComplete?.Invoke(false);
#endif
        }

        private void OnDestroy()
        {
#if GOOGLE_MOBILE_ADS
            bannerHandler?.Destroy();
            interstitialHandler?.Destroy();
            rewardedHandler?.Destroy();
#else
            if (mockBannerObj != null)
            {
                Destroy(mockBannerObj);
            }
#endif
        }
    }
}
