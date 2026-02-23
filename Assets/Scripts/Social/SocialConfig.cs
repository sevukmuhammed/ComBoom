using UnityEngine;

namespace ComBoom.Social
{
    [CreateAssetMenu(fileName = "SocialConfig", menuName = "ComBoom/Social Config")]
    public class SocialConfig : ScriptableObject
    {
        [Header("iOS - Game Center")]
        [Tooltip("App Store Connect'te oluşturduğunuz Leaderboard ID")]
        public string iOSLeaderboardId = "Ranks";

        [Header("Android - Play Games")]
        [Tooltip("Play Console'dan alınan Leaderboard ID")]
        public string androidLeaderboardId = "CgkItqyk5LYXEAIQAA";

        public string LeaderboardId
        {
            get
            {
#if UNITY_IOS
                return iOSLeaderboardId;
#elif UNITY_ANDROID
                return androidLeaderboardId;
#else
                return iOSLeaderboardId;
#endif
            }
        }
    }
}
