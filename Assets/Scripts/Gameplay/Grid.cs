using System.Collections.Generic;
using UnityEngine;

namespace ComBoom.Gameplay
{
    public struct GridSnapshot
    {
        public int[,] cells;
        public Color[,] cellColors;
    }

    public class GameGrid
    {
        public const int SIZE = 8;
        private int[,] cells = new int[SIZE, SIZE];
        private Color[,] cellColors = new Color[SIZE, SIZE];

        public void Clear()
        {
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                {
                    cells[r, c] = 0;
                    cellColors[r, c] = Color.clear;
                }
        }

        public Color GetCellColor(int row, int col)
        {
            return cellColors[row, col];
        }

        public bool IsOccupied(int row, int col)
        {
            return cells[row, col] != 0;
        }

        public void SetCell(int row, int col, int value)
        {
            cells[row, col] = value;
        }

        public bool CanPlacePiece(PieceData piece, int startRow, int startCol)
        {
            for (int r = 0; r < piece.Rows; r++)
            {
                for (int c = 0; c < piece.Cols; c++)
                {
                    if (!piece.shape[r, c]) continue;

                    int targetRow = startRow + r;
                    int targetCol = startCol + c;

                    if (targetRow < 0 || targetRow >= SIZE) return false;
                    if (targetCol < 0 || targetCol >= SIZE) return false;
                    if (cells[targetRow, targetCol] != 0) return false;
                }
            }
            return true;
        }

        public void PlacePiece(PieceData piece, int startRow, int startCol, Color pieceColor)
        {
            for (int r = 0; r < piece.Rows; r++)
            {
                for (int c = 0; c < piece.Cols; c++)
                {
                    if (!piece.shape[r, c]) continue;

                    int targetRow = startRow + r;
                    int targetCol = startCol + c;
                    cells[targetRow, targetCol] = 1;
                    cellColors[targetRow, targetCol] = pieceColor;
                }
            }
        }

        public bool IsRowFull(int row)
        {
            for (int c = 0; c < SIZE; c++)
                if (cells[row, c] == 0) return false;
            return true;
        }

        public bool IsColFull(int col)
        {
            for (int r = 0; r < SIZE; r++)
                if (cells[r, col] == 0) return false;
            return true;
        }

        public List<int> GetFullRows()
        {
            var fullRows = new List<int>();
            for (int r = 0; r < SIZE; r++)
                if (IsRowFull(r)) fullRows.Add(r);
            return fullRows;
        }

        public List<int> GetFullCols()
        {
            var fullCols = new List<int>();
            for (int c = 0; c < SIZE; c++)
                if (IsColFull(c)) fullCols.Add(c);
            return fullCols;
        }

        public void ClearRow(int row)
        {
            for (int c = 0; c < SIZE; c++)
            {
                cells[row, c] = 0;
                cellColors[row, c] = Color.clear;
            }
        }

        public void ClearCol(int col)
        {
            for (int r = 0; r < SIZE; r++)
            {
                cells[r, col] = 0;
                cellColors[r, col] = Color.clear;
            }
        }

        public void GetPotentialClears(PieceData piece, int startRow, int startCol,
            out List<int> clearRows, out List<int> clearCols)
        {
            clearRows = new List<int>();
            clearCols = new List<int>();

            if (!CanPlacePiece(piece, startRow, startCol)) return;

            // Parcaanin kaplayacagi hucreleri topla
            HashSet<int> affectedRows = new HashSet<int>();
            HashSet<int> affectedCols = new HashSet<int>();

            for (int r = 0; r < piece.Rows; r++)
            {
                for (int c = 0; c < piece.Cols; c++)
                {
                    if (!piece.shape[r, c]) continue;
                    affectedRows.Add(startRow + r);
                    affectedCols.Add(startCol + c);
                }
            }

            // Satirlari kontrol et (mevcut dolu + yeni parca)
            foreach (int row in affectedRows)
            {
                bool full = true;
                for (int c = 0; c < SIZE; c++)
                {
                    bool occupied = cells[row, c] != 0;
                    bool willPlace = false;

                    // Bu hucre parca tarafindan doldurulacak mi?
                    int pieceR = row - startRow;
                    int pieceC = c - startCol;
                    if (pieceR >= 0 && pieceR < piece.Rows && pieceC >= 0 && pieceC < piece.Cols)
                        willPlace = piece.shape[pieceR, pieceC];

                    if (!occupied && !willPlace)
                    {
                        full = false;
                        break;
                    }
                }
                if (full) clearRows.Add(row);
            }

            // Sutunlari kontrol et
            foreach (int col in affectedCols)
            {
                bool full = true;
                for (int r = 0; r < SIZE; r++)
                {
                    bool occupied = cells[r, col] != 0;
                    bool willPlace = false;

                    int pieceR = r - startRow;
                    int pieceC = col - startCol;
                    if (pieceR >= 0 && pieceR < piece.Rows && pieceC >= 0 && pieceC < piece.Cols)
                        willPlace = piece.shape[pieceR, pieceC];

                    if (!occupied && !willPlace)
                    {
                        full = false;
                        break;
                    }
                }
                if (full) clearCols.Add(col);
            }
        }

        public bool IsEmpty()
        {
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                    if (cells[r, c] != 0) return false;
            return true;
        }

        public bool CanPlaceAny(PieceData piece)
        {
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                    if (CanPlacePiece(piece, r, c)) return true;
            return false;
        }

        public GridSnapshot CreateSnapshot()
        {
            var snapshot = new GridSnapshot
            {
                cells = new int[SIZE, SIZE],
                cellColors = new Color[SIZE, SIZE]
            };
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                {
                    snapshot.cells[r, c] = cells[r, c];
                    snapshot.cellColors[r, c] = cellColors[r, c];
                }
            return snapshot;
        }

        public void RestoreSnapshot(GridSnapshot snapshot)
        {
            for (int r = 0; r < SIZE; r++)
                for (int c = 0; c < SIZE; c++)
                {
                    cells[r, c] = snapshot.cells[r, c];
                    cellColors[r, c] = snapshot.cellColors[r, c];
                }
        }

        public void ClearCell(int row, int col)
        {
            cells[row, col] = 0;
            cellColors[row, col] = Color.clear;
        }
    }
}
