using UnityEngine;
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

namespace ComBoom.Ads
{
    public class BannerHandler
    {
        private readonly AdConfig config;
        private bool isShowing;

#if GOOGLE_MOBILE_ADS
        private BannerView bannerView;
#endif

        public BannerHandler(AdConfig config)
        {
            this.config = config;
        }

        public void ShowBanner()
        {
#if GOOGLE_MOBILE_ADS
            if (isShowing) return;

            if (bannerView != null)
            {
                bannerView.Show();
                isShowing = true;
                return;
            }

            CreateBanner();
#endif
        }

#if GOOGLE_MOBILE_ADS
        private void CreateBanner()
        {
            AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            bannerView = new BannerView(config.BannerId, adaptiveSize, AdPosition.Bottom);

            bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("[BannerHandler] Banner loaded");
                isShowing = true;
            };

            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError($"[BannerHandler] Banner failed to load: {error.GetMessage()}");
            };

            var adRequest = new AdRequest();
            bannerView.LoadAd(adRequest);
        }
#endif

        public void HideBanner()
        {
#if GOOGLE_MOBILE_ADS
            if (!isShowing || bannerView == null) return;

            bannerView.Hide();
            isShowing = false;
#endif
        }

        public void Destroy()
        {
#if GOOGLE_MOBILE_ADS
            if (bannerView != null)
            {
                bannerView.Destroy();
                bannerView = null;
            }
#endif
            isShowing = false;
        }
    }
}
