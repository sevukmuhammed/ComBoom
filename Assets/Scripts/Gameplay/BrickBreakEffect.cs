using UnityEngine;
using System.Collections;

namespace ComBoom.Gameplay
{
    public class BrickBreakEffect : MonoBehaviour
    {
        private static Sprite fragmentSprite;

        /// <summary>
        /// Bir hucrede tugla kirilma efekti olusturur.
        /// comboLevel: 0-1 normal, 2 combo, 3+ buyuk combo
        /// </summary>
        public static void Spawn(Vector3 worldPosition, Color brickColor, int comboLevel)
        {
            if (fragmentSprite == null)
                fragmentSprite = SpriteGenerator.CreateBrickFragmentSprite();

            GameObject effectObj = new GameObject("BrickBreak");
            effectObj.transform.position = worldPosition;
            BrickBreakEffect effect = effectObj.AddComponent<BrickBreakEffect>();
            effect.StartCoroutine(effect.SpawnFragments(brickColor, comboLevel));
        }

        private IEnumerator SpawnFragments(Color brickColor, int comboLevel)
        {
            // Combo seviyesine gore parca sayisi ve yogunluk
            int fragmentCount;
            float speed;
            float scaleBase;
            float lifetime;

            if (comboLevel >= 4)
            {
                // Mega combo: patlama efekti
                fragmentCount = Random.Range(16, 22);
                speed = Random.Range(7f, 10f);
                scaleBase = 0.24f;
                lifetime = 1.2f;
            }
            else if (comboLevel >= 3)
            {
                // Buyuk combo: cok parca, hizli, buyuk
                fragmentCount = Random.Range(12, 17);
                speed = Random.Range(5.5f, 8f);
                scaleBase = 0.20f;
                lifetime = 1.0f;
            }
            else if (comboLevel >= 2)
            {
                // Combo: orta-guclu
                fragmentCount = Random.Range(8, 13);
                speed = Random.Range(4f, 6f);
                scaleBase = 0.17f;
                lifetime = 0.85f;
            }
            else
            {
                // Normal temizleme
                fragmentCount = Random.Range(4, 7);
                speed = Random.Range(2.5f, 4f);
                scaleBase = 0.12f;
                lifetime = 0.6f;
            }

            // Renk varyasyonu icin taban renk
            Color baseColor = brickColor;

            for (int i = 0; i < fragmentCount; i++)
            {
                GameObject frag = new GameObject($"Frag_{i}");
                frag.transform.SetParent(transform);
                frag.transform.position = transform.position;

                SpriteRenderer sr = frag.AddComponent<SpriteRenderer>();
                sr.sprite = fragmentSprite;
                sr.sortingOrder = 30;

                // Her parca icin hafif renk varyasyonu
                float colorVar = Random.Range(-0.06f, 0.06f);
                sr.color = new Color(
                    Mathf.Clamp01(baseColor.r + colorVar),
                    Mathf.Clamp01(baseColor.g + colorVar),
                    Mathf.Clamp01(baseColor.b + colorVar),
                    1f
                );

                // Rastgele boyut
                float scale = scaleBase * Random.Range(0.6f, 1.4f);
                frag.transform.localScale = Vector3.one * scale;

                // Rastgele yon (yukari agirlikli)
                float angle = Random.Range(15f, 165f) * Mathf.Deg2Rad;
                float fragSpeed = speed * Random.Range(0.6f, 1.3f);
                Vector2 velocity = new Vector2(
                    Mathf.Cos(angle) * fragSpeed * (Random.value > 0.5f ? 1f : -1f),
                    Mathf.Sin(angle) * fragSpeed
                );

                // Rastgele donme hizi
                float rotSpeed = Random.Range(-720f, 720f);

                StartCoroutine(AnimateFragment(frag, sr, velocity, rotSpeed, scale, lifetime));
            }

            // Tum parcalar bitene kadar bekle
            yield return new WaitForSeconds(lifetime + 0.1f);
            Destroy(gameObject);
        }

        private IEnumerator AnimateFragment(GameObject frag, SpriteRenderer sr,
            Vector2 velocity, float rotSpeed, float startScale, float lifetime)
        {
            float elapsed = 0f;
            Vector3 pos = frag.transform.position;
            float gravity = -12f;
            float rotation = Random.Range(0f, 360f);

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;

                // Hareket: hiz + yercekim
                velocity.y += gravity * Time.deltaTime;
                pos.x += velocity.x * Time.deltaTime;
                pos.y += velocity.y * Time.deltaTime;

                if (frag == null) yield break;

                frag.transform.position = pos;

                // Donme
                rotation += rotSpeed * Time.deltaTime;
                frag.transform.rotation = Quaternion.Euler(0, 0, rotation);

                // Kucul (ease-in: baslangicta yavas, sonda hizli)
                float scaleT = 1f - t * t;
                frag.transform.localScale = Vector3.one * startScale * scaleT;

                // Fade out (son %40'ta)
                if (t > 0.6f)
                {
                    float fadeT = (t - 0.6f) / 0.4f;
                    Color c = sr.color;
                    c.a = 1f - fadeT;
                    sr.color = c;
                }

                yield return null;
            }

            if (frag != null)
                Destroy(frag);
        }
    }
}
