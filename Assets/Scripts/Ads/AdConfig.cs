using UnityEngine;

namespace ComBoom.Ads
{
    [CreateAssetMenu(fileName = "AdConfig", menuName = "ComBoom/Ad Config")]
    public class AdConfig : ScriptableObject
    {
        [Header("AdMob App IDs")]
        [Tooltip("iOS App ID (ca-app-pub-xxx~xxx)")]
        public string iOSAppId = "";

        [Tooltip("Android App ID (ca-app-pub-xxx~xxx)")]
        public string androidAppId = "";

        [Header("Banner Ad Units")]
        public string iOSBannerId = "ca-app-pub-3940256099942544/2934735716";
        public string androidBannerId = "ca-app-pub-3940256099942544/6300978111";

        [Header("Interstitial Ad Units")]
        public string iOSInterstitialId = "ca-app-pub-3940256099942544/4411468910";
        public string androidInterstitialId = "ca-app-pub-3940256099942544/1033173712";

        [Header("Rewarded Ad Units")]
        public string iOSRewardedId = "ca-app-pub-3940256099942544/1712485313";
        public string androidRewardedId = "ca-app-pub-3940256099942544/5224354917";

        [Header("Interstitial Settings")]
        [Tooltip("Minimum games before first interstitial")]
        public int gamesBeforeFirstInterstitial = 2;

        [Tooltip("Show interstitial every N games")]
        public int interstitialFrequency = 3;

        [Tooltip("Cooldown between interstitials (seconds)")]
        public float interstitialCooldown = 60f;

        [Tooltip("Maximum interstitials per session")]
        public int maxInterstitialsPerSession = 6;

        [Header("Continue Settings")]
        [Tooltip("Number of rows to clear when continuing")]
        public int rowsToClearOnContinue = 2;

        [Tooltip("Countdown seconds before auto-dismiss")]
        public float continueCountdownSeconds = 5f;

        public string BannerId
        {
            get
            {
#if UNITY_IOS
                return iOSBannerId;
#elif UNITY_ANDROID
                return androidBannerId;
#else
                return "unused";
#endif
            }
        }

        public string InterstitialId
        {
            get
            {
#if UNITY_IOS
                return iOSInterstitialId;
#elif UNITY_ANDROID
                return androidInterstitialId;
#else
                return "unused";
#endif
            }
        }

        public string RewardedId
        {
            get
            {
#if UNITY_IOS
                return iOSRewardedId;
#elif UNITY_ANDROID
                return androidRewardedId;
#else
                return "unused";
#endif
            }
        }
    }
}
