using UnityEngine;
using TMPro;
using System.Collections;

namespace ComBoom.UI
{
    public class FloatingScore : MonoBehaviour
    {
        private TextMeshPro textMesh;
        private static readonly string[] celebrationTexts = { "SUPER!", "HARIKA!", "MUHTESEM!", "BRAVO!" };

        public static void Spawn(Vector3 worldPosition, int score, int lineCount, int comboCount = 0)
        {
            GameObject obj = new GameObject("FloatingScore");
            obj.transform.position = worldPosition;
            FloatingScore fs = obj.AddComponent<FloatingScore>();
            fs.Play(score, lineCount, comboCount);
        }

        private void Play(int score, int lineCount, int comboCount)
        {
            bool isCombo = comboCount >= 2;
            bool isBigCombo = comboCount >= 3;

            // Ana skor yazisi
            textMesh = gameObject.AddComponent<TextMeshPro>();
            textMesh.text = $"+{score}";
            textMesh.fontSize = isCombo ? 10f : 8f;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.fontStyle = FontStyles.Bold;
            textMesh.sortingOrder = 50;

            if (isBigCombo)
                textMesh.color = new Color(1f, 0.4f, 0.2f, 1f); // Turuncu - buyuk combo
            else if (isCombo)
                textMesh.color = new Color(1f, 0.85f, 0.1f, 1f); // Parlak altin - combo
            else
                textMesh.color = new Color(1f, 0.95f, 0.3f, 1f); // Normal altin

            // Combo veya coklu satir mesaji
            if (isCombo || lineCount >= 2)
            {
                int multiplier = 1 << (comboCount - 1);

                GameObject celebObj = new GameObject("CelebrationText");
                celebObj.transform.SetParent(transform);
                celebObj.transform.localPosition = new Vector3(0, -0.8f, 0);

                TextMeshPro celebText = celebObj.AddComponent<TextMeshPro>();
                celebText.alignment = TextAlignmentOptions.Center;
                celebText.fontStyle = FontStyles.Bold;
                celebText.sortingOrder = 50;

                if (isCombo)
                {
                    celebText.text = $"x{multiplier} COMBO!";
                    celebText.fontSize = 7f;

                    if (isBigCombo)
                        celebText.color = new Color(1f, 0.2f, 0.2f, 1f); // Kirmizi
                    else
                        celebText.color = new Color(1f, 0.6f, 0.1f, 1f); // Turuncu
                }
                else
                {
                    string celebration = celebrationTexts[Random.Range(0, celebrationTexts.Length)];
                    celebText.text = celebration;
                    celebText.fontSize = 6f;
                    celebText.color = new Color(0.3f, 1f, 0.5f, 1f); // Yesil
                }
            }

            int animIntensity = isCombo ? comboCount : (lineCount >= 2 ? 2 : 1);
            StartCoroutine(AnimateRoutine(animIntensity));
        }

        private IEnumerator AnimateRoutine(int lineCount)
        {
            float totalDuration = lineCount >= 2 ? 1.4f : 1.0f;
            float elapsed = 0f;

            Vector3 startPos = transform.position;
            float startScale = 0.3f;
            float peakScale = lineCount >= 2 ? 1.4f : 1.1f;

            // Phase 1: Pop in (scale up + yukari hareket)
            float popDuration = 0.25f;
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / popDuration;

                // Elastic ease out
                float scale = Mathf.Lerp(startScale, peakScale, EaseOutBack(t));
                transform.localScale = Vector3.one * scale;

                float y = Mathf.Lerp(0, 0.5f, t);
                transform.position = startPos + Vector3.up * y;

                yield return null;
            }

            // Phase 2: Havada kalma + hafif sallanma
            float holdDuration = totalDuration * 0.4f;
            float holdStart = elapsed;
            Vector3 holdPos = transform.position;

            while (elapsed < holdStart + holdDuration)
            {
                elapsed += Time.deltaTime;
                float t = (elapsed - holdStart) / holdDuration;

                // Hafif yukari kayma
                float y = Mathf.Lerp(0, 0.3f, t);
                transform.position = holdPos + Vector3.up * y;

                // Hafif sallanma (combo ise)
                if (lineCount >= 2)
                {
                    float wobble = Mathf.Sin(elapsed * 15f) * 0.05f * (1f - t);
                    transform.position += Vector3.right * wobble;
                }

                // Scale settle
                float scale = Mathf.Lerp(peakScale, 1f, EaseOutQuad(t));
                transform.localScale = Vector3.one * scale;

                yield return null;
            }

            // Phase 3: Fade out + yukari ucma
            float fadeDuration = totalDuration * 0.3f;
            float fadeStart = elapsed;
            Vector3 fadePos = transform.position;

            while (elapsed < fadeStart + fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = (elapsed - fadeStart) / fadeDuration;

                // Yukari kayma (hizlanan)
                float y = Mathf.Lerp(0, 1.5f, EaseInQuad(t));
                transform.position = fadePos + Vector3.up * y;

                // Kucul + fade
                float scale = Mathf.Lerp(1f, 0.3f, t);
                transform.localScale = Vector3.one * scale;

                // Alpha fade (tum child text'ler)
                float alpha = Mathf.Lerp(1f, 0f, t);
                SetAlphaRecursive(alpha);

                yield return null;
            }

            Destroy(gameObject);
        }

        private void SetAlphaRecursive(float alpha)
        {
            if (textMesh != null)
            {
                Color c = textMesh.color;
                c.a = alpha;
                textMesh.color = c;
            }

            foreach (Transform child in transform)
            {
                TextMeshPro childText = child.GetComponent<TextMeshPro>();
                if (childText != null)
                {
                    Color c = childText.color;
                    c.a = alpha;
                    childText.color = c;
                }
            }
        }

        // Easing functions
        private static float EaseOutBack(float t)
        {
            float c = 1.7f;
            return 1f + (c + 1f) * Mathf.Pow(t - 1f, 3f) + c * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private static float EaseInQuad(float t)
        {
            return t * t;
        }
    }
}
