using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using ComBoom.Core;
using ComBoom.Gameplay;

namespace ComBoom.UI
{
    public class SplashPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform titleRT;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI titleGlowText;
        [SerializeField] private TextMeshProUGUI taglineText;
        [SerializeField] private TextMeshProUGUI studioText;

        private struct SplashBlock
        {
            public RectTransform rt;
            public Image img;
            public Vector2 velocity;
            public float rotSpeed;
            public float baseScale;
        }

        private static readonly Color[] pieceColors = new Color[]
        {
            new Color(0.063f, 0.725f, 0.506f, 1f), // Yesil
            new Color(0.231f, 0.510f, 0.965f, 1f), // Mavi
            new Color(0.937f, 0.267f, 0.267f, 1f), // Kirmizi
            new Color(0.961f, 0.620f, 0.043f, 1f), // Turuncu
            new Color(0.925f, 0.282f, 0.600f, 1f), // Pembe
            new Color(0.545f, 0.361f, 0.965f, 1f), // Mor
        };

        private List<GameObject> dynamicObjects = new List<GameObject>();
        private Sprite blockSprite;
        private Sprite fragmentSprite;

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
            StartCoroutine(AnimateSplash());
        }

        public void Hide()
        {
            StopAllCoroutines();
            CleanupBlocks();
            if (panel != null) panel.SetActive(false);
        }

        private SplashBlock CreateBlock(float size, Color color, Transform parent)
        {
            GameObject obj = new GameObject("SplashBlock");
            obj.transform.SetParent(parent, false);
            dynamicObjects.Add(obj);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size);

            Image img = obj.AddComponent<Image>();
            if (blockSprite == null)
                blockSprite = SpriteGenerator.CreateBlockSprite();
            img.sprite = blockSprite;
            img.color = color;
            img.raycastTarget = false;

            return new SplashBlock { rt = rt, img = img };
        }

        private SplashBlock CreateFragment(float size, Color color, Transform parent)
        {
            GameObject obj = new GameObject("SplashFrag");
            obj.transform.SetParent(parent, false);
            dynamicObjects.Add(obj);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size);

            Image img = obj.AddComponent<Image>();
            if (fragmentSprite == null)
                fragmentSprite = SpriteGenerator.CreateBrickFragmentSprite();
            img.sprite = fragmentSprite;
            img.color = color;
            img.raycastTarget = false;

            return new SplashBlock { rt = rt, img = img };
        }

        private void CleanupBlocks()
        {
            for (int i = dynamicObjects.Count - 1; i >= 0; i--)
            {
                if (dynamicObjects[i] != null)
                    Destroy(dynamicObjects[i]);
            }
            dynamicObjects.Clear();
        }

        private IEnumerator AnimateSplash()
        {
            // Baslangic: her sey gorunmez
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (titleRT != null) titleRT.localScale = Vector3.one * 0.3f;
            if (titleText != null) SetAlpha(titleText, 0f);
            if (titleGlowText != null) SetAlpha(titleGlowText, 0f);
            if (taglineText != null) SetAlpha(taglineText, 0f);
            if (studioText != null) SetAlpha(studioText, 0f);

            Vector2 taglineOrigPos = taglineText != null
                ? taglineText.rectTransform.anchoredPosition : Vector2.zero;

            // Panel boyutunu al (referans cozunurluk: 1080x1920)
            RectTransform panelRT = panel.GetComponent<RectTransform>();
            float panelW = 1080f;
            float panelH = 1920f;
            if (panelRT != null)
            {
                panelW = panelRT.rect.width > 0 ? panelRT.rect.width : 1080f;
                panelH = panelRT.rect.height > 0 ? panelRT.rect.height : 1920f;
            }

            float centerY = 0f; // anchor'lu oldugu icin 0,0 merkez
            float centerX = 0f;

            // ================================================
            // BLOKLARI OLUSTUR (24 ana blok)
            // ================================================
            int blockCount = 24;
            SplashBlock[] blocks = new SplashBlock[blockCount];
            Vector2[] blockStartPos = new Vector2[blockCount];
            float[] blockFallSpeed = new float[blockCount];
            float[] blockTargetY = new float[blockCount];

            for (int i = 0; i < blockCount; i++)
            {
                float size = Random.Range(55f, 95f);
                Color col = pieceColors[i % pieceColors.Length];
                blocks[i] = CreateBlock(size, col, panel.transform);
                blocks[i].baseScale = size / 80f;
                blocks[i].rotSpeed = Random.Range(-200f, 200f);

                // Baslangic: ekranin ustunde, rastgele X
                float startX = Random.Range(-panelW * 0.4f, panelW * 0.4f);
                float startY = panelH * 0.5f + Random.Range(50f, 400f);
                blockStartPos[i] = new Vector2(startX, startY);
                blocks[i].rt.anchoredPosition = blockStartPos[i];
                blocks[i].rt.localScale = Vector3.one * blocks[i].baseScale;

                // Dusus hizi ve hedef Y
                blockFallSpeed[i] = Random.Range(900f, 1500f);
                blockTargetY[i] = Random.Range(-panelH * 0.05f, panelH * 0.15f);

                // Baslangicta gorunmez
                SetImageAlpha(blocks[i].img, 0f);
            }

            // ================================================
            // PHASE 1: Blok Yagmuru (0 - 0.8s)
            // ================================================
            float t = 0f;
            while (t < 0.8f)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / 0.8f);

                // Arka plan fade in
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Clamp01(t / 0.2f);

                for (int i = 0; i < blockCount; i++)
                {
                    if (blocks[i].rt == null) continue;

                    // Bloklar sirayla gorunur olur (stagger)
                    float stagger = (float)i / blockCount * 0.3f;
                    float blockT = Mathf.Clamp01((t - stagger) / (0.8f - stagger));

                    if (blockT <= 0f)
                    {
                        SetImageAlpha(blocks[i].img, 0f);
                        continue;
                    }

                    SetImageAlpha(blocks[i].img, Mathf.Clamp01(blockT * 3f));

                    // Dusus: hizli basla, hedefe yaklasinca yavasla
                    float fallP = EaseOutQuad(blockT);
                    float newY = Mathf.Lerp(blockStartPos[i].y, blockTargetY[i], fallP);
                    float newX = blockStartPos[i].x;
                    blocks[i].rt.anchoredPosition = new Vector2(newX, newY);

                    // Rotasyon
                    float rot = blocks[i].rotSpeed * t;
                    blocks[i].rt.localRotation = Quaternion.Euler(0, 0, rot);
                }

                yield return null;
            }

            // Phase 1 sonu: tum bloklar gorulur, hedef pozisyonlarinda
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            for (int i = 0; i < blockCount; i++)
            {
                if (blocks[i].rt == null) continue;
                blocks[i].rt.anchoredPosition = new Vector2(blockStartPos[i].x, blockTargetY[i]);
                SetImageAlpha(blocks[i].img, 1f);
            }

            // Mevcut pozisyonlari kaydet (Phase 2 icin)
            Vector2[] phase2Start = new Vector2[blockCount];
            for (int i = 0; i < blockCount; i++)
            {
                if (blocks[i].rt != null)
                    phase2Start[i] = blocks[i].rt.anchoredPosition;
            }

            // ================================================
            // PHASE 2: Merkeze Toplanma (0.6s)
            // ================================================
            t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / 0.6f);
                float eased = EaseInQuad(p);

                for (int i = 0; i < blockCount; i++)
                {
                    if (blocks[i].rt == null) continue;

                    // Merkeze dogru cek
                    Vector2 target = Vector2.zero;
                    // Hafif random offset (tam ust uste binmesin)
                    target += new Vector2(
                        Mathf.Sin(i * 1.3f) * 20f,
                        Mathf.Cos(i * 1.7f) * 20f);

                    blocks[i].rt.anchoredPosition = Vector2.Lerp(
                        phase2Start[i], target, eased);

                    // Kucul
                    float scale = Mathf.Lerp(blocks[i].baseScale, 0.35f, eased);
                    blocks[i].rt.localScale = Vector3.one * scale;

                    // Rotasyon hizlan
                    float rot = blocks[i].rotSpeed * (1f + p * 2f) * (0.8f + t);
                    blocks[i].rt.localRotation = Quaternion.Euler(0, 0, rot);
                }

                yield return null;
            }

            // ================================================
            // PHASE 3: Patlama + Baslik (0.7s)
            // ================================================

            // Patlama yonlerini hesapla
            Vector2[] explodeDir = new Vector2[blockCount];
            float[] explodeSpeed = new float[blockCount];
            for (int i = 0; i < blockCount; i++)
            {
                float angle = (360f / blockCount) * i + Random.Range(-15f, 15f);
                float rad = angle * Mathf.Deg2Rad;
                explodeDir[i] = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                explodeSpeed[i] = Random.Range(1200f, 2000f);
            }

            // Fragment parcaciklari olustur
            int fragCount = 16;
            SplashBlock[] frags = new SplashBlock[fragCount];
            Vector2[] fragDir = new Vector2[fragCount];
            float[] fragSpeed = new float[fragCount];
            for (int i = 0; i < fragCount; i++)
            {
                float size = Random.Range(18f, 32f);
                Color col = pieceColors[Random.Range(0, pieceColors.Length)];
                frags[i] = CreateFragment(size, col, panel.transform);
                frags[i].rt.anchoredPosition = Vector2.zero;
                frags[i].rt.localScale = Vector3.one * 0.8f;
                frags[i].rotSpeed = Random.Range(-600f, 600f);

                float angle = Random.Range(0f, 360f);
                float rad = angle * Mathf.Deg2Rad;
                fragDir[i] = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                fragSpeed[i] = Random.Range(800f, 1600f);
            }

            t = 0f;
            while (t < 0.7f)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / 0.7f);

                // Bloklar disa patlayarak kaybolur
                for (int i = 0; i < blockCount; i++)
                {
                    if (blocks[i].rt == null) continue;

                    Vector2 pos = explodeDir[i] * explodeSpeed[i] * p;
                    blocks[i].rt.anchoredPosition = pos;

                    float scale = Mathf.Lerp(0.35f, 0.05f, p);
                    blocks[i].rt.localScale = Vector3.one * scale;

                    SetImageAlpha(blocks[i].img, 1f - EaseInQuad(p));

                    float rot = blocks[i].rotSpeed * 3f * t;
                    blocks[i].rt.localRotation = Quaternion.Euler(0, 0, rot);
                }

                // Fragment parcaciklari
                for (int i = 0; i < fragCount; i++)
                {
                    if (frags[i].rt == null) continue;

                    Vector2 pos = fragDir[i] * fragSpeed[i] * p;
                    frags[i].rt.anchoredPosition = pos;

                    float scale = Mathf.Lerp(0.8f, 0.1f, p);
                    frags[i].rt.localScale = Vector3.one * scale;

                    SetImageAlpha(frags[i].img, 1f - p);

                    frags[i].rt.localRotation = Quaternion.Euler(
                        0, 0, frags[i].rotSpeed * t);
                }

                // Baslik ayni anda belirir
                float titleP = Mathf.Clamp01(t / 0.5f);
                if (titleRT != null)
                    titleRT.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.15f, EaseOutBack(titleP));
                if (titleText != null)
                    SetAlpha(titleText, titleP);
                if (titleGlowText != null)
                    SetAlpha(titleGlowText, Mathf.Lerp(0f, 0.55f, titleP));

                yield return null;
            }

            // Phase 3 sonu: bloklari temizle, baslik tam gorulur
            for (int i = 0; i < blockCount; i++)
            {
                if (blocks[i].rt != null)
                    blocks[i].rt.gameObject.SetActive(false);
            }
            for (int i = 0; i < fragCount; i++)
            {
                if (frags[i].rt != null)
                    frags[i].rt.gameObject.SetActive(false);
            }

            if (titleText != null) SetAlpha(titleText, 1f);
            if (titleGlowText != null) SetAlpha(titleGlowText, 0.55f);

            // ================================================
            // PHASE 4: Yerlesme (0.8s)
            // ================================================

            // Floating bloklar olustur (6 adet, kenarlarda)
            int floatCount = 6;
            SplashBlock[] floaters = new SplashBlock[floatCount];
            Vector2[] floatBasePos = new Vector2[floatCount];
            float[] floatPhase = new float[floatCount];

            for (int i = 0; i < floatCount; i++)
            {
                float size = Random.Range(35f, 55f);
                Color col = pieceColors[i % pieceColors.Length];
                floaters[i] = CreateBlock(size, col, panel.transform);
                floaters[i].baseScale = size / 80f;

                // Kenarlara yerlestir
                float side = (i % 2 == 0) ? -1f : 1f;
                float fx = side * panelW * Random.Range(0.30f, 0.42f);
                float fy = Random.Range(-panelH * 0.15f, panelH * 0.25f);
                floatBasePos[i] = new Vector2(fx, fy);
                floaters[i].rt.anchoredPosition = floatBasePos[i];
                floaters[i].rt.localScale = Vector3.one * floaters[i].baseScale;
                floaters[i].rt.localRotation = Quaternion.Euler(0, 0, Random.Range(-25f, 25f));
                floatPhase[i] = Random.Range(0f, Mathf.PI * 2f);

                SetImageAlpha(floaters[i].img, 0f);
            }

            t = 0f;
            while (t < 0.8f)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / 0.8f);

                // Baslik settle: 1.15 → 1.0
                if (titleRT != null)
                    titleRT.localScale = Vector3.one * Mathf.Lerp(1.15f, 1.0f, EaseOutQuad(p));

                // Tagline fade in + yukari kayma
                if (taglineText != null)
                {
                    float tagP = Mathf.Clamp01((t - 0.1f) / 0.4f);
                    SetAlpha(taglineText, tagP);
                    taglineText.rectTransform.anchoredPosition = new Vector2(
                        taglineOrigPos.x,
                        taglineOrigPos.y + Mathf.Lerp(-15f, 0f, EaseOutQuad(tagP)));
                }

                // Studio fade in
                if (studioText != null)
                {
                    float stP = Mathf.Clamp01((t - 0.3f) / 0.4f);
                    SetAlpha(studioText, stP * 0.5f);
                }

                // Floating bloklar fade in + salinim
                for (int i = 0; i < floatCount; i++)
                {
                    if (floaters[i].rt == null) continue;
                    float fP = Mathf.Clamp01((t - 0.1f) / 0.5f);
                    SetImageAlpha(floaters[i].img, fP * 0.6f);

                    float bobY = Mathf.Sin((t + floatPhase[i]) * 2.5f) * 8f;
                    floaters[i].rt.anchoredPosition = floatBasePos[i] + Vector2.up * bobY;
                }

                yield return null;
            }

            // Tagline sabitle
            if (taglineText != null)
                taglineText.rectTransform.anchoredPosition = taglineOrigPos;

            // ================================================
            // PHASE 5: Fade Out (0.6s)
            // ================================================
            float titleStartScale = titleRT != null ? titleRT.localScale.x : 1f;
            t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / 0.6f);

                // CanvasGroup fade out
                if (canvasGroup != null)
                    canvasGroup.alpha = 1f - p;

                // Baslik kucul
                if (titleRT != null)
                    titleRT.localScale = Vector3.one * Mathf.Lerp(titleStartScale, 0.9f, EaseOutQuad(p));

                // Floating bloklar disa kayar
                for (int i = 0; i < floatCount; i++)
                {
                    if (floaters[i].rt == null) continue;
                    float side = (i % 2 == 0) ? -1f : 1f;
                    float slideX = side * 60f * p;
                    float bobY = Mathf.Sin((t + floatPhase[i]) * 2.5f) * 8f;
                    floaters[i].rt.anchoredPosition =
                        floatBasePos[i] + new Vector2(slideX, bobY);
                }

                yield return null;
            }

            // Bitir
            CleanupBlocks();
            Hide();
            if (GameManager.Instance != null)
                GameManager.Instance.ShowMainMenu();
        }

        // ================================================
        // EASING + HELPERS
        // ================================================
        private static float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private static float EaseInQuad(float t)
        {
            return t * t;
        }

        private static void SetAlpha(TextMeshProUGUI text, float alpha)
        {
            if (text == null) return;
            Color c = text.color;
            c.a = alpha;
            text.color = c;
        }

        private static void SetImageAlpha(Image img, float alpha)
        {
            if (img == null) return;
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
}
