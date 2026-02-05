using UnityEngine;
using System.Collections.Generic;

namespace ComBoom.Gameplay
{
    public class Piece : MonoBehaviour
    {
        private PieceData data;
        private List<SpriteRenderer> blockVisuals = new List<SpriteRenderer>();
        private Vector3 originalPosition;
        private float slotScale = 1f;
        private bool isUsed;
        private Color pieceColor;

        public PieceData Data => data;
        public bool IsUsed => isUsed;
        public Color PieceColor => pieceColor;

        public void Initialize(PieceData pieceData, Sprite blockSprite, Color blockColor, float cellSpacing, float blockScale)
        {
            data = pieceData;
            isUsed = false;
            pieceColor = blockColor;

            // Mevcut gorselleri temizle
            foreach (var visual in blockVisuals)
            {
                if (visual != null) Destroy(visual.gameObject);
            }
            blockVisuals.Clear();

            // Parca seklini olustur
            // cellSpacing = grid hucreler arasi mesafe (TotalCellSize)
            // blockScale = her bir blok gorseli boyutu (cellSize)
            float offsetX = (data.Cols - 1) * cellSpacing * 0.5f;
            float offsetY = (data.Rows - 1) * cellSpacing * 0.5f;

            for (int r = 0; r < data.Rows; r++)
            {
                for (int c = 0; c < data.Cols; c++)
                {
                    if (!data.shape[r, c]) continue;

                    GameObject blockObj = new GameObject($"Block_{r}_{c}");
                    blockObj.transform.SetParent(transform);

                    float x = c * cellSpacing - offsetX;
                    float y = -r * cellSpacing + offsetY;
                    blockObj.transform.localPosition = new Vector3(x, y, 0);
                    blockObj.transform.localScale = Vector3.one * blockScale;

                    SpriteRenderer sr = blockObj.AddComponent<SpriteRenderer>();
                    sr.sprite = blockSprite;
                    sr.color = blockColor;
                    sr.sortingOrder = 10;

                    blockVisuals.Add(sr);
                }
            }

            originalPosition = transform.position;
        }

        public void SaveOriginalPosition()
        {
            originalPosition = transform.position;
        }

        public void ReturnToOriginal()
        {
            transform.position = originalPosition;
            transform.localScale = Vector3.one * slotScale;
        }

        public void SetScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }

        public void SetSlotScale(float scale)
        {
            slotScale = scale;
            transform.localScale = Vector3.one * scale;
        }

        public void MarkAsUsed()
        {
            isUsed = true;
            gameObject.SetActive(false);
        }

        public void RestoreFromUsed()
        {
            isUsed = false;
            gameObject.SetActive(true);
            ReturnToOriginal();
        }

        public void SetAlpha(float alpha)
        {
            foreach (var sr in blockVisuals)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
            }
        }
    }
}
