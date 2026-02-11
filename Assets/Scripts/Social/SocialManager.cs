using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using ComBoom.Core;

namespace ComBoom.Social
{
    public class SocialManager : MonoBehaviour
    {
        public static SocialManager Instance { get; private set; }

        [SerializeField] private SocialConfig config;

        public bool IsAuthenticated { get; private set; }
        public string PlayerName { get; private set; } = "Player";

        private bool isAuthenticating;

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
                return;
            }

            isAuthenticating = true;

#if UNITY_IOS || UNITY_ANDROID
            Debug.Log("[SocialManager] Authentication başlatılıyor...");

            UnityEngine.Social.localUser.Authenticate(success =>
            {
                isAuthenticating = false;
                IsAuthenticated = success;

                if (success)
                {
                    // Game Center'dan isim al
                    string gcName = UnityEngine.Social.localUser.userName;

                    // Eğer isim bir ID gibi görünüyorsa (T:, G:, player: ile başlıyorsa) fallback kullan
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

                    Debug.Log($"[SocialManager] Authentication başarılı: {PlayerName} (raw: {gcName})");

                    // Cloud'dan skoru çek ve local'e senkronize et
                    SyncScoreFromCloud();
                }
                else
                {
                    Debug.Log("[SocialManager] Authentication başarısız");
                }

                onComplete?.Invoke(success);
            });
#else
            Debug.Log("[SocialManager] Social platform desteklenmiyor (Editor)");
            isAuthenticating = false;
            onComplete?.Invoke(false);
#endif
        }

        /// <summary>
        /// Cloud'dan kullanıcının skorunu çekip local'e senkronize eder
        /// </summary>
        private void SyncScoreFromCloud()
        {
#if UNITY_IOS || UNITY_ANDROID
            if (config == null || string.IsNullOrEmpty(config.LeaderboardId))
                return;

            Debug.Log("[SocialManager] Cloud'dan skor senkronizasyonu başlatılıyor...");

            ILeaderboard leaderboard = UnityEngine.Social.CreateLeaderboard();
            leaderboard.id = config.LeaderboardId;
            leaderboard.SetUserFilter(new string[] { UnityEngine.Social.localUser.id });

            leaderboard.LoadScores(success =>
            {
                if (success && leaderboard.localUserScore != null)
                {
                    long cloudScore = leaderboard.localUserScore.value;
                    Debug.Log($"[SocialManager] Cloud'dan skor alındı: {cloudScore}");

                    // ScoreManager'ı bul ve senkronize et
                    ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
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

#if UNITY_IOS || UNITY_ANDROID
            Debug.Log($"[SocialManager] Skor gönderiliyor: {score}");

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

#if UNITY_IOS || UNITY_ANDROID
            Debug.Log("[SocialManager] Leaderboard yükleniyor...");

            ILeaderboard leaderboard = UnityEngine.Social.CreateLeaderboard();
            leaderboard.id = config.LeaderboardId;
            // Use fully qualified name to avoid conflict with System.Range
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

                        // Local player için kayıtlı ismi kullan
                        string displayName = isLocal ? PlayerName : $"Player #{score.rank}";

                        entries[i] = new LeaderboardEntry
                        {
                            Rank = score.rank,
                            PlayerName = displayName,
                            Score = score.value,
                            IsLocalPlayer = isLocal
                        };
                    }

                    // Kullanıcı isimlerini yükle
                    LoadUserNames(entries, leaderboard.scores, onComplete);
                }
                else
                {
                    Debug.LogWarning("[SocialManager] Leaderboard yüklenemedi");
                    onComplete?.Invoke(null);
                }
            });
#else
            Debug.Log("[SocialManager] Leaderboard yükleme desteklenmiyor (Editor)");
            onComplete?.Invoke(null);
#endif
        }

#if UNITY_IOS || UNITY_ANDROID
        private void LoadUserNames(LeaderboardEntry[] entries, IScore[] scores, Action<LeaderboardEntry[]> onComplete)
        {
            if (scores == null || scores.Length == 0)
            {
                onComplete?.Invoke(entries);
                return;
            }

            // Önce local player'ın ismini ayarla (bunu zaten biliyoruz)
            string localUserId = UnityEngine.Social.localUser.id;
            string localUserName = PlayerName;

            // Eğer PlayerName boşsa localUser.userName'i dene
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
                        // Local player'ı atla - zaten ayarladık
                        if (entries[i].IsLocalPlayer) continue;

                        if (users[i] != null && !string.IsNullOrEmpty(users[i].userName))
                        {
                            entries[i].PlayerName = users[i].userName;
                        }
                        else
                        {
                            // Fallback: Anlamlı bir isim göster
                            entries[i].PlayerName = $"Player #{entries[i].Rank}";
                        }
                    }
                }
                else
                {
                    // LoadUsers başarısız olduysa fallback isimler
                    for (int i = 0; i < entries.Length; i++)
                    {
                        if (!entries[i].IsLocalPlayer && (string.IsNullOrEmpty(entries[i].PlayerName) || entries[i].PlayerName.StartsWith("T:") || entries[i].PlayerName.StartsWith("G:")))
                        {
                            entries[i].PlayerName = $"Player #{entries[i].Rank}";
                        }
                    }
                }

                onComplete?.Invoke(entries);
            });
        }
#endif

        /// <summary>
        /// Platform'un native leaderboard UI'ını gösterir
        /// </summary>
        public void ShowNativeLeaderboard()
        {
            if (!IsAuthenticated)
            {
                Debug.Log("[SocialManager] Native UI gösterilemiyor - authenticate değil");
                return;
            }

#if UNITY_IOS || UNITY_ANDROID
            UnityEngine.Social.ShowLeaderboardUI();
#endif
        }
    }
}
