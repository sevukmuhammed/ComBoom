using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;
using ComBoom.Gameplay;
using ComBoom.Input;

namespace ComBoom.Core
{
    public class PowerUpManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private PieceSpawner pieceSpawner;
        [SerializeField] private DragDropHandler dragDropHandler;
        [SerializeField] private Camera mainCamera;

        [Header("Buttons")]
        [SerializeField] private Button undoButton;
        [SerializeField] private Button bombButton;
        [SerializeField] private Button shuffleButton;

        [Header("Count Badges")]
        [SerializeField] private TextMeshProUGUI undoCountText;
        [SerializeField] private TextMeshProUGUI bombCountText;
        [SerializeField] private TextMeshProUGUI shuffleCountText;

        // Undo snapshot
        private GridSnapshot? lastGridSnapshot;
        private PieceSnapshot? lastPieceSnapshot;
        private int lastScore;
        private int lastComboCount;
        private bool lastLinesClearedThisTurn;
        private bool canUndo;

        // Bomb mode
        private bool isBombMode;

        // Power-up counts
        private int undoCount;
        private int bombCount;
        private int shuffleCount;

        private const string UNDO_COUNT_KEY = "ComBoom_UndoCount";
        private const string BOMB_COUNT_KEY = "ComBoom_BombCount";
        private const string SHUFFLE_COUNT_KEY = "ComBoom_ShuffleCount";
        private const string COUNTS_INITIALIZED_KEY = "ComBoom_CountsInit";

        public bool IsBombMode => isBombMode;
        public bool CanUndo => canUndo;

        private void Start()
        {
            LoadCounts();
            UpdateUndoState();
        }

        private void LoadCounts()
        {
            if (PlayerPrefs.GetInt(COUNTS_INITIALIZED_KEY, 0) == 0)
            {
                // Ilk kez: 3'er adet ver
                undoCount = 3;
                bombCount = 3;
                shuffleCount = 3;
                PlayerPrefs.SetInt(COUNTS_INITIALIZED_KEY, 1);
                SaveCounts();
            }
            else
            {
                undoCount = PlayerPrefs.GetInt(UNDO_COUNT_KEY, 0);
                bombCount = PlayerPrefs.GetInt(BOMB_COUNT_KEY, 0);
                shuffleCount = PlayerPrefs.GetInt(SHUFFLE_COUNT_KEY, 0);
            }
            UpdateBadges();
        }

        private void SaveCounts()
        {
            PlayerPrefs.SetInt(UNDO_COUNT_KEY, undoCount);
            PlayerPrefs.SetInt(BOMB_COUNT_KEY, bombCount);
            PlayerPrefs.SetInt(SHUFFLE_COUNT_KEY, shuffleCount);
            PlayerPrefs.Save();
        }

        public void GrantLevelUpRewards()
        {
            undoCount++;
            bombCount++;
            shuffleCount++;
            SaveCounts();
            UpdateBadges();
        }

        private void UpdateBadges()
        {
            if (undoCountText != null) undoCountText.text = undoCount.ToString();
            if (bombCountText != null) bombCountText.text = bombCount.ToString();
            if (shuffleCountText != null) shuffleCountText.text = shuffleCount.ToString();

            // Bomb ve Shuffle butonlarini count=0 ise disable et
            if (bombButton != null) bombButton.interactable = bombCount > 0;
            if (shuffleButton != null) shuffleButton.interactable = shuffleCount > 0;
        }

        private void UpdateUndoState()
        {
            // Undo: hem canUndo hem de undoCount > 0 olmali
            bool interactable = canUndo && undoCount > 0;
            SetUndoInteractable(interactable);
        }

        public void SaveSnapshot(int comboCount, bool linesClearedThisTurn)
        {
            lastGridSnapshot = gridManager.GridData.CreateSnapshot();
            lastPieceSnapshot = pieceSpawner.CreateSnapshot();
            lastScore = scoreManager.CurrentScore;
            lastComboCount = comboCount;
            lastLinesClearedThisTurn = linesClearedThisTurn;
            canUndo = true;
            UpdateUndoState();
        }

        public void UseUndo()
        {
            if (!canUndo || undoCount <= 0 || lastGridSnapshot == null || lastPieceSnapshot == null)
                return;

            var gm = GameManager.Instance;
            if (gm == null || gm.CurrentState != GameState.Playing)
                return;

            // Count azalt
            undoCount--;
            SaveCounts();

            // Grid restore
            gridManager.GridData.RestoreSnapshot(lastGridSnapshot.Value);
            gridManager.RestoreVisuals(lastGridSnapshot.Value);

            // Piece restore
            pieceSpawner.RestoreSnapshot(lastPieceSnapshot.Value);

            // Score restore
            scoreManager.SetScore(lastScore);

            // Combo state restore
            gm.RestoreComboState(lastComboCount, lastLinesClearedThisTurn);

            canUndo = false;
            lastGridSnapshot = null;
            lastPieceSnapshot = null;
            UpdateUndoState();
            UpdateBadges();

            // Haptic feedback
            HapticManager.ImpactLight();
        }

        public void UseBomb()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.CurrentState != GameState.Playing)
                return;

            if (isBombMode)
            {
                CancelBombMode();
                return;
            }

            if (bombCount <= 0) return;

            isBombMode = true;

            // Drag'i devre disi birak (cakisma onle)
            if (dragDropHandler != null)
                dragDropHandler.enabled = false;
        }

        private void CancelBombMode()
        {
            isBombMode = false;
            if (dragDropHandler != null)
                dragDropHandler.enabled = true;
        }

        private void ConsumeBomb()
        {
            bombCount--;
            SaveCounts();
            UpdateBadges();
        }

        public void UseShuffle()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.CurrentState != GameState.Playing)
                return;

            if (shuffleCount <= 0) return;

            if (pieceSpawner.GetUnusedCount() == 0)
                return;

            // Count azalt
            shuffleCount--;
            SaveCounts();

            pieceSpawner.ShuffleUnusedPieces();

            // Shuffle sonrasi game over kontrolu
            if (!pieceSpawner.HasAnyPlaceablePiece(gridManager.GridData))
            {
                // Shuffle bile kurtaramadiysa, tekrar dene (1 kez daha)
                pieceSpawner.ShuffleUnusedPieces();
            }

            UpdateBadges();

            // Haptic feedback
            HapticManager.ImpactLight();
        }

        private void Update()
        {
            if (!isBombMode) return;

            var pointer = Pointer.current;
            if (pointer == null) return;

            if (pointer.press.wasPressedThisFrame)
            {
                HandleBombTap(pointer);
            }
        }

        private void HandleBombTap(Pointer pointer)
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            if (mainCamera == null)
            {
                CancelBombMode();
                return;
            }

            Vector2 screenPos = pointer.position.ReadValue();
            Vector3 mousePos = new Vector3(screenPos.x, screenPos.y, -mainCamera.transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

            if (gridManager.WorldToGrid(worldPos, out int row, out int col))
            {
                if (gridManager.GridData.IsOccupied(row, col))
                {
                    gridManager.ClearSingleCell(row, col);
                    ConsumeBomb();
                    HapticManager.ImpactHeavy();
                    CancelBombMode();
                    return;
                }
            }

            // Grid disina veya bos hucreye tiklanirsa iptal
            CancelBombMode();
        }

        private void SetUndoInteractable(bool interactable)
        {
            if (undoButton == null) return;
            undoButton.interactable = interactable;
        }
    }
}
