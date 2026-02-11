using UnityEngine;
using System;
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

namespace ComBoom.Ads
{
    public class RewardedHandler
    {
        private readonly AdConfig config;
        private bool isLoading;

        private Action onRewardEarnedCallback;
        private Action onAdClosedCallback;
        private Action<string> onAdFailedCallback;

#if GOOGLE_MOBILE_ADS
        private RewardedAd rewardedAd;
#endif

        public RewardedHandler(AdConfig config)
        {
            this.config = config;
        }

        public bool IsReady()
        {
#if GOOGLE_MOBILE_ADS
            return rewardedAd != null && rewardedAd.CanShowAd();
#else
            return false;
#endif
        }

        public void LoadAd()
        {
#if GOOGLE_MOBILE_ADS
            if (isLoading) return;
            if (rewardedAd != null && rewardedAd.CanShowAd()) return;

            isLoading = true;

            var adRequest = new AdRequest();

            RewardedAd.Load(config.RewardedId, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                isLoading = false;

                if (error != null)
                {
                    Debug.LogError($"[RewardedHandler] Failed to load: {error.GetMessage()}");
                    return;
                }

                if (ad == null)
                {
                    Debug.LogError("[RewardedHandler] Ad is null despite no error");
                    return;
                }

                Debug.Log("[RewardedHandler] Ad loaded successfully");
                rewardedAd = ad;

                RegisterEventHandlers(ad);
            });
#endif
        }

#if GOOGLE_MOBILE_ADS
        private void RegisterEventHandlers(RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[RewardedHandler] Ad closed");
                onAdClosedCallback?.Invoke();
                ClearCallbacks();
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError($"[RewardedHandler] Ad failed to show: {error.GetMessage()}");
                onAdFailedCallback?.Invoke(error.GetMessage());
                ClearCallbacks();
            };
        }
#endif

        public void ShowAd(Action onRewardEarned, Action onAdClosed, Action<string> onAdFailed)
        {
#if GOOGLE_MOBILE_ADS
            if (!IsReady())
            {
                Debug.LogWarning("[RewardedHandler] Ad not ready");
                onAdFailed?.Invoke("Ad not ready");
                return;
            }

            onRewardEarnedCallback = onRewardEarned;
            onAdClosedCallback = onAdClosed;
            onAdFailedCallback = onAdFailed;

            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"[RewardedHandler] User earned reward: {reward.Amount} {reward.Type}");
                onRewardEarnedCallback?.Invoke();
            });
#else
            onAdFailed?.Invoke("SDK not installed");
#endif
        }

        private void ClearCallbacks()
        {
            onRewardEarnedCallback = null;
            onAdClosedCallback = null;
            onAdFailedCallback = null;
        }

        public void Destroy()
        {
#if GOOGLE_MOBILE_ADS
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }
#endif
            ClearCallbacks();
        }
    }
}
