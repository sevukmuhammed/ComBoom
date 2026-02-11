using UnityEngine;
using UnityEngine.SocialPlatforms;
using System;
using System.Collections.Generic;
#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#endif
#if UNITY_ANDROID && GOOGLE_PLAY_GAMES
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace ComBoom.Social
{
    public class SocialManager : MonoBehaviour
    {
        public static SocialManager Instance { get; private set; }

        [SerializeField] private SocialConfig config;

        public bool IsAuthenticated { get; private set; }
        public string PlayerId { get; private set; }
        public string PlayerName { get; private set; }

        public event Action<bool> OnAuthenticationChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePlatform();
        }

        private void Start()
        {
            if (config != null && config.autoAuthenticate)
            {
                Authenticate();
            }
        }

        private void InitializePlatform()
        {
#if UNITY_ANDROID && GOOGLE_PLAY_GAMES
            // Google Play Games'i yapılandır
            PlayGamesClientConfiguration playGamesConfig = new PlayGamesClientConfiguration.Builder()
                .RequestServerAuthCode(false)
                .Build();
            PlayGamesPlatform.InitializeInstance(playGamesConfig);
            PlayGamesPlatform.DebugLogEnabled = Debug.isDebugBuild;
            PlayGamesPlatform.Activate();
            Debug.Log("[SocialManager] Google Play Games initialized");
#elif UNITY_IOS
            Debug.Log("[SocialManager] Game Center ready (iOS)");
#else
            Debug.Log("[SocialManager] Social platform not available (Editor/other)");
#endif
        }

        /// <summary>
        /// Platform'a giriş yapar (Game Center / Play Games)
        /// </summary>
        public void Authenticate(Action<bool> onComplete = null)
        {
            if (IsAuthenticated)
            {
                Debug.Log("[SocialManager] Already authenticated");
                onComplete?.Invoke(true);
                return;
            }

#if UNITY_IOS || UNITY_ANDROID
            Debug.Log("[SocialManager] Authenticating...");
            Social.localUser.Authenticate(success =>
            {
                IsAuthenticated = success;
                if (success)
                {
                    PlayerId = Social.localUser.id;
                    PlayerName = Social.localUser.userName;
                    Debug.Log($"[SocialManager] Authenticated: {PlayerName} ({PlayerId})");
                }
                else
                {
                    Debug.LogWarning("[SocialManager] Authentication failed");
                }

                OnAuthenticationChanged?.Invoke(success);
                onComplete?.Invoke(success);
            });
#else
            // Editor'da mock authentication
            Debug.Log("[SocialManager] Mock authentication (Editor)");
            IsAuthenticated = false;
            onComplete?.Invoke(false);
#endif
        }

        /// <summary>
        /// Skor gönderir
        /// </summary>
        public void SubmitScore(long score, Action<bool> onComplete = null)
        {
            if (!IsAuthenticated)
            {
                Debug.LogWarning("[SocialManager] Cannot submit score - not authenticated");
                onComplete?.Invoke(false);
                return;
            }

            if (config == null || string.IsNullOrEmpty(config.LeaderboardId))
            {
                Debug.LogWarning("[SocialManager] Cannot submit score - leaderboard not configured");
                onComplete?.Invoke(false);
                return;
            }

#if UNITY_IOS || UNITY_ANDROID
            Debug.Log($"[SocialManager] Submitting score: {score}");
            Social.ReportScore(score, config.LeaderboardId, success =>
            {
                if (success)
                    Debug.Log("[SocialManager] Score submitted successfully");
                else
                    Debug.LogWarning("[SocialManager] Score submission failed");

                onComplete?.Invoke(success);
            });
#else
            Debug.Log($"[SocialManager] Mock score submit: {score}");
            onComplete?.Invoke(false);
#endif
        }

        /// <summary>
        /// Leaderboard verilerini yükler
        /// </summary>
        public void LoadLeaderboard(Action<LeaderboardEntry[]> onComplete)
        {
            if (!IsAuthenticated)
            {
                Debug.LogWarning("[SocialManager] Cannot load leaderboard - not authenticated");
                onComplete?.Invoke(null);
                return;
            }

            if (config == null || string.IsNullOrEmpty(config.LeaderboardId))
            {
                Debug.LogWarning("[SocialManager] Cannot load leaderboard - not configured");
                onComplete?.Invoke(null);
                return;
            }

#if UNITY_IOS || UNITY_ANDROID
            Debug.Log("[SocialManager] Loading leaderboard...");

            ILeaderboard leaderboard = Social.CreateLeaderboard();
            leaderboard.id = config.LeaderboardId;
            leaderboard.range = new Range(1, config.maxLeaderboardEntries);
            leaderboard.timeScope = TimeScope.AllTime;

            leaderboard.LoadScores(success =>
            {
                if (success && leaderboard.scores != null)
                {
                    Debug.Log($"[SocialManager] Loaded {leaderboard.scores.Length} scores");

                    List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
                    foreach (IScore score in leaderboard.scores)
                    {
                        entries.Add(new LeaderboardEntry(
                            score.rank,
                            score.userID,
                            GetUserName(score.userID),
                            score.value,
                            score.userID == Social.localUser.id
                        ));
                    }

                    // Kullanıcı adlarını yükle (async)
                    LoadUserNames(entries, () =>
                    {
                        onComplete?.Invoke(entries.ToArray());
                    });
                }
                else
                {
                    Debug.LogWarning("[SocialManager] Failed to load leaderboard");
                    onComplete?.Invoke(null);
                }
            });
#else
            Debug.Log("[SocialManager] Mock leaderboard load");
            onComplete?.Invoke(null);
#endif
        }

        private string GetUserName(string userId)
        {
            if (userId == Social.localUser.id)
                return Social.localUser.userName;
            return "Player"; // Placeholder
        }

        private void LoadUserNames(List<LeaderboardEntry> entries, Action onComplete)
        {
#if UNITY_IOS || UNITY_ANDROID
            // Kullanıcı ID'lerini topla
            List<string> userIds = new List<string>();
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.PlayerId) && entry.PlayerId != Social.localUser.id)
                    userIds.Add(entry.PlayerId);
            }

            if (userIds.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            // Kullanıcı profillerini yükle
            Social.LoadUsers(userIds.ToArray(), users =>
            {
                if (users != null)
                {
                    Dictionary<string, string> nameMap = new Dictionary<string, string>();
                    foreach (var user in users)
                    {
                        nameMap[user.id] = user.userName;
                    }

                    foreach (var entry in entries)
                    {
                        if (nameMap.ContainsKey(entry.PlayerId))
                            entry.PlayerName = nameMap[entry.PlayerId];
                    }
                }
                onComplete?.Invoke();
            });
#else
            onComplete?.Invoke();
#endif
        }

        /// <summary>
        /// Platform'un native leaderboard UI'ını gösterir
        /// </summary>
        public void ShowNativeLeaderboard()
        {
            if (!IsAuthenticated)
            {
                Debug.LogWarning("[SocialManager] Cannot show leaderboard - not authenticated");
                return;
            }

#if UNITY_IOS || UNITY_ANDROID
            Social.ShowLeaderboardUI();
#endif
        }

        /// <summary>
        /// Local player'ın sıralamasını yükler
        /// </summary>
        public void LoadLocalPlayerRank(Action<int, long> onComplete)
        {
            if (!IsAuthenticated)
            {
                onComplete?.Invoke(-1, 0);
                return;
            }

#if UNITY_IOS || UNITY_ANDROID
            ILeaderboard leaderboard = Social.CreateLeaderboard();
            leaderboard.id = config.LeaderboardId;
            leaderboard.SetUserFilter(new string[] { Social.localUser.id });

            leaderboard.LoadScores(success =>
            {
                if (success && leaderboard.localUserScore != null)
                {
                    onComplete?.Invoke(leaderboard.localUserScore.rank, leaderboard.localUserScore.value);
                }
                else
                {
                    onComplete?.Invoke(-1, 0);
                }
            });
#else
            onComplete?.Invoke(-1, 0);
#endif
        }
    }
}
