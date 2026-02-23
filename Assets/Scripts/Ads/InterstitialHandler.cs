using UnityEngine;
using System;
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

namespace ComBoom.Ads
{
    public class InterstitialHandler
    {
        private readonly AdConfig config;
        private bool isLoading;

#if GOOGLE_MOBILE_ADS
        private InterstitialAd interstitialAd;
#endif

        public InterstitialHandler(AdConfig config)
        {
            this.config = config;
        }

        public bool IsReady()
        {
#if GOOGLE_MOBILE_ADS
            return interstitialAd != null && interstitialAd.CanShowAd();
#else
            return false;
#endif
        }

        public void LoadAd()
        {
#if GOOGLE_MOBILE_ADS
            if (isLoading) return;
            if (interstitialAd != null && interstitialAd.CanShowAd()) return;

            isLoading = true;

            var adRequest = new AdRequest();

            InterstitialAd.Load(config.InterstitialId, adRequest, (InterstitialAd ad, LoadAdError error) =>
            {
                isLoading = false;

                if (error != null)
                {
                    Debug.LogError($"[InterstitialHandler] Failed to load: {error.GetMessage()}");
                    return;
                }

                if (ad == null)
                {
                    Debug.LogError("[InterstitialHandler] Ad is null despite no error");
                    return;
                }

                Debug.Log("[InterstitialHandler] Ad loaded successfully");
                interstitialAd = ad;

                RegisterEventHandlers(ad);
            });
#endif
        }

#if GOOGLE_MOBILE_ADS
        private void RegisterEventHandlers(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[InterstitialHandler] Ad closed");
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError($"[InterstitialHandler] Ad failed to show: {error.GetMessage()}");
            };
        }
#endif

        public void ShowAd(Action onComplete)
        {
#if GOOGLE_MOBILE_ADS
            if (!IsReady())
            {
                Debug.LogWarning("[InterstitialHandler] Ad not ready");
                onComplete?.Invoke();
                return;
            }

            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                onComplete?.Invoke();
            };

            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                onComplete?.Invoke();
            };

            interstitialAd.Show();
#else
            onComplete?.Invoke();
#endif
        }

        public void Destroy()
        {
#if GOOGLE_MOBILE_ADS
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }
#endif
        }
    }
}
