using UnityEngine;
using UnityEngine.InputSystem;
using System;
using ComBoom.Core;
using ComBoom.Gameplay;

namespace ComBoom.Input
{
    public class DragDropHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private Camera mainCamera;

        [Header("Audio")]
        [SerializeField] private AudioManager audioManager;

        [Header("Settings")]
        [SerializeField] private float dragScale = 1.0f;
        [SerializeField] private float dragOffsetY = 1.5f;
        [SerializeField] private float touchRadius = 0.8f;

        private Piece selectedPiece;
        private bool isDragging;
        private int lastPreviewRow = -1;
        private int lastPreviewCol = -1;

        public event Action OnBeforePiecePlaced;
        public event Action<Piece, int, int> OnPiecePlaced;

        private void Update()
        {
            if (mainCamera == null) return;

            HandleInput();
        }

        private void HandleInput()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            var pointer = Pointer.current;
            if (pointer == null) return;

            bool pressed = pointer.press.wasPressedThisFrame;
            bool held = pointer.press.isPressed;
            bool released = pointer.press.wasReleasedThisFrame;

            if (pressed)
            {
                OnTouchBegin(GetWorldPoint(pointer));
            }
            else if (held && isDragging)
            {
                OnTouchMove(GetWorldPoint(pointer));
            }
            else if (released && isDragging)
            {
                OnTouchEnd(GetWorldPoint(pointer));
            }
        }

        private Vector3 GetWorldPoint(Pointer pointer)
        {
            Vector2 screenPos = pointer.position.ReadValue();
            Vector3 mousePos = new Vector3(screenPos.x, screenPos.y, -mainCamera.transform.position.z);
            return mainCamera.ScreenToWorldPoint(mousePos);
        }

        private void OnTouchBegin(Vector3 worldPoint)
        {
            // Dairesel arama: parmaga en yakin parcayi bul (kucuk parcalar icin toleransli)
            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPoint, touchRadius);

            Piece closestPiece = null;
            float closestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                Piece piece = hit.GetComponent<Piece>();
                if (piece == null || piece.IsUsed) continue;

                float dist = Vector2.Distance(worldPoint, piece.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestPiece = piece;
                }
            }

            if (closestPiece == null) return;

            selectedPiece = closestPiece;
            isDragging = true;

            if (audioManager != null) audioManager.PlayPick();

            // Gorsel feedback: biraz buyut
            selectedPiece.SetScale(dragScale);
            selectedPiece.SetAlpha(0.8f);
        }

        private void OnTouchMove(Vector3 worldPoint)
        {
            if (selectedPiece == null) return;

            // Parcayi parmakla birlikte hareket ettir (Y offset ile parmak parcayi kapatmasin)
            Vector3 dragPos = worldPoint + Vector3.up * dragOffsetY;
            selectedPiece.transform.position = dragPos;

            // Piece merkezi yerine sol-ust kosesini grid'e eslestir
            Vector3 pieceTopLeft = GetPieceTopLeftWorld(dragPos, selectedPiece.Data);

            // Grid uzerinde mi kontrol et
            if (gridManager.WorldToGrid(pieceTopLeft, out int row, out int col))
            {
                // Sadece pozisyon degistiyse preview guncelle
                if (row != lastPreviewRow || col != lastPreviewCol)
                {
                    gridManager.ShowPlacementPreview(selectedPiece.Data, row, col);
                    lastPreviewRow = row;
                    lastPreviewCol = col;
                }
            }
            else
            {
                gridManager.ClearPreview();
                lastPreviewRow = -1;
                lastPreviewCol = -1;
            }
        }

        private void OnTouchEnd(Vector3 worldPoint)
        {
            if (selectedPiece == null)
            {
                isDragging = false;
                return;
            }

            gridManager.ClearPreview();

            Vector3 dropPos = worldPoint + Vector3.up * dragOffsetY;
            Vector3 pieceTopLeft = GetPieceTopLeftWorld(dropPos, selectedPiece.Data);

            if (gridManager.WorldToGrid(pieceTopLeft, out int row, out int col))
            {
                // Yerlestirme basarili olacaksa ONCE snapshot kaydet
                if (gridManager.GridData.CanPlacePiece(selectedPiece.Data, row, col))
                    OnBeforePiecePlaced?.Invoke();

                if (gridManager.TryPlacePiece(selectedPiece.Data, row, col, selectedPiece.PieceColor))
                {
                    OnPiecePlaced?.Invoke(selectedPiece, row, col);
                    selectedPiece = null;
                    isDragging = false;
                    lastPreviewRow = -1;
                    lastPreviewCol = -1;
                    return;
                }
            }

            // Gecersiz pozisyon: parcayi geri dondur
            selectedPiece.ReturnToOriginal();
            selectedPiece.SetAlpha(1f);

            selectedPiece = null;
            isDragging = false;
            lastPreviewRow = -1;
            lastPreviewCol = -1;
        }

        private Vector3 GetPieceTopLeftWorld(Vector3 pieceCenter, PieceData piece)
        {
            float offsetX = (piece.Cols - 1) * gridManager.TotalCellSize * 0.5f;
            float offsetY = (piece.Rows - 1) * gridManager.TotalCellSize * 0.5f;
            return pieceCenter + new Vector3(-offsetX, offsetY, 0);
        }
    }
}
