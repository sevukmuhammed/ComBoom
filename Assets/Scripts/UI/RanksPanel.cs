using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ComBoom.Core;
using ComBoom.Gameplay;

namespace ComBoom.UI
{
    public class RanksPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform contentParent;
        [SerializeField] private TextMeshProUGUI playerRankText;
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private AudioManager audioManager;

        private bool populated;

        private static readonly string[] DummyNames = {
            "BlockMaster", "PuzzleKing", "FitQueen", "GridWizard", "ComboLord",
            "NeonNinja", "PixelPro", "CubeCrush", "TetrisGod", "StackHero",
            "LineBreaker", "ScoreBoss", "ShapeShift", "BrickLayer", "ClearKing",
            "JewelJam", "BlockBust", "GridGuru", "PiecePro", "DropMaster",
            "SlotKing", "FillPro", "RowClear", "ColSmash", "ComboX",
            "MegaBlock", "SuperFit", "UltraGrid", "HyperDrop", "TurboLine",
            "SwiftBlock", "QuickFit", "FlashGrid", "RapidDrop", "SpeedLine",
            "AlphaBlock", "BetaFit", "GammaGrid", "DeltaDrop", "OmegaLine",
            "ProBlock", "EliteFit", "MasterGrid", "ChampDrop", "AceLine",
            "TopBlock", "PeakFit", "MaxGrid", "PrimeDrop", "BestLine"
        };

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
            if (!populated) PopulateList();
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

        private void PopulateList()
        {
            if (contentParent == null) return;
            populated = true;

            int entryCount = 50;
            int topScore = 900000;
            int botScore = 10000;

            // Kullanıcının gerçek skorunu al
            int playerScore = 0;
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                playerScore = scoreManager.HighScore;
            }

            // Mock listeyi oluştur ve kullanıcı sıralamasını hesapla
            int playerRank = entryCount + 1; // Varsayılan: listenin sonunda
            int[] mockScores = new int[entryCount];

            for (int i = 0; i < entryCount; i++)
            {
                float t = (float)i / (entryCount - 1);
                int score = Mathf.RoundToInt(Mathf.Lerp(topScore, botScore, t * t));
                mockScores[i] = (score / 100) * 100;

                // Kullanıcı sıralamasını hesapla
                if (playerScore >= mockScores[i] && playerRank > i + 1)
                {
                    playerRank = i + 1;
                }
            }

            // Listeyi oluştur
            for (int i = 0; i < entryCount; i++)
            {
                int rank = i + 1;
                string playerName = DummyNames[i % DummyNames.Length];
                CreateRankRow(rank, playerName, mockScores[i]);
            }

            // Kullanıcı skor barı - gerçek değerler
            if (playerRankText != null) playerRankText.text = playerScore > 0 ? $"#{playerRank}" : "#--";
            if (playerScoreText != null) playerScoreText.text = playerScore.ToString("N0");
        }

        private void CreateRankRow(int rank, string playerName, int score)
        {
            bool isTop3 = rank <= 3;

            // Row container
            GameObject row = new GameObject($"RankRow_{rank}");
            row.transform.SetParent(contentParent, false);
            RectTransform rowRT = row.AddComponent<RectTransform>();
            rowRT.sizeDelta = new Vector2(0, 96);

            Image rowBg = row.AddComponent<Image>();
            rowBg.sprite = SpriteGenerator.CreateRoundedUISprite();
            rowBg.type = Image.Type.Sliced;
            rowBg.raycastTarget = false;

            if (isTop3)
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

            // Rank column (sabit 40px container - trophy veya sayi, hepsi ayni hizada)
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
                    1 => new Color(0.961f, 0.620f, 0.043f, 1f), // gold
                    2 => new Color(0.631f, 0.667f, 0.718f, 1f), // silver
                    _ => new Color(0.804f, 0.498f, 0.196f, 1f), // bronze
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
                rankTMP.color = new Color(0.392f, 0.455f, 0.545f, 1f); // slate-500
                rankTMP.fontStyle = FontStyles.Bold;
            }

            // Name column (flexible width)
            GameObject nameObj = new GameObject("PlayerName");
            nameObj.transform.SetParent(row.transform, false);
            TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
            nameTMP.font = GetDefaultTMPFont();
            nameTMP.text = playerName;
            nameTMP.fontSize = 32;
            nameTMP.alignment = TextAlignmentOptions.Left;
            nameTMP.color = Color.white;
            nameTMP.fontStyle = FontStyles.Normal;
            nameTMP.enableAutoSizing = false;

            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.flexibleWidth = 1;
            nameLE.minWidth = 80;

            // Score column (sabit sag taraf)
            GameObject scoreObj = new GameObject("Score");
            scoreObj.transform.SetParent(row.transform, false);
            TextMeshProUGUI scoreTMP = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreTMP.font = GetDefaultTMPFont();
            scoreTMP.text = score.ToString("N0");
            scoreTMP.fontSize = 30;
            scoreTMP.alignment = TextAlignmentOptions.Right;
            scoreTMP.fontStyle = FontStyles.Bold;
            scoreTMP.enableAutoSizing = false;

            if (isTop3)
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
                scoreTMP.color = new Color(0.580f, 0.639f, 0.722f, 1f); // slate-400
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
