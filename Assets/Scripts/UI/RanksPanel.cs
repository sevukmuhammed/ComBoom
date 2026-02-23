using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ComBoom.Core;
using ComBoom.Gameplay;
using ComBoom.Social;

namespace ComBoom.UI
{
    public class RanksPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform contentParent;
        [SerializeField] private TextMeshProUGUI playerRankText;
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private GameObject loadingIndicator;

        private bool isLoading;


        public void Show()
        {
            if (panel != null) panel.SetActive(true);
            RefreshLeaderboard();
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void OnBackButton()
        {
            if (audioManager != null) audioManager.PlayClick();
            Hide();
            GameManager.Instance.ShowMainMenu();
        }

        public void RefreshLeaderboard()
        {
            ClearList();
            UpdatePlayerScoreBar();

            if (SocialManager.Instance != null && SocialManager.Instance.IsAuthenticated)
            {
                LoadRealLeaderboard();
            }
            else
            {
                ShowPlayGamesPrompt();
            }
        }

        private void LoadRealLeaderboard()
        {
            if (isLoading) return;
            isLoading = true;

            if (loadingIndicator != null)
                loadingIndicator.SetActive(true);

            SocialManager.Instance.LoadLeaderboard(entries =>
            {
                isLoading = false;

                if (loadingIndicator != null)
                    loadingIndicator.SetActive(false);

                if (entries != null && entries.Length > 0)
                {
                    PopulateRealList(entries);
                }
                else
                {
                    ShowEmptyLeaderboardMessage();
                }
            });
        }

        private void PopulateRealList(LeaderboardEntry[] entries)
        {
            int playerScore = GetPlayerHighScore();
            int playerRank = -1;

            foreach (var entry in entries)
            {
                bool isLocalPlayer = entry.IsLocalPlayer;
                CreateRankRow(entry.Rank, entry.PlayerName, (int)entry.Score, isLocalPlayer);

                if (isLocalPlayer)
                    playerRank = entry.Rank;
            }

            if (playerRankText != null)
            {
                if (playerRank > 0)
                    playerRankText.text = $"#{playerRank}";
                else
                    playerRankText.text = playerScore > 0 ? "#--" : "#--";
            }
        }

        private void ShowEmptyLeaderboardMessage()
        {
            if (playerRankText != null)
                playerRankText.text = "#1";

            GameObject msgObj = new GameObject("EmptyLeaderboard");
            msgObj.transform.SetParent(contentParent, false);
            RectTransform msgRT = msgObj.AddComponent<RectTransform>();
            msgRT.sizeDelta = new Vector2(0, 300);

            VerticalLayoutGroup vlg = msgObj.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 16;
            vlg.padding = new RectOffset(30, 30, 60, 30);

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(msgObj.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = SpriteGenerator.CreateTrophyIconSprite();
            iconImg.preserveAspect = true;
            iconImg.color = new Color(0.961f, 0.620f, 0.043f, 0.7f);
            iconImg.raycastTarget = false;
            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 80;
            iconLE.preferredHeight = 80;

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(msgObj.transform, false);
            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.font = GetDefaultTMPFont();
            titleTMP.text = LocalizationManager.Get("ranks_empty_title");
            titleTMP.fontSize = 42;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = Color.white;
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 55;

            GameObject descObj = new GameObject("Desc");
            descObj.transform.SetParent(msgObj.transform, false);
            TextMeshProUGUI descTMP = descObj.AddComponent<TextMeshProUGUI>();
            descTMP.font = GetDefaultTMPFont();
            descTMP.text = LocalizationManager.Get("ranks_empty_desc");
            descTMP.fontSize = 32;
            descTMP.alignment = TextAlignmentOptions.Center;
            descTMP.color = new Color(0.392f, 0.455f, 0.545f, 1f);
            descTMP.enableWordWrapping = true;
            LayoutElement descLE = descObj.AddComponent<LayoutElement>();
            descLE.preferredHeight = 100;
        }

        private void ShowPlayGamesPrompt()
        {
            if (playerRankText != null)
                playerRankText.text = "#--";

            GameObject msgObj = new GameObject("PlayGamesPrompt");
            msgObj.transform.SetParent(contentParent, false);
            RectTransform msgRT = msgObj.AddComponent<RectTransform>();
            msgRT.sizeDelta = new Vector2(0, 300);

            VerticalLayoutGroup vlg = msgObj.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 16;
            vlg.padding = new RectOffset(30, 30, 60, 30);

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(msgObj.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = SpriteGenerator.CreateTrophyIconSprite();
            iconImg.preserveAspect = true;
            iconImg.color = new Color(0.392f, 0.455f, 0.545f, 0.5f);
            iconImg.raycastTarget = false;
            LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 80;
            iconLE.preferredHeight = 80;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(msgObj.transform, false);
            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.font = GetDefaultTMPFont();
            titleTMP.text = LocalizationManager.Get("ranks_no_play_games_title");
            titleTMP.fontSize = 42;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = Color.white;
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 55;

            // Description
            GameObject descObj = new GameObject("Desc");
            descObj.transform.SetParent(msgObj.transform, false);
            TextMeshProUGUI descTMP = descObj.AddComponent<TextMeshProUGUI>();
            descTMP.font = GetDefaultTMPFont();
            descTMP.text = LocalizationManager.Get("ranks_no_play_games_desc");
            descTMP.fontSize = 32;
            descTMP.alignment = TextAlignmentOptions.Center;
            descTMP.color = new Color(0.392f, 0.455f, 0.545f, 1f);
            descTMP.enableWordWrapping = true;
            LayoutElement descLE = descObj.AddComponent<LayoutElement>();
            descLE.preferredHeight = 100;
        }

        private void UpdatePlayerScoreBar()
        {
            int playerScore = GetPlayerHighScore();
            if (playerScoreText != null)
                playerScoreText.text = playerScore.ToString("N0");
        }

        private int GetPlayerHighScore()
        {
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            return scoreManager != null ? scoreManager.HighScore : 0;
        }

        private void ClearList()
        {
            if (contentParent == null) return;

            for (int i = contentParent.childCount - 1; i >= 0; i--)
            {
                Destroy(contentParent.GetChild(i).gameObject);
            }
        }

        private void CreateRankRow(int rank, string playerName, int score, bool isLocalPlayer = false)
        {
            bool isTop3 = rank <= 3;

            GameObject row = new GameObject($"RankRow_{rank}");
            row.transform.SetParent(contentParent, false);
            RectTransform rowRT = row.AddComponent<RectTransform>();
            rowRT.sizeDelta = new Vector2(0, 96);

            Image rowBg = row.AddComponent<Image>();
            rowBg.sprite = SpriteGenerator.CreateRoundedUISprite();
            rowBg.type = Image.Type.Sliced;
            rowBg.raycastTarget = false;

            if (isLocalPlayer)
                rowBg.color = new Color(0.063f, 0.725f, 0.506f, 0.4f);
            else if (isTop3)
                rowBg.color = new Color(0.059f, 0.090f, 0.165f, 0.60f);
            else
                rowBg.color = new Color(0.059f, 0.090f, 0.165f, 0.35f);

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(18, 18, 10, 10);
            hlg.spacing = 14;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            GameObject rankContainer = new GameObject("RankCol");
            rankContainer.transform.SetParent(row.transform, false);
            RectTransform rankContainerRT = rankContainer.AddComponent<RectTransform>();
            LayoutElement rankContainerLE = rankContainer.AddComponent<LayoutElement>();
            rankContainerLE.minWidth = 54;
            rankContainerLE.preferredWidth = 54;

            if (isTop3)
            {
                GameObject trophyObj = new GameObject("Trophy");
                trophyObj.transform.SetParent(rankContainer.transform, false);
                RectTransform trophyRT = trophyObj.AddComponent<RectTransform>();
                trophyRT.anchorMin = new Vector2(0.5f, 0.5f);
                trophyRT.anchorMax = new Vector2(0.5f, 0.5f);
                trophyRT.pivot = new Vector2(0.5f, 0.5f);
                trophyRT.sizeDelta = new Vector2(42, 42);
                trophyRT.anchoredPosition = Vector2.zero;

                Image trophyImg = trophyObj.AddComponent<Image>();
                trophyImg.sprite = SpriteGenerator.CreateTrophyIconSprite();
                trophyImg.preserveAspect = true;
                trophyImg.raycastTarget = false;

                Color trophyColor = rank switch
                {
                    1 => new Color(0.961f, 0.620f, 0.043f, 1f),
                    2 => new Color(0.631f, 0.667f, 0.718f, 1f),
                    _ => new Color(0.804f, 0.498f, 0.196f, 1f),
                };
                trophyImg.color = trophyColor;
            }
            else
            {
                GameObject rankObj = new GameObject("RankNum");
                rankObj.transform.SetParent(rankContainer.transform, false);
                RectTransform rankObjRT = rankObj.AddComponent<RectTransform>();
                rankObjRT.anchorMin = Vector2.zero;
                rankObjRT.anchorMax = Vector2.one;
                rankObjRT.offsetMin = Vector2.zero;
                rankObjRT.offsetMax = Vector2.zero;

                TextMeshProUGUI rankTMP = rankObj.AddComponent<TextMeshProUGUI>();
                rankTMP.font = GetDefaultTMPFont();
                rankTMP.text = rank.ToString();
                rankTMP.fontSize = 32;
                rankTMP.alignment = TextAlignmentOptions.Center;
                rankTMP.color = isLocalPlayer
                    ? new Color(0.063f, 0.725f, 0.506f, 1f)
                    : new Color(0.392f, 0.455f, 0.545f, 1f);
                rankTMP.fontStyle = FontStyles.Bold;
            }

            GameObject nameObj = new GameObject("PlayerName");
            nameObj.transform.SetParent(row.transform, false);
            TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.font = GetDefaultTMPFont();
            nameTMP.text = isLocalPlayer ? "You" : playerName;
            nameTMP.fontSize = 32;
            nameTMP.alignment = TextAlignmentOptions.Left;
            nameTMP.color = isLocalPlayer ? new Color(0.063f, 0.725f, 0.506f, 1f) : Color.white;
            nameTMP.fontStyle = isLocalPlayer ? FontStyles.Bold : FontStyles.Normal;
            nameTMP.enableAutoSizing = false;

            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.flexibleWidth = 1;
            nameLE.minWidth = 80;

            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(row.transform, false);
            TextMeshProUGUI scoreTMP = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreTMP.font = GetDefaultTMPFont();
            scoreTMP.text = score.ToString("N0");
            scoreTMP.fontSize = 30;
            scoreTMP.alignment = TextAlignmentOptions.Right;
            scoreTMP.fontStyle = FontStyles.Bold;
            scoreTMP.enableAutoSizing = false;

            if (isLocalPlayer)
            {
                scoreTMP.color = new Color(0.063f, 0.725f, 0.506f, 1f);
            }
            else if (isTop3)
            {
                scoreTMP.color = rank switch
                {
                    1 => new Color(0.961f, 0.620f, 0.043f, 1f),
                    2 => new Color(0.631f, 0.667f, 0.718f, 1f),
                    _ => new Color(0.804f, 0.498f, 0.196f, 1f),
                };
            }
            else
            {
                scoreTMP.color = new Color(0.580f, 0.639f, 0.722f, 1f);
            }

            LayoutElement scoreLE = scoreObj.AddComponent<LayoutElement>();
            scoreLE.minWidth = 140;
            scoreLE.preferredWidth = 160;
        }

        private static TMP_FontAsset _cachedFont;
        private static TMP_FontAsset GetDefaultTMPFont()
        {
            if (_cachedFont != null) return _cachedFont;
            _cachedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (_cachedFont == null)
                _cachedFont = TMP_Settings.defaultFontAsset;
            return _cachedFont;
        }
    }
}
