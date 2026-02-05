using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ComBoom.Gameplay;
using ComBoom.UI;

namespace ComBoom.Core
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float cellSize = 0.65f;
        [SerializeField] private float cellSpacing = 0.05f;
        [SerializeField] private Sprite cellSprite;

        [Header("Colors")]
        [SerializeField] private Color emptyColor = new Color(0.08f, 0.11f, 0.18f, 1f);         // slate-800/30
        [SerializeField] private Color validHighlight = new Color(0.063f, 0.725f, 0.506f, 0.40f); // Green #10B981
        [SerializeField] private Color invalidHighlight = new Color(0.937f, 0.267f, 0.267f, 0.40f); // Red #EF4444
        [SerializeField] private Color lineClearHighlight = new Color(1f, 1f, 1f, 0.55f);       // Beyaz flas

        private GameGrid grid;
        private Cell[,] cellObjects;
        private Vector3 gridOrigin;
        private HashSet<Vector2Int> lineClearHighlightedCells = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> lastClearedCells = new HashSet<Vector2Int>();
        private Dictionary<Vector2Int, Color> lastClearedColors = new Dictionary<Vector2Int, Color>();
        private Color[,] cellColorMap;

        public GameGrid GridData => grid;
        public float CellSize => cellSize;
        public float TotalCellSize => cellSize + cellSpacing;
        public Cell[,] CellObjects => cellObjects;
        public Color EmptyColor => emptyColor;

        public Color GetCellColor(int row, int col) => cellColorMap != null ? cellColorMap[row, col] : Color.clear;

        public event Action<int, Vector3, HashSet<Vector2Int>, Dictionary<Vector2Int, Color>> OnLinesCleared;

        private void Awake()
        {
            grid = new GameGrid();
            cellObjects = new Cell[GameGrid.SIZE, GameGrid.SIZE];
        }

        public void InitializeGrid()
        {
            grid.Clear();
            cellColorMap = new Color[GameGrid.SIZE, GameGrid.SIZE];

            // Sprite yoksa runtime'da olustur
            if (cellSprite == null)
                cellSprite = SpriteGenerator.CreateCellSprite();

            // Onceki hucreleri temizle
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            float totalSize = GameGrid.SIZE * TotalCellSize;
            gridOrigin = new Vector3(-totalSize / 2f + TotalCellSize / 2f,
                                      totalSize / 2f - TotalCellSize / 2f, 0);
            gridOrigin += transform.position;

            for (int r = 0; r < GameGrid.SIZE; r++)
            {
                for (int c = 0; c < GameGrid.SIZE; c++)
                {
                    Vector3 pos = GetWorldPosition(r, c);

                    GameObject cellObj = new GameObject($"Cell_{r}_{c}");
                    cellObj.transform.SetParent(transform);
                    cellObj.transform.position = pos;

                    SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();
                    sr.sprite = cellSprite;
                    sr.color = emptyColor;
                    sr.sortingOrder = 1;
                    cellObj.transform.localScale = Vector3.one * cellSize;

                    Cell cell = cellObj.AddComponent<Cell>();
                    cell.Initialize(r, c);
                    cellObjects[r, c] = cell;
                }
            }
        }

        public Vector3 GetWorldPosition(int row, int col)
        {
            float x = gridOrigin.x + col * TotalCellSize;
            float y = gridOrigin.y - row * TotalCellSize;
            return new Vector3(x, y, 0);
        }

        public bool WorldToGrid(Vector3 worldPos, out int row, out int col)
        {
            float relX = worldPos.x - gridOrigin.x + TotalCellSize / 2f;
            float relY = gridOrigin.y - worldPos.y + TotalCellSize / 2f;

            col = Mathf.FloorToInt(relX / TotalCellSize);
            row = Mathf.FloorToInt(relY / TotalCellSize);

            return row >= 0 && row < GameGrid.SIZE && col >= 0 && col < GameGrid.SIZE;
        }

        public bool TryPlacePiece(PieceData piece, int row, int col, Color pieceColor)
        {
            if (!grid.CanPlacePiece(piece, row, col))
                return false;

            grid.PlacePiece(piece, row, col, pieceColor);

            // Gorselleri animasyonlu guncelle
            for (int r = 0; r < piece.Rows; r++)
            {
                for (int c = 0; c < piece.Cols; c++)
                {
                    if (!piece.shape[r, c]) continue;
                    int targetRow = row + r;
                    int targetCol = col + c;
                    cellObjects[targetRow, targetCol].PlayPlaceAnimation(pieceColor);
                    cellColorMap[targetRow, targetCol] = pieceColor;
                }
            }

            // Satir/sutun kontrolu
            int linesCleared = CheckAndClearLines(out Vector3 clearCenter);
            if (linesCleared > 0)
            {
                OnLinesCleared?.Invoke(linesCleared, clearCenter, lastClearedCells, lastClearedColors);
            }

            return true;
        }

        private int CheckAndClearLines(out Vector3 clearCenter)
        {
            clearCenter = Vector3.zero;
            List<int> fullRows = grid.GetFullRows();
            List<int> fullCols = grid.GetFullCols();

            int totalLines = fullRows.Count + fullCols.Count;

            if (totalLines == 0) return 0;

            // Temizlenecek tum hucreleri topla (ayni hucre iki kez temizlenmesin)
            lastClearedCells.Clear();
            HashSet<Vector2Int> cellsToClear = lastClearedCells;

            foreach (int row in fullRows)
                for (int c = 0; c < GameGrid.SIZE; c++)
                    cellsToClear.Add(new Vector2Int(row, c));

            foreach (int col in fullCols)
                for (int r = 0; r < GameGrid.SIZE; r++)
                    cellsToClear.Add(new Vector2Int(r, col));

            // Temizlenen hucrelerin ortasini bul (popup pozisyonu)
            clearCenter = Vector3.zero;
            foreach (var pos in cellsToClear)
                clearCenter += GetWorldPosition(pos.x, pos.y);
            clearCenter /= cellsToClear.Count;

            // Temizlenmeden once hucre renklerini yakala
            lastClearedColors.Clear();
            foreach (var pos in cellsToClear)
            {
                lastClearedColors[pos] = cellColorMap[pos.x, pos.y];
            }

            // Grid verisini temizle
            foreach (int row in fullRows) grid.ClearRow(row);
            foreach (int col in fullCols) grid.ClearCol(col);

            // cellColorMap'i temizle
            foreach (var pos in cellsToClear)
                cellColorMap[pos.x, pos.y] = Color.clear;

            // Gorselleri animasyonlu temizle
            foreach (var pos in cellsToClear)
            {
                cellObjects[pos.x, pos.y].PlayClearAnimation(emptyColor);
            }

            return totalLines;
        }

        public void ShowPlacementPreview(PieceData piece, int row, int col)
        {
            ClearPreview();

            bool canPlace = grid.CanPlacePiece(piece, row, col);
            Color highlight = canPlace ? validHighlight : invalidHighlight;

            for (int r = 0; r < piece.Rows; r++)
            {
                for (int c = 0; c < piece.Cols; c++)
                {
                    if (!piece.shape[r, c]) continue;

                    int targetRow = row + r;
                    int targetCol = col + c;

                    if (targetRow < 0 || targetRow >= GameGrid.SIZE) continue;
                    if (targetCol < 0 || targetCol >= GameGrid.SIZE) continue;

                    cellObjects[targetRow, targetCol].SetHighlight(true, highlight);
                }
            }

            // Satir/sutun tamamlanma onizlemesi (sadece gecerli yerlestirmede)
            if (canPlace)
            {
                grid.GetPotentialClears(piece, row, col, out var clearRows, out var clearCols);

                if (clearRows.Count > 0 || clearCols.Count > 0)
                {
                    foreach (int clearRow in clearRows)
                    {
                        for (int c = 0; c < GameGrid.SIZE; c++)
                        {
                            var pos = new Vector2Int(clearRow, c);
                            cellObjects[clearRow, c].SetLineClearHighlight(lineClearHighlight);
                            lineClearHighlightedCells.Add(pos);
                        }
                    }

                    foreach (int clearCol in clearCols)
                    {
                        for (int r = 0; r < GameGrid.SIZE; r++)
                        {
                            var pos = new Vector2Int(r, clearCol);
                            cellObjects[r, clearCol].SetLineClearHighlight(lineClearHighlight);
                            lineClearHighlightedCells.Add(pos);
                        }
                    }
                }
            }
        }

        public void ClearPreview()
        {
            // Standart highlight'lari temizle
            for (int r = 0; r < GameGrid.SIZE; r++)
                for (int c = 0; c < GameGrid.SIZE; c++)
                    cellObjects[r, c].SetHighlight(false, Color.clear);

            // Line clear highlight'li dolu hucreleri eski rengine dondur
            foreach (var pos in lineClearHighlightedCells)
            {
                cellObjects[pos.x, pos.y].RestoreColor(cellColorMap[pos.x, pos.y]);
            }
            lineClearHighlightedCells.Clear();
        }

        public void RestoreVisuals(GridSnapshot snapshot)
        {
            for (int r = 0; r < GameGrid.SIZE; r++)
            {
                for (int c = 0; c < GameGrid.SIZE; c++)
                {
                    bool occupied = snapshot.cells[r, c] != 0;
                    Color color = snapshot.cellColors[r, c];

                    if (occupied)
                    {
                        cellObjects[r, c].SetOccupied(true, color);
                        cellColorMap[r, c] = color;
                    }
                    else
                    {
                        cellObjects[r, c].ClearCell(emptyColor);
                        cellColorMap[r, c] = Color.clear;
                    }
                }
            }
        }

        public void ClearSingleCell(int row, int col)
        {
            Color cellColor = cellColorMap[row, col];
            grid.ClearCell(row, col);
            cellObjects[row, col].PlayClearAnimation(emptyColor);
            cellColorMap[row, col] = Color.clear;

            Vector3 cellWorldPos = GetWorldPosition(row, col);
            BrickBreakEffect.Spawn(cellWorldPos, cellColor, 0);
        }
    }
}
