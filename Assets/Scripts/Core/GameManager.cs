using UnityEngine;
using ComBoom.Gameplay;
using ComBoom.Input;
using ComBoom.UI;
using ComBoom.Ads;
using ComBoom.Social;
using System.Collections.Generic;

namespace ComBoom.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private PieceSpawner pieceSpawner;
        [SerializeField] private DragDropHandler dragDropHandler;
        [SerializeField] private AudioManager audioManager;

        [Header("Layout")]
        [SerializeField] private LayoutManager layoutManager;

        [Header("UI")]
        [SerializeField] private UIManager uiManager;

        [Header("Power-Ups")]
        [SerializeField] private PowerUpManager powerUpManager;

        [Header("Pause")]
        [SerializeField] private PausePanel pausePanel;

        [Header("Level")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private LevelProgressBar levelProgressBar;

        [Header("Effects")]
        [SerializeField] private ComboDisplay comboDisplay;

        [Header("Splash")]
        [SerializeField] private SplashPanel splashPanel;

        [Header("Continue")]
        [SerializeField] private ContinuePanel continuePanel;

        private GameState currentState;
        public GameState CurrentState => currentState;

        private int comboCount;
        private bool linesClearedThisTurn;
        private bool xpAddedThisGame;
        private bool canContinue;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            Debug.Log("[ComBoom] GameManager.Start() called");

            if (gridManager == null) { Debug.LogError("[ComBoom] gridManager is NULL!"); return; }
            if (scoreManager == null) { Debug.LogError("[ComBoom] scoreManager is NULL!"); return; }
            if (pieceSpawner == null) { Debug.LogError("[ComBoom] pieceSpawner is NULL!"); return; }
            if (dragDropHandler == null) { Debug.LogError("[ComBoom] dragDropHandler is NULL!"); return; }

            // Apply saved preferences
            if (audioManager != null)
                audioManager.SetMute(PlayerPrefs.GetInt("ComBoom_SoundEnabled", 1) != 1);
            HapticManager.SetEnabled(PlayerPrefs.GetInt("ComBoom_VibeEnabled", 1) == 1);

            BindEvents();

            if (levelManager != null) levelManager.Initialize();
            ShowSplash();
        }

        public void ShowSplash()
        {
            currentState = GameState.Splash;

            // Grid ve spawner'i gizle - splash sirasinda gorunmesin
            if (gridManager != null) gridManager.gameObject.SetActive(false);
            if (pieceSpawner != null) pieceSpawner.gameObject.SetActive(false);

            if (splashPanel != null)
                splashPanel.Show();
            else
                ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            currentState = GameState.Menu;
            Time.timeScale = 1f;

            if (gridManager != null) gridManager.gameObject.SetActive(false);
            if (pieceSpawner != null) pieceSpawner.gameObject.SetActive(false);

            int level = levelManager != null ? levelManager.CurrentLevel : 1;
            int best = scoreManager != null ? scoreManager.HighScore : 0;
            if (uiManager != null) uiManager.ShowMainMenu(level, best);

            if (audioManager != null) audioManager.StartMusic();

            if (AdManager.Instance != null) AdManager.Instance.ShowBanner();
        }

        private void BindEvents()
        {
            dragDropHandler.OnBeforePiecePlaced += HandleBeforePiecePlaced;
            dragDropHandler.OnPiecePlaced += HandlePiecePlaced;
            pieceSpawner.OnAllPiecesUsed += HandleAllPiecesUsed;
            gridManager.OnLinesCleared += HandleLinesCleared;
            if (levelManager != null) levelManager.OnLevelChanged += HandleLevelUp;
        }

        private void OnDestroy()
        {
            if (dragDropHandler != null) dragDropHandler.OnBeforePiecePlaced -= HandleBeforePiecePlaced;
            if (dragDropHandler != null) dragDropHandler.OnPiecePlaced -= HandlePiecePlaced;
            if (pieceSpawner != null) pieceSpawner.OnAllPiecesUsed -= HandleAllPiecesUsed;
            if (gridManager != null) gridManager.OnLinesCleared -= HandleLinesCleared;
            if (levelManager != null) levelManager.OnLevelChanged -= HandleLevelUp;
        }

        private void HandleLevelUp(int newLevel)
        {
            if (powerUpManager != null) powerUpManager.GrantLevelUpRewards();
            LevelUpEffect.Spawn(newLevel);
        }

        public void StartGame()
        {
            currentState = GameState.Playing;
            comboCount = 0;
            linesClearedThisTurn = false;
            xpAddedThisGame = false;
            canContinue = true;

            if (audioManager != null) audioManager.StopMusic();

            if (gridManager != null) gridManager.gameObject.SetActive(true);
            if (pieceSpawner != null) pieceSpawner.gameObject.SetActive(true);

            if (layoutManager != null)
                layoutManager.AdjustLayout();

            gridManager.InitializeGrid();
            scoreManager.Initialize();
            pieceSpawner.SpawnNewSet();

            if (uiManager != null) uiManager.ShowGameUI();

            if (levelProgressBar != null && levelManager != null && scoreManager != null)
                levelProgressBar.Init(levelManager, scoreManager);

            if (AdManager.Instance != null)
            {
                AdManager.Instance.OnGameStarted();
                AdManager.Instance.ShowBanner();
            }
        }

        public void GoToMenu()
        {
            if (pausePanel != null) pausePanel.Hide();
            Time.timeScale = 1f;

            if (!xpAddedThisGame && levelManager != null && scoreManager != null)
            {
                levelManager.AddGameScore(scoreManager.CurrentScore);
                xpAddedThisGame = true;
            }

            ShowMainMenu();
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.Show();
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.Hide();
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
                PauseGame();
            else if (currentState == GameState.Paused)
                ResumeGame();
        }

        public void RestartGame()
        {
            if (pausePanel != null) pausePanel.Hide();
            Time.timeScale = 1f;
            StartGame();
        }

        private void HandleBeforePiecePlaced()
        {
            if (currentState != GameState.Playing) return;

            // Snapshot kaydet (undo icin) - grid degismeden ONCE
            if (powerUpManager != null)
                powerUpManager.SaveSnapshot(comboCount, linesClearedThisTurn);
        }

        private void HandlePiecePlaced(Piece piece, int row, int col)
        {
            if (currentState != GameState.Playing) return;

            // Parcayi havuzdan kaldir
            pieceSpawner.OnPieceUsed(piece);

            // Ses + Haptic
            if (audioManager != null) audioManager.PlayPlace();
            HapticManager.ImpactLight();

            // Combo kontrolu: bu turda satir temizlenmediyse combo sifirla
            if (!linesClearedThisTurn)
            {
                comboCount = 0;
            }
            linesClearedThisTurn = false;

            // Game over kontrolu (3 parca da kullanildiysa yeni set sonrasi kontrol edilir)
            CheckGameOverAfterPlace();
        }

        private void HandleAllPiecesUsed()
        {
            pieceSpawner.SpawnNewSet();
            CheckGameOverAfterPlace();
        }

        private void HandleLinesCleared(int lineCount, Vector3 clearCenter, HashSet<Vector2Int> clearedCells, Dictionary<Vector2Int, Color> clearedCellColors)
        {
            linesClearedThisTurn = true;
            comboCount++;

            // Combo carpani: 1, 2, 4, 8, 16...
            int multiplier = 1 << (comboCount - 1);

            // Grid tamamen temizlendiyse 2x bonus
            bool gridCleared = gridManager.GridData.IsEmpty();
            if (gridCleared) multiplier *= 2;

            // Puan hesapla ve ekle
            int earnedScore = scoreManager.AddLineClearScore(lineCount, multiplier);

            // Floating score popup (combo bilgisiyle)
            FloatingScore.Spawn(clearCenter + Vector3.up * 0.5f, earnedScore, lineCount, comboCount);

            // Tugla kirilma efekti - her temizlenen hucre icin (hucre basi renk)
            foreach (var pos in clearedCells)
            {
                Vector3 cellWorldPos = gridManager.GetWorldPosition(pos.x, pos.y);
                Color cellColor = clearedCellColors.ContainsKey(pos) ? clearedCellColors[pos] : Color.white;
                BrickBreakEffect.Spawn(cellWorldPos, cellColor, comboCount);
            }

            // Combo display (ekranda buyuk COMBO x2! gostergesi)
            if (comboDisplay != null && comboCount >= 2)
                comboDisplay.ShowCombo(comboCount);

            // Ses
            if (audioManager != null)
            {
                if (lineCount >= 2 || comboCount >= 2)
                    audioManager.PlayCombo();
                else
                    audioManager.PlayClear();
            }

            // Haptic - combo veya coklu satir icin guclu titresim
            if (lineCount >= 2 || comboCount >= 2)
                HapticManager.ImpactHeavy();
            else
                HapticManager.NotificationSuccess();
        }

        private void CheckGameOverAfterPlace()
        {
            if (!pieceSpawner.HasAnyPlaceablePiece(gridManager.GridData))
            {
                EndGame();
            }
        }

        public void RestoreComboState(int combo, bool linesCleared)
        {
            comboCount = combo;
            linesClearedThisTurn = linesCleared;
        }

        private void EndGame()
        {
            bool adReady = AdManager.Instance != null && AdManager.Instance.IsRewardedReady();

            if (canContinue && adReady)
            {
                currentState = GameState.WaitingForContinue;

                if (AdManager.Instance != null) AdManager.Instance.HideBanner();

                if (continuePanel != null)
                {
                    continuePanel.Show(
                        scoreManager.CurrentScore,
                        onContinue: () => Continue(),
                        onSkipCallback: () => ShowRealGameOver()
                    );
                }
                else
                {
                    ShowRealGameOver();
                }
            }
            else
            {
                ShowRealGameOver();
            }
        }

        public void Continue()
        {
            canContinue = false;

            int rowsToClear = AdManager.Instance != null && AdManager.Instance.Config != null
                ? AdManager.Instance.Config.rowsToClearOnContinue
                : 2;

            gridManager.ClearRandomRows(rowsToClear);
            pieceSpawner.SpawnNewSet();

            currentState = GameState.Playing;

            if (uiManager != null) uiManager.ShowGameUI();
            if (AdManager.Instance != null) AdManager.Instance.ShowBanner();

            HapticManager.NotificationSuccess();
        }

        private void ShowRealGameOver()
        {
            currentState = GameState.GameOver;

            if (!xpAddedThisGame && levelManager != null && scoreManager != null)
            {
                levelManager.AddGameScore(scoreManager.CurrentScore);
                xpAddedThisGame = true;
            }

            // Skoru leaderboard'a gönder
            SubmitScoreToLeaderboard();

            if (audioManager != null) audioManager.PlayGameOver();
            HapticManager.NotificationError();

            if (AdManager.Instance != null) AdManager.Instance.HideBanner();

            if (AdManager.Instance != null)
            {
                AdManager.Instance.TryShowInterstitial(() =>
                {
                    if (uiManager != null) uiManager.ShowGameOver(scoreManager.CurrentScore, scoreManager.HighScore);
                });
            }
            else
            {
                if (uiManager != null) uiManager.ShowGameOver(scoreManager.CurrentScore, scoreManager.HighScore);
            }
        }

        private void SubmitScoreToLeaderboard()
        {
            if (SocialManager.Instance == null || !SocialManager.Instance.IsAuthenticated)
                return;

            if (scoreManager == null)
                return;

            // Sadece high score'u gönder (en iyi skor)
            int highScore = scoreManager.HighScore;
            if (highScore > 0)
            {
                SocialManager.Instance.SubmitScore(highScore);
            }
        }
    }
}
