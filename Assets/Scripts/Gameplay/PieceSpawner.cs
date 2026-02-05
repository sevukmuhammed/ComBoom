using UnityEngine;
using System;

namespace ComBoom.Gameplay
{
    public struct PieceSnapshot
    {
        public bool[] isUsed;
        public PieceData[] data;
        public Color[] colors;
        public float[] scales;
    }

    public class PieceSpawner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform[] slotPositions = new Transform[3];
        [SerializeField] private Sprite blockSprite;

        [Header("Grid Match - GridManager ile ayni olmali")]
        [SerializeField] private float cellSize = 0.65f;
        [SerializeField] private float cellSpacing = 0.05f;

        private Piece[] currentPieces = new Piece[3];

        private static readonly Color[] pieceColors = new Color[]
        {
            new Color(0.063f, 0.725f, 0.506f, 1f),  // Jewel Green  #10B981
            new Color(0.231f, 0.510f, 0.965f, 1f),  // Jewel Blue   #3B82F6
            new Color(0.937f, 0.267f, 0.267f, 1f),  // Jewel Red    #EF4444
            new Color(0.961f, 0.620f, 0.043f, 1f),  // Jewel Orange #F59E0B
            new Color(0.925f, 0.282f, 0.600f, 1f),  // Jewel Pink   #EC4899
            new Color(0.545f, 0.361f, 0.965f, 1f),  // Jewel Purple #8B5CF6
        };

        public Piece[] CurrentPieces => currentPieces;
        private float TotalCellSize => cellSize + cellSpacing;

        public event Action OnAllPiecesUsed;

        public void SpawnNewSet()
        {
            // Sprite yoksa runtime'da olustur
            if (blockSprite == null)
                blockSprite = SpriteGenerator.CreateBlockSprite();

            // 3 parca icin 3 farkli renk sec
            Color[] setColors = PickDistinctColors(3);
            for (int i = 0; i < 3; i++)
            {
                SpawnPieceAtSlot(i, setColors[i]);
            }
        }

        private Color[] PickDistinctColors(int count)
        {
            Color[] result = new Color[count];
            int[] indices = new int[pieceColors.Length];
            for (int i = 0; i < indices.Length; i++) indices[i] = i;

            // Fisher-Yates partial shuffle
            for (int i = 0; i < count; i++)
            {
                int j = UnityEngine.Random.Range(i, indices.Length);
                int temp = indices[i];
                indices[i] = indices[j];
                indices[j] = temp;
                result[i] = pieceColors[indices[i]];
            }
            return result;
        }

        private void SpawnPieceAtSlot(int slotIndex, Color color)
        {
            // Eski parcayi temizle
            if (currentPieces[slotIndex] != null)
            {
                Destroy(currentPieces[slotIndex].gameObject);
            }

            PieceData randomPiece = PieceDatabase.GetRandom();

            GameObject pieceObj = new GameObject($"Piece_Slot{slotIndex}");
            pieceObj.transform.position = slotPositions[slotIndex].position;

            Piece piece = pieceObj.AddComponent<Piece>();
            piece.Initialize(randomPiece, blockSprite, color, TotalCellSize, cellSize);

            // Parcayi slot boyutuna gore olcekle
            float fitScale = CalculatePieceScale(randomPiece);
            piece.SetSlotScale(fitScale);
            piece.SaveOriginalPosition();

            // Collider ekle (drag icin) - kucuk parcalar icin minimum dokunma alani
            BoxCollider2D collider = pieceObj.AddComponent<BoxCollider2D>();
            float width = randomPiece.Cols * TotalCellSize;
            float height = randomPiece.Rows * TotalCellSize;
            float minTouchSize = 2.0f / fitScale; // Scale'den bagimsiz min ~2 unit
            collider.size = new Vector2(
                Mathf.Max(width * 1.2f, minTouchSize),
                Mathf.Max(height * 1.2f, minTouchSize)
            );

            currentPieces[slotIndex] = piece;
        }

        public void OnPieceUsed(Piece piece)
        {
            piece.MarkAsUsed();

            if (AllPiecesUsed())
            {
                OnAllPiecesUsed?.Invoke();
            }
        }

        private bool AllPiecesUsed()
        {
            for (int i = 0; i < currentPieces.Length; i++)
            {
                if (currentPieces[i] != null && !currentPieces[i].IsUsed)
                    return false;
            }
            return true;
        }

        private float CalculatePieceScale(PieceData piece)
        {
            float slotSize = GetSlotAvailableSize();
            float maxDim = Mathf.Max(piece.Cols, piece.Rows) * TotalCellSize;
            float scale = (slotSize * 0.8f) / maxDim;
            return Mathf.Clamp(scale, 0.3f, 0.7f);
        }

        private float GetSlotAvailableSize()
        {
            if (slotPositions != null && slotPositions.Length >= 2
                && slotPositions[0] != null && slotPositions[1] != null)
            {
                return Vector3.Distance(slotPositions[0].position, slotPositions[1].position);
            }
            return 2.0f;
        }

        public bool HasAnyPlaceablePiece(GameGrid grid)
        {
            for (int i = 0; i < currentPieces.Length; i++)
            {
                if (currentPieces[i] == null || currentPieces[i].IsUsed)
                    continue;

                if (grid.CanPlaceAny(currentPieces[i].Data))
                    return true;
            }
            return false;
        }

        public int GetUnusedCount()
        {
            int count = 0;
            for (int i = 0; i < currentPieces.Length; i++)
            {
                if (currentPieces[i] != null && !currentPieces[i].IsUsed)
                    count++;
            }
            return count;
        }

        public PieceSnapshot CreateSnapshot()
        {
            var snapshot = new PieceSnapshot
            {
                isUsed = new bool[3],
                data = new PieceData[3],
                colors = new Color[3],
                scales = new float[3]
            };
            for (int i = 0; i < 3; i++)
            {
                if (currentPieces[i] != null)
                {
                    snapshot.isUsed[i] = currentPieces[i].IsUsed;
                    snapshot.data[i] = currentPieces[i].Data;
                    snapshot.colors[i] = currentPieces[i].PieceColor;
                    snapshot.scales[i] = currentPieces[i].transform.localScale.x;
                }
                else
                {
                    snapshot.isUsed[i] = true;
                }
            }
            return snapshot;
        }

        public void RestoreSnapshot(PieceSnapshot snapshot)
        {
            if (blockSprite == null)
                blockSprite = SpriteGenerator.CreateBlockSprite();

            for (int i = 0; i < 3; i++)
            {
                // Mevcut piece'i temizle
                if (currentPieces[i] != null)
                    Destroy(currentPieces[i].gameObject);

                if (snapshot.data[i] == null)
                {
                    currentPieces[i] = null;
                    continue;
                }

                // Piece'i yeniden olustur
                GameObject pieceObj = new GameObject($"Piece_Slot{i}");
                pieceObj.transform.position = slotPositions[i].position;

                Piece piece = pieceObj.AddComponent<Piece>();
                piece.Initialize(snapshot.data[i], blockSprite, snapshot.colors[i], TotalCellSize, cellSize);

                float fitScale = CalculatePieceScale(snapshot.data[i]);
                piece.SetSlotScale(fitScale);
                piece.SaveOriginalPosition();

                BoxCollider2D collider = pieceObj.AddComponent<BoxCollider2D>();
                float width = snapshot.data[i].Cols * TotalCellSize;
                float height = snapshot.data[i].Rows * TotalCellSize;
                float minTouchSize = 2.0f / fitScale;
                collider.size = new Vector2(
                    Mathf.Max(width * 1.2f, minTouchSize),
                    Mathf.Max(height * 1.2f, minTouchSize)
                );

                currentPieces[i] = piece;

                if (snapshot.isUsed[i])
                    piece.MarkAsUsed();
            }
        }

        public void ShuffleUnusedPieces()
        {
            if (blockSprite == null)
                blockSprite = SpriteGenerator.CreateBlockSprite();

            Color[] newColors = PickDistinctColors(3);
            int colorIdx = 0;

            for (int i = 0; i < 3; i++)
            {
                if (currentPieces[i] == null || currentPieces[i].IsUsed)
                    continue;

                // Eski piece'i sil, yeni rastgele olustur
                Destroy(currentPieces[i].gameObject);
                SpawnPieceAtSlot(i, newColors[colorIdx % newColors.Length]);
                colorIdx++;
            }
        }
    }
}
