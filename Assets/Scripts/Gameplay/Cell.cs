using UnityEngine;
using System.Collections;

namespace ComBoom.Gameplay
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private int row;
        private int col;
        private bool isOccupied;
        private Vector3 baseScale;

        public int Row => row;
        public int Col => col;
        public bool IsOccupied => isOccupied;

        public void Initialize(int row, int col)
        {
            this.row = row;
            this.col = col;
            this.isOccupied = false;
            this.spriteRenderer = GetComponent<SpriteRenderer>();
            this.baseScale = transform.localScale;
        }

        public void SetOccupied(bool occupied, Color color)
        {
            StopAllCoroutines();
            isOccupied = occupied;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
            transform.localScale = baseScale;
        }

        public void PlayPlaceAnimation(Color targetColor)
        {
            isOccupied = true;
            StartCoroutine(PlaceAnimationRoutine(targetColor));
        }

        private IEnumerator PlaceAnimationRoutine(Color targetColor)
        {
            if (spriteRenderer == null) yield break;

            // 1. Parlak beyaz flash
            Color flashColor = new Color(1f, 1f, 1f, 0.9f);
            spriteRenderer.color = flashColor;

            // 2. Buyuterek pop efekti
            float popScale = 1.25f;
            float duration = 0.15f;
            float elapsed = 0f;

            // Scale up
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = Mathf.Lerp(1f, popScale, t);
                transform.localScale = baseScale * scale;
                yield return null;
            }

            // Rengi hedefe gecir
            spriteRenderer.color = targetColor;

            // Scale down (bounce back)
            elapsed = 0f;
            duration = 0.12f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Overshoot bounce: biraz asagi inip geri gelir
                float scale = Mathf.Lerp(popScale, 1f, t) + Mathf.Sin(t * Mathf.PI) * 0.05f;
                transform.localScale = baseScale * scale;
                yield return null;
            }

            transform.localScale = baseScale;
        }

        public void PlayClearAnimation(Color emptyColor)
        {
            StartCoroutine(ClearAnimationRoutine(emptyColor));
        }

        private IEnumerator ClearAnimationRoutine(Color emptyColor)
        {
            if (spriteRenderer == null) yield break;

            isOccupied = false;

            // Parlak flash
            Color flashColor = new Color(0.8f, 0.95f, 1.0f, 1f);
            spriteRenderer.color = flashColor;

            // Shrink + fade
            float duration = 0.2f;
            float elapsed = 0f;
            Color startColor = flashColor;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = Mathf.Lerp(1f, 0.6f, t);
                transform.localScale = baseScale * scale;
                spriteRenderer.color = Color.Lerp(startColor, emptyColor, t);
                yield return null;
            }

            // Geri buyut
            elapsed = 0f;
            duration = 0.1f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = Mathf.Lerp(0.6f, 1f, t);
                transform.localScale = baseScale * scale;
                yield return null;
            }

            transform.localScale = baseScale;
            spriteRenderer.color = emptyColor;
        }

        public void SetHighlight(bool show, Color color)
        {
            if (spriteRenderer != null && !isOccupied)
            {
                Color c = show ? color : GetEmptyColor();
                spriteRenderer.color = c;
            }
        }

        public void SetLineClearHighlight(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        public void RestoreColor(Color occupiedColor)
        {
            if (spriteRenderer != null && isOccupied)
            {
                spriteRenderer.color = occupiedColor;
            }
        }

        public void ClearCell(Color emptyColor)
        {
            StopAllCoroutines();
            isOccupied = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = emptyColor;
            }
            transform.localScale = baseScale;
        }

        private Color GetEmptyColor()
        {
            return new Color(0.08f, 0.11f, 0.18f, 1f); // slate-800/30 effective
        }
    }
}
