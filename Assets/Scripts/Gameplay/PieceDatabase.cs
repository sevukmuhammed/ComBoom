using System.Collections.Generic;

namespace ComBoom.Gameplay
{
    public enum PieceSize { Small, Medium, Large }

    public static class PieceDatabase
    {
        private static List<PieceData> allPieces;

        public static List<PieceData> AllPieces
        {
            get
            {
                if (allPieces == null)
                    Initialize();
                return allPieces;
            }
        }

        private static void Initialize()
        {
            allPieces = new List<PieceData>
            {
                // === TEKLI (1 blok) ===
                new PieceData("Single", new bool[,] {
                    { true }
                }),

                // === IKILI (2 blok) ===
                new PieceData("Line2H", new bool[,] {
                    { true, true }
                }),
                new PieceData("Line2V", new bool[,] {
                    { true },
                    { true }
                }),

                // === UCLU (3 blok) ===
                new PieceData("Line3H", new bool[,] {
                    { true, true, true }
                }),
                new PieceData("Line3V", new bool[,] {
                    { true },
                    { true },
                    { true }
                }),
                new PieceData("SmallL", new bool[,] {
                    { true, false },
                    { true, true }
                }),
                new PieceData("SmallLR", new bool[,] {
                    { false, true },
                    { true, true }
                }),

                // === DORTLU (4 blok) ===
                new PieceData("Square2x2", new bool[,] {
                    { true, true },
                    { true, true }
                }),
                new PieceData("Line4H", new bool[,] {
                    { true, true, true, true }
                }),
                new PieceData("Line4V", new bool[,] {
                    { true },
                    { true },
                    { true },
                    { true }
                }),
                new PieceData("LShape", new bool[,] {
                    { true, false },
                    { true, false },
                    { true, true }
                }),
                new PieceData("LShapeR", new bool[,] {
                    { false, true },
                    { false, true },
                    { true, true }
                }),
                new PieceData("TShape", new bool[,] {
                    { true, true, true },
                    { false, true, false }
                }),

                // === BESLI (5 blok) ===
                new PieceData("Line5H", new bool[,] {
                    { true, true, true, true, true }
                }),
                new PieceData("Line5V", new bool[,] {
                    { true },
                    { true },
                    { true },
                    { true },
                    { true }
                }),
                new PieceData("BigL", new bool[,] {
                    { true, false, false },
                    { true, false, false },
                    { true, true, true }
                }),
                new PieceData("BigLR", new bool[,] {
                    { false, false, true },
                    { false, false, true },
                    { true, true, true }
                }),

                // === BUYUK (9 blok) ===
                new PieceData("Square3x3", new bool[,] {
                    { true, true, true },
                    { true, true, true },
                    { true, true, true }
                }),
            };
        }

        public static PieceData GetRandom()
        {
            int index = UnityEngine.Random.Range(0, AllPieces.Count);
            return AllPieces[index];
        }

        public static PieceSize GetPieceSize(PieceData piece)
        {
            if (piece.blockCount <= 3) return PieceSize.Small;
            if (piece.blockCount <= 4) return PieceSize.Medium;
            return PieceSize.Large;
        }

        public static List<PieceData> GetPiecesBySize(PieceSize size)
        {
            var result = new List<PieceData>();
            foreach (var piece in AllPieces)
            {
                if (GetPieceSize(piece) == size)
                    result.Add(piece);
            }
            return result;
        }
    }
}
