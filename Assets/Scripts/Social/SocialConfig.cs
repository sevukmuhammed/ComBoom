using UnityEngine;

namespace ComBoom.Social
{
    [CreateAssetMenu(fileName = "SocialConfig", menuName = "ComBoom/Social Config")]
    public class SocialConfig : ScriptableObject
    {
        [Header("iOS - Game Center")]
        [Tooltip("App Store Connect > App > Game Center > Leaderboard ID")]
        public string iOSLeaderboardId = "com.comboom.highscore";

        [Header("Android - Play Games")]
        [Tooltip("Play Console > Play Games Services > Leaderboard ID")]
        public string androidLeaderboardId = "";

        [Tooltip("Play Console > Play Games Services > App ID")]
        public string androidAppId = "";

        [Header("Settings")]
        [Tooltip("Leaderboard'da gösterilecek maksimum kayıt sayısı")]
        public int maxLeaderboardEntries = 25;

        [Tooltip("Authentication otomatik başlasın mı?")]
        public bool autoAuthenticate = true;

        public string LeaderboardId
        {
            get
            {
#if UNITY_IOS
                return iOSLeaderboardId;
#elif UNITY_ANDROID
                return androidLeaderboardId;
#else
                return iOSLeaderboardId; // Editor'da iOS ID kullan
#endif
            }
        }

        public bool IsConfigured
        {
            get
            {
#if UNITY_IOS
                return !string.IsNullOrEmpty(iOSLeaderboardId);
#elif UNITY_ANDROID
                return !string.IsNullOrEmpty(androidLeaderboardId) && !string.IsNullOrEmpty(androidAppId);
#else
                return true; // Editor'da her zaman true
#endif
            }
        }
    }
}
