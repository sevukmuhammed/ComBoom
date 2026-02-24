using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using ComBoom.Core;
#if UNITY_ANDROID
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
        public string PlayerName { get; private set; } = "Player";

        private bool isAuthenticating;
        private bool isInitialized;
        private ScoreManager _cachedScoreManager;
        private System.Collections.Generic.List<Action<bool>> pendingAuthCallbacks;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Config yoksa Resources'dan yüklemeyi dene
            if (config == null)
            {
                config = Resources.Load<SocialConfig>("SocialConfig");
            }

            // Config hala yoksa varsayılan oluştur
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SocialConfig>();
                Debug.Log("[SocialManager] Varsayılan config oluşturuldu (Leaderboard ID: Ranks)");
            }

            // Platform'u başlat
            InitializePlatform();
        }

        private void InitializePlatform()
        {
            if (isInitialized) return;

#if UNITY_ANDROID
            // Google Play Games'i aktif et
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
            Debug.Log("[SocialManager] Google Play Games platform aktif edildi");
#endif

            isInitialized = true;
        }

        /// <summary>
        /// Game Center / Play Games authentication başlatır
        /// </summary>
        public void Authenticate(Action<bool> onComplete = null)
        {
            if (IsAuthenticated)
            {
                onComplete?.Invoke(true);
                return;
            }

            if (isAuthenticating)
            {
                // Callback'i kuyruğa ekle, kaybetme
                if (onComplete != null)
                {
                    if (pendingAuthCallbacks == null)
                        pendingAuthCallbacks = new System.Collections.Generic.List<Action<bool>>();
                    pendingAuthCallbacks.Add(onComplete);
                }
                return;
            }

            isAuthenticating = true;
            if (onComplete != null)
            {
                if (pendingAuthCallbacks == null)
                    pendingAuthCallbacks = new System.Collections.Generic.List<Action<bool>>();
                pendingAuthCallbacks.Add(onComplete);
            }

#if UNITY_IOS
            // iOS - Game Center
            Debug.Log("[SocialManager] Game Center authentication başlatılıyor...");

            UnityEngine.Social.localUser.Authenticate(success =>
            {
                isAuthenticating = false;
                IsAuthenticated = success;

                if (success)
                {
                    string gcName = UnityEngine.Social.localUser.userName;

                    if (string.IsNullOrEmpty(gcName) ||
                        gcName.StartsWith("T:") ||
                        gcName.StartsWith("G:") ||
                        gcName.StartsWith("player:") ||
                        gcName.Length > 30)
                    {
                        PlayerName = "You";
                    }
                    else
                    {
                        PlayerName = gcName;
                    }

                    Debug.Log($"[SocialManager] Game Center authentication başarılı: {PlayerName}");
                    SyncScoreFromCloud();
                    SubmitLocalHighScore();
                }
                else
                {
                    Debug.Log("[SocialManager] Game Center authentication başarısız");
                }

                InvokeAuthCallbacks(success);
            });

#elif UNITY_ANDROID
            // Android - Google Play Games
            Debug.Log("[SocialManager] Google Play Games authentication başlatılıyor...");

            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                if (status == SignInStatus.Success)
                {
                    isAuthenticating = false;
                    IsAuthenticated = true;
                    PlayerName = PlayGamesPlatform.Instance.GetUserDisplayName();

                    if (string.IsNullOrEmpty(PlayerName))
                    {
                        PlayerName = "You";
                    }

                    Debug.Log($"[SocialManager] Google Play Games authentication başarılı: {PlayerName}");
                    SyncScoreFromCloud();
                    SubmitLocalHighScore();
                    InvokeAuthCallbacks(true);
                }
                else
                {
                    Debug.Log($"[SocialManager] Sessiz giriş başarısız ({status}), manuel giriş deneniyor...");
                    PlayGamesPlatform.Instance.ManuallyAuthenticate(manualStatus =>
                    {
                        isAuthenticating = false;
                        IsAuthenticated = (manualStatus == SignInStatus.Success);

                        if (IsAuthenticated)
                        {
                            PlayerName = PlayGamesPlatform.Instance.GetUserDisplayName();
                            if (string.IsNullOrEmpty(PlayerName))
                                PlayerName = "You";

                            Debug.Log($"[SocialManager] Manuel giriş başarılı: {PlayerName}");
                            SyncScoreFromCloud();
                            SubmitLocalHighScore();
                        }
                        else
                        {
                            Debug.Log($"[SocialManager] Manuel giriş de başarısız: {manualStatus}");
                        }

                        InvokeAuthCallbacks(IsAuthenticated);
                    });
                }
            });
#else
            Debug.Log("[SocialManager] Social platform desteklenmiyor (Editor)");
            isAuthenticating = false;
            InvokeAuthCallbacks(false);
#endif
        }

        private void InvokeAuthCallbacks(bool success)
        {
            if (pendingAuthCallbacks != null)
            {
                foreach (var cb in pendingAuthCallbacks)
                    cb?.Invoke(success);
                pendingAuthCallbacks.Clear();
            }
        }

        private ScoreManager GetScoreManager()
        {
            if (_cachedScoreManager == null)
                _cachedScoreManager = FindObjectOfType<ScoreManager>();
            return _cachedScoreManager;
        }

        /// <summary>
        /// Cloud'dan kullanıcının skorunu çekip local'e senkronize eder
        /// </summary>
        private void SyncScoreFromCloud()
        {
            if (config == null || string.IsNullOrEmpty(config.LeaderboardId))
                return;

            Debug.Log("[SocialManager] Cloud'dan skor senkronizasyonu başlatılıyor...");

#if UNITY_IOS
            ILeaderboard leaderboard = UnityEngine.Social.CreateLeaderboard();
            leaderboard.id = config.LeaderboardId;
            leaderboard.SetUserFilter(new string[] { UnityEngine.Social.localUser.id });

            leaderboard.LoadScores(success =>
            {
                if (success && leaderboard.localUserScore != null)
                {
                    long cloudScore = leaderboard.localUserScore.value;
                    Debug.Log($"[SocialManager] Cloud'dan skor alındı: {cloudScore}");

                    ScoreManager scoreManager = GetScoreManager();
                    if (scoreManager != null)
                    {
                        scoreManager.SyncFromCloud((int)cloudScore);
                    }
                }
                else
                {
                    Debug.Log("[SocialManager] Cloud'da skor bulunamadı");
                }
            });

#elif UNITY_ANDROID
            PlayGamesPlatform.Instance.LoadScores(
                config.LeaderboardId,
                LeaderboardStart.PlayerCentered,
                1,
                LeaderboardCollection.Public,
                LeaderboardTimeSpan.AllTime,
                (data) =>
                {
                    if (data.Valid && data.PlayerScore != null)
                    {
                        long cloudScore = data.PlayerScore.value;
                        Debug.Log($"[SocialManager] Cloud'dan skor alındı: {cloudScore}");

                        ScoreManager scoreManager = GetScoreManager();
                        if (scoreManager != null)
                        {
                            scoreManager.SyncFromCloud((int)cloudScore);
                        }
                    }
                    else
                    {
                        Debug.Log("[SocialManager] Cloud'da skor bulunamadı");
                    }
                });
#endif
        }

        /// <summary>
        /// Auth sonrası lokal high score'u leaderboard'a gönderir
        /// </summary>
        private void SubmitLocalHighScore()
        {
            ScoreManager scoreManager = GetScoreManager();
            if (scoreManager != null && scoreManager.HighScore > 0)
            {
                Debug.Log($"[SocialManager] Lokal high score gönderiliyor: {scoreManager.HighScore}");
                SubmitScore(scoreManager.HighScore);
            }
        }

        /// <summary>
        /// Skoru leaderboard'a gönderir
        /// </summary>
        public void SubmitScore(int score, Action<bool> onComplete = null)
        {
            if (!IsAuthenticated)
            {
                Debug.Log("[SocialManager] Skor gönderilemedi - authenticate değil");
                onComplete?.Invoke(false);
                return;
            }

            if (config == null || string.IsNullOrEmpty(config.LeaderboardId))
            {
                Debug.LogWarning("[SocialManager] Leaderboard ID ayarlanmamış");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"[SocialManager] Skor gönderiliyor: {score}");

#if UNITY_IOS
            UnityEngine.Social.ReportScore(score, config.LeaderboardId, success =>
            {
                if (success)
                {
                    Debug.Log($"[SocialManager] Skor başarıyla gönderildi: {score}");
                }
                else
                {
                    Debug.LogWarning("[SocialManager] Skor gönderilemedi");
                }
                onComplete?.Invoke(success);
            });

#elif UNITY_ANDROID
            PlayGamesPlatform.Instance.ReportScore(score, config.LeaderboardId, success =>
            {
                if (success)
                {
                    Debug.Log($"[SocialManager] Skor başarıyla gönderildi: {score}");
                }
                else
                {
                    Debug.LogWarning("[SocialManager] Skor gönderilemedi");
                }
                onComplete?.Invoke(success);
            });
#else
            Debug.Log("[SocialManager] Skor gönderimi desteklenmiyor (Editor)");
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
                Debug.Log("[SocialManager] Leaderboard yüklenemedi - authenticate değil");
                onComplete?.Invoke(null);
                return;
            }

            if (config == null || string.IsNullOrEmpty(config.LeaderboardId))
            {
                Debug.LogWarning("[SocialManager] Leaderboard ID ayarlanmamış");
                onComplete?.Invoke(null);
                return;
            }

            Debug.Log("[SocialManager] Leaderboard yükleniyor...");

#if UNITY_IOS
            LoadLeaderboardIOS(onComplete);
#elif UNITY_ANDROID
            LoadLeaderboardAndroid(onComplete);
#else
            Debug.Log("[SocialManager] Leaderboard yükleme desteklenmiyor (Editor)");
            onComplete?.Invoke(null);
#endif
        }

#if UNITY_IOS
        private void LoadLeaderboardIOS(Action<LeaderboardEntry[]> onComplete)
        {
            ILeaderboard leaderboard = UnityEngine.Social.CreateLeaderboard();
            leaderboard.id = config.LeaderboardId;
            leaderboard.range = new UnityEngine.SocialPlatforms.Range(1, 50);

            leaderboard.LoadScores(success =>
            {
                if (success && leaderboard.scores != null)
                {
                    Debug.Log($"[SocialManager] {leaderboard.scores.Length} skor yüklendi");

                    LeaderboardEntry[] entries = new LeaderboardEntry[leaderboard.scores.Length];
                    string localUserId = UnityEngine.Social.localUser.id;

                    for (int i = 0; i < leaderboard.scores.Length; i++)
                    {
                        IScore score = leaderboard.scores[i];
                        bool isLocal = score.userID == localUserId;

                        entries[i] = new LeaderboardEntry
                        {
                            Rank = score.rank,
                            PlayerName = isLocal ? PlayerName : $"Player #{score.rank}",
                            Score = score.value,
                            IsLocalPlayer = isLocal
                        };
                    }

                    LoadUserNamesIOS(entries, leaderboard.scores, onComplete);
                }
                else
                {
                    Debug.LogWarning("[SocialManager] Leaderboard yüklenemedi");
                    onComplete?.Invoke(null);
                }
            });
        }

        private void LoadUserNamesIOS(LeaderboardEntry[] entries, IScore[] scores, Action<LeaderboardEntry[]> onComplete)
        {
            if (scores == null || scores.Length == 0)
            {
                onComplete?.Invoke(entries);
                return;
            }

            string localUserId = UnityEngine.Social.localUser.id;
            string localUserName = PlayerName;

            if (string.IsNullOrEmpty(localUserName) || localUserName == "Player")
            {
                localUserName = UnityEngine.Social.localUser.userName;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].IsLocalPlayer && !string.IsNullOrEmpty(localUserName))
                {
                    entries[i].PlayerName = localUserName;
                }
            }

            string[] userIds = new string[scores.Length];
            for (int i = 0; i < scores.Length; i++)
            {
                userIds[i] = scores[i].userID;
            }

            UnityEngine.Social.LoadUsers(userIds, users =>
            {
                if (users != null)
                {
                    for (int i = 0; i < users.Length && i < entries.Length; i++)
                    {
                        if (entries[i].IsLocalPlayer) continue;

                        if (users[i] != null && !string.IsNullOrEmpty(users[i].userName))
                        {
                            entries[i].PlayerName = users[i].userName;
                        }
                        else
                        {
                            entries[i].PlayerName = $"Player #{entries[i].Rank}";
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < entries.Length; i++)
                    {
                        if (!entries[i].IsLocalPlayer &&
                            (string.IsNullOrEmpty(entries[i].PlayerName) ||
                             entries[i].PlayerName.StartsWith("T:") ||
                             entries[i].PlayerName.StartsWith("G:")))
                        {
                            entries[i].PlayerName = $"Player #{entries[i].Rank}";
                        }
                    }
                }

                onComplete?.Invoke(entries);
            });
        }
#endif

#if UNITY_ANDROID
        private void LoadLeaderboardAndroid(Action<LeaderboardEntry[]> onComplete)
        {
            PlayGamesPlatform.Instance.LoadScores(
                config.LeaderboardId,
                LeaderboardStart.PlayerCentered,
                25,
                LeaderboardCollection.Public,
                LeaderboardTimeSpan.AllTime,
                (data) =>
                {
                    Debug.Log($"[SocialManager] LoadScores Valid={data.Valid}, Scores={data.Scores?.Length ?? -1}, PlayerScore={data.PlayerScore?.value ?? -1}");

                    if (data.Valid)
                    {
                        var entryList = new System.Collections.Generic.List<LeaderboardEntry>();
                        string localUserId = PlayGamesPlatform.Instance.GetUserId();

                        // Scores dizisinden gelen kayıtları ekle
                        if (data.Scores != null)
                        {
                            for (int i = 0; i < data.Scores.Length; i++)
                            {
                                var score = data.Scores[i];
                                bool isLocal = score.userID == localUserId;

                                entryList.Add(new LeaderboardEntry
                                {
                                    Rank = score.rank,
                                    PlayerName = isLocal ? PlayerName : $"Player #{score.rank}",
                                    Score = score.value,
                                    IsLocalPlayer = isLocal
                                });
                            }
                        }

                        // Scores boşsa ama PlayerScore varsa, onu ekle
                        if (entryList.Count == 0 && data.PlayerScore != null && data.PlayerScore.value > 0)
                        {
                            Debug.Log($"[SocialManager] Scores boş, PlayerScore kullanılıyor: {data.PlayerScore.value}");
                            entryList.Add(new LeaderboardEntry
                            {
                                Rank = data.PlayerScore.rank > 0 ? data.PlayerScore.rank : 1,
                                PlayerName = PlayerName,
                                Score = data.PlayerScore.value,
                                IsLocalPlayer = true
                            });
                        }

                        Debug.Log($"[SocialManager] {entryList.Count} skor yüklendi");
                        onComplete?.Invoke(entryList.Count > 0 ? entryList.ToArray() : null);
                    }
                    else
                    {
                        Debug.LogWarning("[SocialManager] Leaderboard yüklenemedi");
                        onComplete?.Invoke(null);
                    }
                });
        }
#endif

    }
}
