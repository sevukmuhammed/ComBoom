using UnityEngine;

namespace ComBoom.Gameplay
{
    public static class SpriteGenerator
    {
        private static Sprite cachedBlockSprite;
        private static Sprite cachedCellSprite;
        private static Sprite cachedSlotSprite;
        private static Sprite cachedRoundedUISprite;
        private static Sprite cachedGridBgSprite;
        private static Sprite cachedFragmentSprite;
        private static Sprite cachedUndoIcon;
        private static Sprite cachedBombIcon;
        private static Sprite cachedShuffleIcon;
        private static Sprite cachedSoundIcon;
        private static Sprite cachedMusicIcon;
        private static Sprite cachedVibeIcon;
        private static Sprite cachedRefreshIcon;
        private static Sprite cachedHomeIcon;
        private static Sprite cachedPlayArrow;
        private static Sprite cachedLeaderboard;
        private static Sprite cachedSettingsGear;
        private static Sprite cachedStar;
        private static Sprite cachedGlowOrb;
        private static Sprite cachedMenuBg;
        private static Sprite cachedCircleBadge;
        private static Sprite cachedTrophyIcon;
        private static Sprite cachedBackArrow;
        private static Sprite cachedAvatarPlaceholder;
        private static Sprite cachedShareIcon;
        private static Sprite cachedGlobeIcon;
        private static Sprite cachedDocumentIcon;
        private static Sprite cachedMailIcon;
        private static Sprite cachedChevronRight;
        private static Sprite cachedExternalLink;
        private static Sprite cachedTogglePill;
        private static Sprite cachedSplashGrid;
        private static Sprite cachedSplashBurst;

        public static void ClearAllCaches()
        {
            cachedBlockSprite = null;
            cachedCellSprite = null;
            cachedSlotSprite = null;
            cachedRoundedUISprite = null;
            cachedGridBgSprite = null;
            cachedFragmentSprite = null;
            cachedUndoIcon = null;
            cachedBombIcon = null;
            cachedShuffleIcon = null;
            cachedSoundIcon = null;
            cachedMusicIcon = null;
            cachedVibeIcon = null;
            cachedRefreshIcon = null;
            cachedHomeIcon = null;
            cachedPlayArrow = null;
            cachedLeaderboard = null;
            cachedSettingsGear = null;
            cachedStar = null;
            cachedGlowOrb = null;
            cachedMenuBg = null;
            cachedCircleBadge = null;
            cachedTrophyIcon = null;
            cachedBackArrow = null;
            cachedAvatarPlaceholder = null;
            cachedShareIcon = null;
            cachedGlobeIcon = null;
            cachedDocumentIcon = null;
            cachedMailIcon = null;
            cachedChevronRight = null;
            cachedExternalLink = null;
            cachedTogglePill = null;
            cachedSplashGrid = null;
            cachedSplashBurst = null;
        }

        // ============================================================
        // BLOCK SPRITE - Parlak cam efektli jewel blok
        // ============================================================
        public static Sprite CreateBlockSprite()
        {
            if (cachedBlockSprite != null) return cachedBlockSprite;

            int size = 128;
            int radius = 16;
            int borderWidth = 2;
            float glossyZone = 0.40f; // Ust %40 parlak cam efekti
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float alpha = GetRoundedRectAlpha(x, y, size, radius);
                    if (alpha <= 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float distLeft = x;
                    float distRight = size - 1 - x;
                    float distBottom = y;
                    float distTop = size - 1 - y;
                    float minEdgeDist = Mathf.Min(distLeft, distRight, distBottom, distTop);

                    float brightness;

                    // 1. Ince beyaz kenar (rgba(255,255,255,0.1) efekti)
                    if (minEdgeDist < borderWidth)
                    {
                        float borderT = minEdgeDist / borderWidth;
                        brightness = Mathf.Lerp(1.12f, 1.0f, borderT);
                    }
                    else
                    {
                        // 2. Alt-ust derinlik gradienti
                        float normalizedY = (float)(size - 1 - y) / (float)(size - 1);
                        brightness = Mathf.Lerp(0.82f, 0.95f, normalizedY);

                        // 3. Ust %40 parlak cam highlight
                        if (normalizedY > (1f - glossyZone))
                        {
                            float glossT = (normalizedY - (1f - glossyZone)) / glossyZone;
                            float glossAmount = glossT * glossT * 0.30f;
                            brightness += glossAmount;
                        }

                        // 4. Merkez radial ic parlama
                        float cx = (x - size * 0.5f) / (size * 0.5f);
                        float cy = (y - size * 0.5f) / (size * 0.5f);
                        float radialDist = Mathf.Sqrt(cx * cx + cy * cy);
                        float innerGlow = Mathf.Lerp(1.06f, 1.0f, Mathf.Clamp01(radialDist));
                        brightness *= innerGlow;
                    }

                    brightness = Mathf.Clamp(brightness, 0f, 1.25f);
                    float v = Mathf.Clamp01(brightness);
                    tex.SetPixel(x, y, new Color(v, v, v, alpha));
                }
            }

            tex.Apply();
            cachedBlockSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedBlockSprite.name = "BlockSprite";
            return cachedBlockSprite;
        }

        // ============================================================
        // CELL SPRITE - Hafif gomuk, neredeyse gorunmez grid cizgisi
        // ============================================================
        public static Sprite CreateCellSprite()
        {
            if (cachedCellSprite != null) return cachedCellSprite;

            int size = 128;
            int radius = 4;
            int bevel = 3;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float alpha = GetRoundedRectAlpha(x, y, size, radius);
                    if (alpha <= 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float distTop = size - 1 - y;
                    float distBottom = y;
                    float distLeft = x;
                    float distRight = size - 1 - x;
                    float minEdge = Mathf.Min(distLeft, distRight, distBottom, distTop);

                    float brightness = 0.94f;

                    // Cok hafif gomuk bevel
                    if (distTop < bevel)
                    {
                        float t = distTop / bevel;
                        float s = t * t * (3f - 2f * t);
                        brightness = Mathf.Lerp(0.75f, 0.94f, s);
                    }
                    else if (distBottom < bevel)
                    {
                        float t = distBottom / bevel;
                        float s = t * t * (3f - 2f * t);
                        brightness = Mathf.Lerp(1.05f, 0.94f, s);
                    }
                    else if (distLeft < bevel)
                    {
                        float t = distLeft / bevel;
                        float s = t * t * (3f - 2f * t);
                        brightness = Mathf.Lerp(0.78f, 0.94f, s);
                    }
                    else if (distRight < bevel)
                    {
                        float t = distRight / bevel;
                        float s = t * t * (3f - 2f * t);
                        brightness = Mathf.Lerp(1.02f, 0.94f, s);
                    }

                    brightness = Mathf.Clamp01(brightness);
                    tex.SetPixel(x, y, new Color(brightness, brightness, brightness, alpha));
                }
            }

            tex.Apply();
            cachedCellSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedCellSprite.name = "CellSprite";
            return cachedCellSprite;
        }

        // ============================================================
        // SLOT SPRITE - Yuvarlatilmis panel, ince kenar
        // ============================================================
        public static Sprite CreateSlotSprite()
        {
            if (cachedSlotSprite != null) return cachedSlotSprite;

            int size = 128;
            int radius = 20;
            int bevel = 3;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float alpha = GetRoundedRectAlpha(x, y, size, radius);
                    if (alpha <= 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float distTop = size - 1 - y;
                    float distBottom = y;
                    float distLeft = x;
                    float distRight = size - 1 - x;
                    float minEdge = Mathf.Min(distLeft, distRight, distBottom, distTop);

                    float brightness = 0.92f;

                    // Ince kenar cizgisi (1-2px)
                    if (minEdge < 2)
                    {
                        brightness = 1.1f;
                    }
                    else if (distTop < bevel + 2)
                    {
                        float t = (distTop - 2) / bevel;
                        brightness = Mathf.Lerp(0.80f, 0.92f, Mathf.Clamp01(t));
                    }
                    else if (distBottom < bevel + 2)
                    {
                        float t = (distBottom - 2) / bevel;
                        brightness = Mathf.Lerp(1.0f, 0.92f, Mathf.Clamp01(t));
                    }

                    brightness = Mathf.Clamp01(brightness);
                    tex.SetPixel(x, y, new Color(brightness, brightness, brightness, alpha));
                }
            }

            tex.Apply();
            cachedSlotSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedSlotSprite.name = "SlotSprite";
            return cachedSlotSprite;
        }

        // ============================================================
        // GRID BACKGROUND SPRITE - Temiz koyu panel
        // ============================================================
        public static Sprite CreateGridBgSprite()
        {
            if (cachedGridBgSprite != null) return cachedGridBgSprite;

            int size = 128;
            int radius = 18;
            int bevel = 5;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float alpha = GetRoundedRectAlpha(x, y, size, radius);
                    if (alpha <= 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float distTop = size - 1 - y;
                    float distBottom = y;
                    float distLeft = x;
                    float distRight = size - 1 - x;
                    float minEdge = Mathf.Min(distLeft, distRight, distBottom, distTop);

                    float brightness = 0.92f;

                    // Hafif gomuk bevel
                    if (distTop < bevel)
                    {
                        float t = distTop / bevel;
                        float s = t * t * (3f - 2f * t);
                        brightness = Mathf.Lerp(0.70f, 0.92f, s);
                    }
                    else if (distBottom < bevel)
                    {
                        float t = distBottom / bevel;
                        float s = t * t * (3f - 2f * t);
                        brightness = Mathf.Lerp(1.05f, 0.92f, s);
                    }
                    else if (distLeft < bevel)
                    {
                        float t = distLeft / bevel;
                        float s = t * t * (3f - 2f * t);
                        brightness = Mathf.Lerp(0.72f, 0.92f, s);
                    }
                    else if (distRight < bevel)
                    {
                        float t = distRight / bevel;
                        float s = t * t * (3f - 2f * t);
                        brightness = Mathf.Lerp(1.03f, 0.92f, s);
                    }

                    brightness = Mathf.Clamp01(brightness);
                    tex.SetPixel(x, y, new Color(brightness, brightness, brightness, alpha));
                }
            }

            tex.Apply();
            cachedGridBgSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedGridBgSprite.name = "GridBackgroundSprite";
            return cachedGridBgSprite;
        }

        // ============================================================
        // ROUNDED UI SPRITE - Butonlar icin 9-slice
        // ============================================================
        public static Sprite CreateRoundedUISprite()
        {
            if (cachedRoundedUISprite != null) return cachedRoundedUISprite;

            int size = 64;
            int radius = 16;
            int bevel = 3;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float alpha = GetRoundedRectAlpha(x, y, size, radius);
                    if (alpha <= 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float distTop = size - 1 - y;
                    float distBottom = y;
                    float brightness = 1f;

                    if (distTop < bevel)
                    {
                        float t = distTop / bevel;
                        brightness = Mathf.Lerp(1.1f, 1f, t);
                    }
                    else if (distBottom < bevel)
                    {
                        float t = distBottom / bevel;
                        brightness = Mathf.Lerp(0.75f, 0.95f, t);
                    }

                    brightness = Mathf.Clamp01(brightness);
                    tex.SetPixel(x, y, new Color(brightness, brightness, brightness, alpha));
                }
            }

            tex.Apply();
            Vector4 sliceBorder = new Vector4(radius + 1, radius + 1, radius + 1, radius + 1);
            cachedRoundedUISprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect, sliceBorder);
            cachedRoundedUISprite.name = "RoundedUISprite";
            return cachedRoundedUISprite;
        }

        // ============================================================
        // FRAGMENT SPRITE - Kirilma efekti parcasi
        // ============================================================
        public static Sprite CreateBrickFragmentSprite()
        {
            if (cachedFragmentSprite != null) return cachedFragmentSprite;

            int size = 16;
            int radius = 3;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float alpha = GetRoundedRectAlpha(x, y, size, radius);
                    if (alpha <= 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float distTop = size - 1 - y;
                    float distBottom = y;

                    // Hafif bevel
                    float brightness = 0.95f;
                    if (distTop < 2) brightness *= Mathf.Lerp(1.12f, 1f, distTop / 2f);
                    if (distBottom < 2) brightness *= Mathf.Lerp(0.75f, 1f, distBottom / 2f);

                    brightness = Mathf.Clamp01(brightness);
                    tex.SetPixel(x, y, new Color(brightness, brightness, brightness, alpha));
                }
            }

            tex.Apply();
            cachedFragmentSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedFragmentSprite.name = "FragmentSprite";
            return cachedFragmentSprite;
        }

        // ============================================================
        // UNDO ICON - Sola donuk ok
        // ============================================================
        public static Sprite CreateUndoIconSprite()
        {
            if (cachedUndoIcon != null) return cachedUndoIcon;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color white = Color.white;
            Color clear = Color.clear;

            // Temiz arka plan
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, clear);

            // Dairesel ok: sola donuk ok + yarim daire
            float cx = 34f, cy = 32f, r = 18f;
            float thickness = 4.5f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    // Yarim daire yay (sag yarim, 270->90 derece, yani ust yarim)
                    float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (angle < 0) angle += 360f;

                    bool onArc = false;
                    // Yay: -30 ile 210 derece arasi (sola acik yarim daire)
                    if (angle >= 330f || angle <= 210f)
                    {
                        float arcDist = Mathf.Abs(dist - r);
                        if (arcDist < thickness)
                        {
                            float aa = 1f - Mathf.Clamp01(arcDist - thickness + 1.2f);
                            if (aa > 0f)
                            {
                                Color existing = tex.GetPixel(x, y);
                                float newAlpha = Mathf.Max(existing.a, aa);
                                tex.SetPixel(x, y, new Color(1f, 1f, 1f, newAlpha));
                                onArc = true;
                            }
                        }
                    }

                    // Ok ucu (sol tarafta, yay baslangici)
                    if (!onArc)
                    {
                        // Ok ucu ucgen: nokta (12, 32), ustten ve alttan cizgiler
                        float arrowTipX = 12f;
                        float arrowTipY = 32f;
                        float arrowBaseX = 24f;
                        float arrowHalf = 10f;

                        if (x >= arrowTipX - 1 && x <= arrowBaseX + 1)
                        {
                            float t = (x - arrowTipX) / (arrowBaseX - arrowTipX);
                            float halfWidth = arrowHalf * t;
                            float distFromCenter = Mathf.Abs(y - arrowTipY);

                            if (distFromCenter <= halfWidth + 1.5f)
                            {
                                float aa = 1f - Mathf.Clamp01(distFromCenter - halfWidth);
                                if (aa > 0f)
                                {
                                    Color existing = tex.GetPixel(x, y);
                                    float newAlpha = Mathf.Max(existing.a, aa);
                                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, newAlpha));
                                }
                            }
                        }
                    }
                }
            }

            tex.Apply();
            cachedUndoIcon = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedUndoIcon.name = "UndoIcon";
            return cachedUndoIcon;
        }

        // ============================================================
        // BOMB ICON - Patlama yildizi
        // ============================================================
        public static Sprite CreateBombIconSprite()
        {
            if (cachedBombIcon != null) return cachedBombIcon;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float cx = 32f, cy = 32f;
            float outerR = 24f;
            float innerR = 12f;
            int points = 8;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, Color.clear);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);

                    // Yildiz seklinde mesafe hesapla
                    float segmentAngle = Mathf.PI * 2f / points;
                    float halfSegment = segmentAngle / 2f;
                    float localAngle = Mathf.Repeat(angle + Mathf.PI * 2f, segmentAngle);
                    float t = Mathf.Abs(localAngle - halfSegment) / halfSegment;
                    float starR = Mathf.Lerp(outerR, innerR, t);

                    // Anti-aliased kenar
                    if (dist <= starR + 1.2f)
                    {
                        float aa = 1f - Mathf.Clamp01(dist - starR);
                        // Merkez parlama
                        float glow = 1f;
                        if (dist < innerR * 0.6f)
                            glow = Mathf.Lerp(1.2f, 1f, dist / (innerR * 0.6f));

                        float v = Mathf.Clamp01(glow);
                        tex.SetPixel(x, y, new Color(v, v, v, aa));
                    }
                }
            }

            tex.Apply();
            cachedBombIcon = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedBombIcon.name = "BombIcon";
            return cachedBombIcon;
        }

        // ============================================================
        // SHUFFLE ICON - Cift dairesel ok (refresh)
        // ============================================================
        public static Sprite CreateShuffleIconSprite()
        {
            if (cachedShuffleIcon != null) return cachedShuffleIcon;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float cx = 32f, cy = 32f, r = 18f;
            float thickness = 4.5f;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, Color.clear);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float arcDist = Mathf.Abs(dist - r);

                    float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (angle < 0) angle += 360f;

                    // Ust yarim yay (0-180)
                    bool onUpperArc = angle >= 10f && angle <= 170f && arcDist < thickness;
                    // Alt yarim yay (190-350)
                    bool onLowerArc = angle >= 190f && angle <= 350f && arcDist < thickness;

                    if (onUpperArc || onLowerArc)
                    {
                        float aa = 1f - Mathf.Clamp01(arcDist - thickness + 1.2f);
                        if (aa > 0f)
                        {
                            Color existing = tex.GetPixel(x, y);
                            float newAlpha = Mathf.Max(existing.a, aa);
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, newAlpha));
                        }
                    }

                    // Ok ucu 1: ust yay sag ucu (saat yonunde ok)
                    {
                        float tipX = cx + r * Mathf.Cos(10f * Mathf.Deg2Rad);
                        float tipY = cy + r * Mathf.Sin(10f * Mathf.Deg2Rad);
                        float adx = x - tipX;
                        float ady = y - tipY;
                        // Kucuk ucgen ok ucu (saga donuk)
                        float arrowLen = 9f;
                        float arrowHalf = 6f;
                        float projX = adx;
                        float projY = -ady; // asagi isaret ediyor
                        if (projY >= -1 && projY <= arrowLen + 1)
                        {
                            float tA = Mathf.Clamp01(projY / arrowLen);
                            float halfW = arrowHalf * (1f - tA);
                            float distC = Mathf.Abs(projX);
                            if (distC <= halfW + 1.2f)
                            {
                                float aa2 = 1f - Mathf.Clamp01(distC - halfW);
                                Color existing = tex.GetPixel(x, y);
                                float newAlpha = Mathf.Max(existing.a, aa2);
                                tex.SetPixel(x, y, new Color(1f, 1f, 1f, newAlpha));
                            }
                        }
                    }

                    // Ok ucu 2: alt yay sol ucu (saat yonunde ok)
                    {
                        float tipX = cx + r * Mathf.Cos(190f * Mathf.Deg2Rad);
                        float tipY = cy + r * Mathf.Sin(190f * Mathf.Deg2Rad);
                        float adx = x - tipX;
                        float ady = y - tipY;
                        // Yukari isaret eden ok ucu
                        float arrowLen = 9f;
                        float arrowHalf = 6f;
                        float projX = adx;
                        float projY = ady;
                        if (projY >= -1 && projY <= arrowLen + 1)
                        {
                            float tA = Mathf.Clamp01(projY / arrowLen);
                            float halfW = arrowHalf * (1f - tA);
                            float distC = Mathf.Abs(projX);
                            if (distC <= halfW + 1.2f)
                            {
                                float aa2 = 1f - Mathf.Clamp01(distC - halfW);
                                Color existing = tex.GetPixel(x, y);
                                float newAlpha = Mathf.Max(existing.a, aa2);
                                tex.SetPixel(x, y, new Color(1f, 1f, 1f, newAlpha));
                            }
                        }
                    }
                }
            }

            tex.Apply();
            cachedShuffleIcon = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedShuffleIcon.name = "ShuffleIcon";
            return cachedShuffleIcon;
        }

        // ============================================================
        // SOUND ICON - Hoparlor + ses dalgalari
        // ============================================================
        public static Sprite CreateSoundIconSprite()
        {
            if (cachedSoundIcon != null) return cachedSoundIcon;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, Color.clear);

            float cy = 32f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float alpha = 0f;

                    // Speaker body (small rectangle)
                    if (x >= 10 && x <= 18 && y >= 26 && y <= 38)
                    {
                        alpha = 1f;
                    }

                    // Speaker cone (expanding trapezoid)
                    if (x >= 18 && x <= 30)
                    {
                        float t = (x - 18f) / 12f;
                        float topY = Mathf.Lerp(26f, 18f, t);
                        float botY = Mathf.Lerp(38f, 46f, t);

                        if (y >= topY - 0.5f && y <= botY + 0.5f)
                        {
                            float edgeDistTop = y - topY;
                            float edgeDistBot = botY - y;
                            float edgeDist = Mathf.Min(edgeDistTop, edgeDistBot);
                            alpha = Mathf.Max(alpha, Mathf.Clamp01(edgeDist + 0.5f));
                        }
                    }

                    // Sound wave 1 (inner arc)
                    {
                        float arcCx = 22f;
                        float dx = x - arcCx;
                        float dy = y - cy;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        float r = 18f;
                        float thick = 3.5f;
                        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

                        if (angle > -50f && angle < 50f)
                        {
                            float arcDist = Mathf.Abs(dist - r);
                            if (arcDist < thick)
                            {
                                float aa = 1f - Mathf.Clamp01(arcDist - thick + 1.2f);
                                alpha = Mathf.Max(alpha, aa);
                            }
                        }
                    }

                    // Sound wave 2 (outer arc)
                    {
                        float arcCx = 22f;
                        float dx = x - arcCx;
                        float dy = y - cy;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        float r = 26f;
                        float thick = 3.5f;
                        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

                        if (angle > -40f && angle < 40f)
                        {
                            float arcDist = Mathf.Abs(dist - r);
                            if (arcDist < thick)
                            {
                                float aa = 1f - Mathf.Clamp01(arcDist - thick + 1.2f);
                                alpha = Mathf.Max(alpha, aa);
                            }
                        }
                    }

                    if (alpha > 0f)
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            cachedSoundIcon = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedSoundIcon.name = "SoundIcon";
            return cachedSoundIcon;
        }

        // ============================================================
        // MUSIC ICON - Muzik notasi (sekizlik nota)
        // ============================================================
        public static Sprite CreateMusicIconSprite()
        {
            if (cachedMusicIcon != null) return cachedMusicIcon;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, Color.clear);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float alpha = 0f;

                    // Note head (filled ellipse at bottom-left)
                    {
                        float ecx = 24f, ecy = 18f;
                        float rx = 9f, ry = 7f;
                        float angle = -25f * Mathf.Deg2Rad;
                        float cosA = Mathf.Cos(angle), sinA = Mathf.Sin(angle);
                        float dx = x - ecx, dy = y - ecy;
                        float lx = dx * cosA + dy * sinA;
                        float ly = -dx * sinA + dy * cosA;
                        float ellipseDist = (lx * lx) / (rx * rx) + (ly * ly) / (ry * ry);

                        if (ellipseDist <= 1.15f)
                        {
                            alpha = Mathf.Max(alpha, 1f - Mathf.Clamp01(ellipseDist - 1f) * 6f);
                        }
                    }

                    // Stem (vertical line from note head to top)
                    {
                        float stemX = 31f;
                        float stemBot = 18f, stemTop = 52f;
                        float thick = 3.5f;
                        float distX = Mathf.Abs(x - stemX);

                        if (y >= stemBot && y <= stemTop && distX < thick)
                        {
                            float aa = 1f - Mathf.Clamp01(distX - thick + 1.2f);
                            alpha = Mathf.Max(alpha, aa);
                        }
                    }

                    // Flag (curved line at top)
                    {
                        float flagStartY = 52f;
                        float flagEndY = 38f;
                        float flagStartX = 31f;
                        float flagEndX = 42f;

                        if (y >= flagEndY - 2 && y <= flagStartY + 2)
                        {
                            float t = (flagStartY - y) / (flagStartY - flagEndY);
                            t = Mathf.Clamp01(t);
                            float curveX = flagStartX + (flagEndX - flagStartX) * Mathf.Sin(t * Mathf.PI * 0.5f);
                            float distX = Mathf.Abs(x - curveX);
                            float thick = 3.5f;

                            if (distX < thick)
                            {
                                float aa = 1f - Mathf.Clamp01(distX - thick + 1.2f);
                                alpha = Mathf.Max(alpha, aa);
                            }
                        }
                    }

                    if (alpha > 0f)
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            cachedMusicIcon = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedMusicIcon.name = "MusicIcon";
            return cachedMusicIcon;
        }

        // ============================================================
        // VIBE ICON - Telefon + titresim cizgileri
        // ============================================================
        public static Sprite CreateVibeIconSprite()
        {
            if (cachedVibeIcon != null) return cachedVibeIcon;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, Color.clear);

            float cx = 32f, cy = 32f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float alpha = 0f;

                    // Phone body (rounded rectangle)
                    float phoneL = 22f, phoneR = 42f, phoneB = 10f, phoneT = 54f;
                    float phoneRadius = 5f;

                    if (x >= phoneL && x <= phoneR && y >= phoneB && y <= phoneT)
                    {
                        float cornerAlpha = 1f;
                        // Check corners
                        if (x < phoneL + phoneRadius && y < phoneB + phoneRadius)
                        {
                            float d = Vector2.Distance(new Vector2(x, y), new Vector2(phoneL + phoneRadius, phoneB + phoneRadius));
                            cornerAlpha = 1f - Mathf.Clamp01(d - phoneRadius + 0.7f);
                        }
                        else if (x > phoneR - phoneRadius && y < phoneB + phoneRadius)
                        {
                            float d = Vector2.Distance(new Vector2(x, y), new Vector2(phoneR - phoneRadius, phoneB + phoneRadius));
                            cornerAlpha = 1f - Mathf.Clamp01(d - phoneRadius + 0.7f);
                        }
                        else if (x < phoneL + phoneRadius && y > phoneT - phoneRadius)
                        {
                            float d = Vector2.Distance(new Vector2(x, y), new Vector2(phoneL + phoneRadius, phoneT - phoneRadius));
                            cornerAlpha = 1f - Mathf.Clamp01(d - phoneRadius + 0.7f);
                        }
                        else if (x > phoneR - phoneRadius && y > phoneT - phoneRadius)
                        {
                            float d = Vector2.Distance(new Vector2(x, y), new Vector2(phoneR - phoneRadius, phoneT - phoneRadius));
                            cornerAlpha = 1f - Mathf.Clamp01(d - phoneRadius + 0.7f);
                        }

                        // Hollow phone (border only)
                        float edgeL = x - phoneL;
                        float edgeR = phoneR - x;
                        float edgeB = y - phoneB;
                        float edgeT = phoneT - y;
                        float minEdge = Mathf.Min(edgeL, edgeR, edgeB, edgeT);
                        float borderW = 3f;

                        if (minEdge < borderW)
                        {
                            alpha = Mathf.Max(alpha, cornerAlpha);
                        }

                        // Screen circle (small dot at top of phone)
                        float screenDist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, phoneT - 6f));
                        if (screenDist < 2f)
                        {
                            float aa = 1f - Mathf.Clamp01(screenDist - 1.5f);
                            alpha = Mathf.Max(alpha, aa);
                        }
                    }

                    // Vibration lines left
                    {
                        float lineX = 14f;
                        float thick = 2.5f;
                        float distX = Mathf.Abs(x - lineX);
                        if (distX < thick && y >= 24 && y <= 40)
                        {
                            float aa = 1f - Mathf.Clamp01(distX - thick + 1.2f);
                            alpha = Mathf.Max(alpha, aa);
                        }
                    }
                    {
                        float lineX = 8f;
                        float thick = 2.5f;
                        float distX = Mathf.Abs(x - lineX);
                        if (distX < thick && y >= 28 && y <= 36)
                        {
                            float aa = 1f - Mathf.Clamp01(distX - thick + 1.2f);
                            alpha = Mathf.Max(alpha, aa);
                        }
                    }

                    // Vibration lines right
                    {
                        float lineX = 50f;
                        float thick = 2.5f;
                        float distX = Mathf.Abs(x - lineX);
                        if (distX < thick && y >= 24 && y <= 40)
                        {
                            float aa = 1f - Mathf.Clamp01(distX - thick + 1.2f);
                            alpha = Mathf.Max(alpha, aa);
                        }
                    }
                    {
                        float lineX = 56f;
                        float thick = 2.5f;
                        float distX = Mathf.Abs(x - lineX);
                        if (distX < thick && y >= 28 && y <= 36)
                        {
                            float aa = 1f - Mathf.Clamp01(distX - thick + 1.2f);
                            alpha = Mathf.Max(alpha, aa);
                        }
                    }

                    if (alpha > 0f)
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            cachedVibeIcon = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedVibeIcon.name = "VibeIcon";
            return cachedVibeIcon;
        }

        // ============================================================
        // REFRESH ICON - Saat yonunde dairesel ok
        // ============================================================
        public static Sprite CreateRefreshIconSprite()
        {
            if (cachedRefreshIcon != null) return cachedRefreshIcon;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float cx = 32f, cy = 32f, r = 18f;
            float thickness = 4.5f;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, Color.clear);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float arcDist = Mathf.Abs(dist - r);

                    float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (angle < 0) angle += 360f;

                    // Arc: 30 to 330 degrees (300 degree arc, gap at top-right)
                    bool onArc = angle >= 40f && angle <= 340f && arcDist < thickness;

                    if (onArc)
                    {
                        float aa = 1f - Mathf.Clamp01(arcDist - thickness + 1.2f);
                        if (aa > 0f)
                        {
                            Color existing = tex.GetPixel(x, y);
                            float newAlpha = Mathf.Max(existing.a, aa);
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, newAlpha));
                        }
                    }

                    // Arrow tip at ~40 degrees (start of arc, pointing clockwise/downward)
                    {
                        float tipAngle = 40f * Mathf.Deg2Rad;
                        float tipX = cx + r * Mathf.Cos(tipAngle);
                        float tipY = cy + r * Mathf.Sin(tipAngle);
                        float adx = x - tipX;
                        float ady = y - tipY;

                        // Arrow pointing along the arc tangent (downward at this position)
                        float arrowLen = 10f;
                        float arrowHalf = 7f;
                        // Tangent direction at 40 deg: perpendicular to radius, clockwise
                        float tangentX = -Mathf.Sin(tipAngle);
                        float tangentY = Mathf.Cos(tipAngle);
                        // Inward direction (toward center)
                        float inwardX = -Mathf.Cos(tipAngle);
                        float inwardY = -Mathf.Sin(tipAngle);

                        // Project onto tangent (arrow direction) and perpendicular
                        float projTangent = adx * (-tangentX) + ady * (-tangentY);
                        float projPerp = adx * inwardX + ady * inwardY;

                        if (projTangent >= -1 && projTangent <= arrowLen + 1)
                        {
                            float tA = Mathf.Clamp01(projTangent / arrowLen);
                            float halfW = arrowHalf * (1f - tA);
                            float distC = Mathf.Abs(projPerp);
                            if (distC <= halfW + 1.2f)
                            {
                                float aa2 = 1f - Mathf.Clamp01(distC - halfW);
                                Color existing = tex.GetPixel(x, y);
                                float newAlpha = Mathf.Max(existing.a, aa2);
                                tex.SetPixel(x, y, new Color(1f, 1f, 1f, newAlpha));
                            }
                        }
                    }
                }
            }

            tex.Apply();
            cachedRefreshIcon = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedRefreshIcon.name = "RefreshIcon";
            return cachedRefreshIcon;
        }

        // ============================================================
        // HOME ICON - Ev sekli (ucgen cati + dikdortgen govde)
        // ============================================================
        public static Sprite CreateHomeIconSprite()
        {
            if (cachedHomeIcon != null) return cachedHomeIcon;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, Color.clear);

            float cx = 32f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float alpha = 0f;

                    // Roof (triangle): peak at (32, 54), left (10, 32), right (54, 32)
                    float roofPeakX = cx, roofPeakY = 54f;
                    float roofLeftX = 10f, roofRightX = 54f, roofBaseY = 32f;

                    if (y >= roofBaseY && y <= roofPeakY)
                    {
                        float t = (y - roofBaseY) / (roofPeakY - roofBaseY);
                        float leftEdge = Mathf.Lerp(roofLeftX, roofPeakX, t);
                        float rightEdge = Mathf.Lerp(roofRightX, roofPeakX, t);

                        if (x >= leftEdge - 1f && x <= rightEdge + 1f)
                        {
                            float distL = x - leftEdge;
                            float distR = rightEdge - x;
                            float distB = y - roofBaseY;
                            float distT = roofPeakY - y;
                            float minDist = Mathf.Min(distL, distR);

                            // Filled triangle with AA edges
                            float edgeAA = Mathf.Clamp01(Mathf.Min(distL, distR) + 0.5f);
                            alpha = Mathf.Max(alpha, edgeAA);
                        }
                    }

                    // Body (rectangle): (16, 10) to (48, 34)
                    float bodyL = 16f, bodyR = 48f, bodyB = 10f, bodyT = 34f;

                    if (x >= bodyL - 0.5f && x <= bodyR + 0.5f && y >= bodyB - 0.5f && y <= bodyT + 0.5f)
                    {
                        float edgeL = x - bodyL;
                        float edgeR = bodyR - x;
                        float edgeB = y - bodyB;
                        float edgeT = bodyT - y;
                        float minEdge = Mathf.Min(edgeL, edgeR, edgeB, edgeT);
                        float aa = Mathf.Clamp01(minEdge + 0.5f);
                        alpha = Mathf.Max(alpha, aa);
                    }

                    // Door cutout (darker/empty area in body center)
                    float doorL = 26f, doorR = 38f, doorB = 10f, doorT = 26f;

                    if (x >= doorL + 0.5f && x <= doorR - 0.5f && y >= doorB - 0.5f && y <= doorT - 0.5f)
                    {
                        float edgeL2 = x - doorL;
                        float edgeR2 = doorR - x;
                        float edgeT2 = doorT - y;
                        float minEdge2 = Mathf.Min(edgeL2, edgeR2, edgeT2);
                        float cutout = Mathf.Clamp01(minEdge2 - 0.5f);
                        alpha = Mathf.Max(0f, alpha - cutout);
                    }

                    if (alpha > 0f)
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            cachedHomeIcon = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedHomeIcon.name = "HomeIcon";
            return cachedHomeIcon;
        }

        // ============================================================
        // PLAY ARROW ICON - Sag ok ucgeni
        // ============================================================
        public static Sprite CreatePlayArrowSprite()
        {
            if (cachedPlayArrow != null) return cachedPlayArrow;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, Color.clear);

            // Right-pointing triangle: left(18,12) to left(18,52) to right(50,32)
            float tipX = 50f, tipY = 32f;
            float baseX = 18f, baseTop = 52f, baseBot = 12f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (x < baseX - 1 || x > tipX + 1) continue;

                    float t = (x - baseX) / (tipX - baseX);
                    float halfH = (baseTop - baseBot) * 0.5f * (1f - t);
                    float centerY = (baseTop + baseBot) * 0.5f;
                    float distFromCenter = Mathf.Abs(y - centerY);

                    if (distFromCenter <= halfH + 1f)
                    {
                        float aa = Mathf.Clamp01(halfH - distFromCenter + 0.8f);
                        float edgeAA = Mathf.Clamp01((x - baseX + 0.8f)) * Mathf.Clamp01((tipX - x + 0.8f));
                        aa = Mathf.Min(aa, edgeAA);
                        if (aa > 0f)
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, aa));
                    }
                }
            }

            tex.Apply();
            cachedPlayArrow = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedPlayArrow.name = "PlayArrowIcon";
            return cachedPlayArrow;
        }

        // ============================================================
        // LEADERBOARD ICON - 3 cubuk grafik
        // ============================================================
        public static Sprite CreateLeaderboardSprite()
        {
            if (cachedLeaderboard != null) return cachedLeaderboard;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, Color.clear);

            float barWidth = 10f;
            float gap = 4f;
            float startX = 11f;
            float baseY = 10f;
            float[] heights = { 22f, 36f, 28f }; // left short, center tall, right medium

            for (int b = 0; b < 3; b++)
            {
                float bx = startX + b * (barWidth + gap);
                float barTop = baseY + heights[b];

                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        float distL = x - bx;
                        float distR = (bx + barWidth) - x;
                        float distB = y - baseY;
                        float distT = barTop - y;

                        if (distL >= -0.8f && distR >= -0.8f && distB >= -0.8f && distT >= -0.8f)
                        {
                            float aa = Mathf.Clamp01(Mathf.Min(distL + 0.8f, distR + 0.8f, distB + 0.8f, distT + 0.8f));
                            if (aa > 0f)
                            {
                                Color existing = tex.GetPixel(x, y);
                                float newA = Mathf.Max(existing.a, aa);
                                tex.SetPixel(x, y, new Color(1f, 1f, 1f, newA));
                            }
                        }
                    }
                }
            }

            tex.Apply();
            cachedLeaderboard = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedLeaderboard.name = "LeaderboardIcon";
            return cachedLeaderboard;
        }

        // ============================================================
        // SETTINGS GEAR ICON - Disli cark
        // ============================================================
        public static Sprite CreateSettingsGearSprite()
        {
            if (cachedSettingsGear != null) return cachedSettingsGear;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float cx = 32f, cy = 32f;
            float outerR = 24f, innerR = 17f, holeR = 8f;
            int teeth = 8;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);

                    // Gear profile: varies between innerR and outerR based on angle
                    float segAngle = Mathf.PI * 2f / teeth;
                    float localAngle = Mathf.Repeat(angle + Mathf.PI * 2f, segAngle);
                    float toothCenter = segAngle * 0.5f;
                    float toothHalfWidth = segAngle * 0.28f;
                    float distFromToothCenter = Mathf.Abs(localAngle - toothCenter);

                    float gearR;
                    if (distFromToothCenter < toothHalfWidth)
                        gearR = outerR;
                    else if (distFromToothCenter < toothHalfWidth + segAngle * 0.08f)
                    {
                        float t = (distFromToothCenter - toothHalfWidth) / (segAngle * 0.08f);
                        gearR = Mathf.Lerp(outerR, innerR, t);
                    }
                    else
                        gearR = innerR;

                    // Inside gear profile and outside hole
                    if (dist <= gearR + 1f && dist >= holeR - 1f)
                    {
                        float aaOuter = Mathf.Clamp01(gearR - dist + 0.8f);
                        float aaInner = Mathf.Clamp01(dist - holeR + 0.8f);
                        float aa = Mathf.Min(aaOuter, aaInner);
                        if (aa > 0f)
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, aa));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            cachedSettingsGear = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedSettingsGear.name = "SettingsGearIcon";
            return cachedSettingsGear;
        }

        // ============================================================
        // STAR ICON - 5 koseli yildiz
        // ============================================================
        public static Sprite CreateStarSprite()
        {
            if (cachedStar != null) return cachedStar;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float cx = 32f, cy = 32f;
            float outerR = 22f, innerR = 10f;
            int points = 5;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);

                    // Star radius at this angle
                    float segAngle = Mathf.PI * 2f / points;
                    float halfSeg = segAngle * 0.5f;
                    float localAngle = Mathf.Repeat(angle - Mathf.PI * 0.5f + Mathf.PI * 2f, segAngle);
                    float t = Mathf.Abs(localAngle - halfSeg) / halfSeg;
                    float starR = Mathf.Lerp(outerR, innerR, 1f - Mathf.Abs(1f - t * 2f));

                    if (dist <= starR + 1f)
                    {
                        float aa = Mathf.Clamp01(starR - dist + 0.8f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, aa));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            cachedStar = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedStar.name = "StarIcon";
            return cachedStar;
        }

        // ============================================================
        // GLOW ORB SPRITE - Yumusak radyal gradient daire (accent orb)
        // ============================================================
        public static Sprite CreateGlowOrbSprite()
        {
            if (cachedGlowOrb != null) return cachedGlowOrb;

            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float cx = size * 0.5f, cy = size * 0.5f;
            float maxR = size * 0.5f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = dist / maxR;
                    // Soft gaussian-ish falloff
                    float alpha = Mathf.Exp(-t * t * 3f);
                    alpha = Mathf.Clamp01(alpha);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            cachedGlowOrb = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedGlowOrb.name = "GlowOrb";
            return cachedGlowOrb;
        }

        // ============================================================
        // MENU BACKGROUND - Radyal gradient (indigo merkez → navy kenar)
        // ============================================================
        public static Sprite CreateMenuBackgroundSprite()
        {
            if (cachedMenuBg != null) return cachedMenuBg;

            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float cx = size * 0.5f, cy = size * 0.6f; // slightly above center
            float maxDist = size * 0.75f;
            Color centerColor = new Color(0.118f, 0.106f, 0.294f, 1f); // indigo-900 #1e1b4b
            Color edgeColor = new Color(0.020f, 0.031f, 0.078f, 1f);   // navy-deep #050814

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = Mathf.Clamp01(dist / maxDist);
                    // Smooth falloff
                    t = t * t;
                    Color c = Color.Lerp(centerColor, edgeColor, t);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            cachedMenuBg = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedMenuBg.name = "MenuBackground";
            return cachedMenuBg;
        }

        // ============================================================
        // CIRCLE BADGE SPRITE (kirmizi daire, badge icin)
        // ============================================================
        public static Sprite CreateCircleBadgeSprite()
        {
            if (cachedCircleBadge != null) return cachedCircleBadge;

            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float center = size * 0.5f;
            float radius = size * 0.5f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist <= radius - 1f)
                        tex.SetPixel(x, y, Color.white);
                    else if (dist <= radius)
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, radius - dist));
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply();
            cachedCircleBadge = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedCircleBadge.name = "CircleBadge";
            return cachedCircleBadge;
        }

        // ============================================================
        // TROPHY ICON - Kupa sekli (beyaz, renk Image.color ile verilir)
        // ============================================================
        public static Sprite CreateTrophyIconSprite()
        {
            if (cachedTrophyIcon != null) return cachedTrophyIcon;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, Color.clear);

            float cx = 32f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float alpha = 0f;

                    // Cup body (trapezoid, wider at top)
                    float cupTop = 52f, cupBot = 24f;
                    float topHalf = 18f, botHalf = 10f;

                    if (y >= cupBot && y <= cupTop)
                    {
                        float t = (y - cupBot) / (cupTop - cupBot);
                        float halfW = Mathf.Lerp(botHalf, topHalf, t);
                        float distFromCenter = Mathf.Abs(x - cx);

                        if (distFromCenter <= halfW + 1f)
                        {
                            float aa = Mathf.Clamp01(halfW - distFromCenter + 0.8f);
                            alpha = Mathf.Max(alpha, aa);
                        }
                    }

                    // Cup rim (top bar, wider)
                    if (y >= 50 && y <= 56)
                    {
                        float rimHalf = 20f;
                        float distFromCenter = Mathf.Abs(x - cx);
                        if (distFromCenter <= rimHalf + 0.5f)
                        {
                            float aa = Mathf.Clamp01(rimHalf - distFromCenter + 0.5f);
                            float yaa = Mathf.Clamp01(Mathf.Min(y - 50f, 56f - y) + 0.5f);
                            alpha = Mathf.Max(alpha, Mathf.Min(aa, yaa));
                        }
                    }

                    // Left handle (arc)
                    {
                        float hcx = 13f, hcy = 42f, hr = 9f;
                        float thick = 3.5f;
                        float dx = x - hcx, dy = y - hcy;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                        if (angle < 0) angle += 360f;
                        if (angle >= 90f && angle <= 270f)
                        {
                            float arcDist = Mathf.Abs(dist - hr);
                            if (arcDist < thick)
                            {
                                float aa = 1f - Mathf.Clamp01(arcDist - thick + 1.2f);
                                alpha = Mathf.Max(alpha, aa);
                            }
                        }
                    }

                    // Right handle (arc)
                    {
                        float hcx = 51f, hcy = 42f, hr = 9f;
                        float thick = 3.5f;
                        float dx = x - hcx, dy = y - hcy;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                        if (angle < 0) angle += 360f;
                        if (angle <= 90f || angle >= 270f)
                        {
                            float arcDist = Mathf.Abs(dist - hr);
                            if (arcDist < thick)
                            {
                                float aa = 1f - Mathf.Clamp01(arcDist - thick + 1.2f);
                                alpha = Mathf.Max(alpha, aa);
                            }
                        }
                    }

                    // Stem (narrow rectangle below cup)
                    if (y >= 16 && y <= 24 && Mathf.Abs(x - cx) <= 4f)
                    {
                        float aa = Mathf.Clamp01(4f - Mathf.Abs(x - cx) + 0.5f);
                        alpha = Mathf.Max(alpha, aa);
                    }

                    // Base (wide rectangle at bottom)
                    if (y >= 8 && y <= 16)
                    {
                        float baseHalf = 12f;
                        float distFromCenter = Mathf.Abs(x - cx);
                        if (distFromCenter <= baseHalf + 0.5f)
                        {
                            float aa = Mathf.Clamp01(baseHalf - distFromCenter + 0.5f);
                            float yaa = Mathf.Clamp01(Mathf.Min(y - 8f, 16f - y) + 0.5f);
                            alpha = Mathf.Max(alpha, Mathf.Min(aa, yaa));
                        }
                    }

                    if (alpha > 0f)
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            cachedTrophyIcon = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedTrophyIcon.name = "TrophyIcon";
            return cachedTrophyIcon;
        }

        // ============================================================
        // BACK ARROW ICON - Sol ok ("<")
        // ============================================================
        public static Sprite CreateBackArrowSprite()
        {
            if (cachedBackArrow != null) return cachedBackArrow;

            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            // Rounded-cap chevron (SF Symbols style)
            // Tip at left-center, arms extend to upper-right and lower-right
            Vector2 tip = new Vector2(36f, 64f);
            Vector2 topEnd = new Vector2(88f, 104f);
            Vector2 botEnd = new Vector2(88f, 24f);
            float radius = 6f; // stroke half-thickness (rounded caps)

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);

                    // Capsule SDF for each arm: distance to line segment minus radius
                    float d1 = CapsuleSDF(p, tip, topEnd, radius);
                    float d2 = CapsuleSDF(p, tip, botEnd, radius);
                    float dist = Mathf.Min(d1, d2);

                    // Smooth anti-aliasing (1.2px feather)
                    float alpha = Mathf.Clamp01(-dist / 1.2f + 0.5f);

                    tex.SetPixel(x, y, alpha > 0.004f ? new Color(1f, 1f, 1f, alpha) : Color.clear);
                }
            }

            tex.Apply();
            cachedBackArrow = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedBackArrow.name = "BackArrowIcon";
            return cachedBackArrow;
        }

        // Capsule SDF: distance from point p to line segment a-b with radius r
        private static float CapsuleSDF(Vector2 p, Vector2 a, Vector2 b, float r)
        {
            Vector2 pa = p - a, ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude - r;
        }

        // ============================================================
        // AVATAR PLACEHOLDER - Dolu daire (profil resmi yerine)
        // ============================================================
        public static Sprite CreateAvatarPlaceholderSprite()
        {
            if (cachedAvatarPlaceholder != null) return cachedAvatarPlaceholder;

            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float center = size * 0.5f;
            float radius = size * 0.5f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist <= radius - 1f)
                        tex.SetPixel(x, y, Color.white);
                    else if (dist <= radius)
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, radius - dist));
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply();
            cachedAvatarPlaceholder = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cachedAvatarPlaceholder.name = "AvatarPlaceholder";
            return cachedAvatarPlaceholder;
        }

        // ============================================================
        // SHARE ICON - Paylasim (ok yukari + iki dal)
        // ============================================================
        public static Sprite CreateShareIconSprite()
        {
            if (cachedShareIcon != null) return cachedShareIcon;
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] clear = new Color[size * size];
            tex.SetPixels(clear);

            float cx = 32f;
            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float a = 0f;
                // Vertical line (stem)
                if (y >= 24 && y <= 54 && Mathf.Abs(x - cx) < 3f)
                    a = Mathf.Max(a, Mathf.Clamp01(3f - Mathf.Abs(x - cx)));
                // Arrow head (V shape pointing up at y=54)
                float ax1 = cx - (54 - y) * 0.7f, ax2 = cx + (54 - y) * 0.7f;
                if (y >= 42 && y <= 56)
                {
                    float d1 = Mathf.Abs(x - ax1), d2 = Mathf.Abs(x - ax2);
                    float dMin = Mathf.Min(d1, d2);
                    if (dMin < 3.5f) a = Mathf.Max(a, Mathf.Clamp01(3.5f - dMin));
                }
                // Three dots (nodes)
                float r = 5f;
                foreach (var p in new Vector2[]{new Vector2(32,54), new Vector2(16,18), new Vector2(48,18)})
                {
                    float d = Vector2.Distance(new Vector2(x,y), p);
                    if (d < r) a = Mathf.Max(a, Mathf.Clamp01(r - d));
                }
                // Branch lines from center to bottom dots
                foreach (var target in new Vector2[]{new Vector2(16,18), new Vector2(48,18)})
                {
                    float midY = 36f;
                    float dx = target.x - cx, dy = target.y - midY;
                    float len = Mathf.Sqrt(dx*dx+dy*dy);
                    float nx=-dy/len, ny=dx/len;
                    float px=x-cx, py=y-midY;
                    float proj=(px*dx+py*dy)/len;
                    float perp=Mathf.Abs(px*nx+py*ny);
                    if(proj>=0&&proj<=len&&perp<2.5f)
                        a=Mathf.Max(a,Mathf.Clamp01(2.5f-perp));
                }
                if (a > 0f) tex.SetPixel(x, y, new Color(1,1,1,a));
            }
            tex.Apply();
            cachedShareIcon = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), size);
            cachedShareIcon.name = "ShareIcon";
            return cachedShareIcon;
        }

        // ============================================================
        // GLOBE ICON - Kure (daire + enlem/boylam cizgileri)
        // ============================================================
        public static Sprite CreateGlobeIconSprite()
        {
            if (cachedGlobeIcon != null) return cachedGlobeIcon;
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] clear = new Color[size * size];
            tex.SetPixels(clear);

            float cx = 32f, cy = 32f, R = 24f;
            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dx = x - cx, dy = y - cy;
                float dist = Mathf.Sqrt(dx*dx+dy*dy);
                float a = 0f;
                // Outer circle ring
                float ringDist = Mathf.Abs(dist - R);
                if (ringDist < 2.5f) a = Mathf.Max(a, Mathf.Clamp01(2.5f - ringDist));
                // Only draw internals inside circle
                if (dist <= R + 0.5f)
                {
                    // Horizontal equator line
                    if (Mathf.Abs(dy) < 2f) a = Mathf.Max(a, Mathf.Clamp01(2f - Mathf.Abs(dy)));
                    // Vertical meridian line
                    if (Mathf.Abs(dx) < 2f) a = Mathf.Max(a, Mathf.Clamp01(2f - Mathf.Abs(dx)));
                    // Elliptical meridian lines
                    foreach (float scale in new float[]{0.5f})
                    {
                        float ex = dx / (R * scale);
                        float ey = dy / R;
                        float eDist = Mathf.Sqrt(ex*ex+ey*ey);
                        float eRing = Mathf.Abs(eDist - 1f) * R * scale;
                        if (eRing < 2f) a = Mathf.Max(a, Mathf.Clamp01(2f - eRing) * 0.7f);
                    }
                    // Latitude lines
                    foreach (float lat in new float[]{-12f, 12f})
                    {
                        float halfW = Mathf.Sqrt(Mathf.Max(0, R*R - lat*lat));
                        if (Mathf.Abs(dx) <= halfW && Mathf.Abs(dy - lat) < 1.8f)
                            a = Mathf.Max(a, Mathf.Clamp01(1.8f - Mathf.Abs(dy - lat)) * 0.6f);
                    }
                }
                if (a > 0f) tex.SetPixel(x, y, new Color(1,1,1,Mathf.Clamp01(a)));
            }
            tex.Apply();
            cachedGlobeIcon = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), size);
            cachedGlobeIcon.name = "GlobeIcon";
            return cachedGlobeIcon;
        }

        // ============================================================
        // DOCUMENT ICON - Belge (dikdortgen + text cizgileri)
        // ============================================================
        public static Sprite CreateDocumentIconSprite()
        {
            if (cachedDocumentIcon != null) return cachedDocumentIcon;
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] clear = new Color[size * size];
            tex.SetPixels(clear);

            float left = 16, right = 48, bot = 8, top = 56;
            float corner = 10; // folded corner size
            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float a = 0f;
                bool inDoc = x >= left && x <= right && y >= bot && y <= top;
                // Cut corner (top-right)
                if (x > right - corner && y > top - corner)
                    inDoc = inDoc && (x - (right - corner) + y - (top - corner) <= corner);

                if (inDoc)
                {
                    // Border
                    float dLeft = x - left, dRight = right - x, dBot = y - bot, dTop = top - y;
                    float dEdge = Mathf.Min(Mathf.Min(dLeft,dRight),Mathf.Min(dBot,dTop));
                    if (dEdge < 2.5f) a = Mathf.Max(a, Mathf.Clamp01(2.5f - dEdge));
                    // Text lines
                    foreach (float ly in new float[]{20, 28, 36, 44})
                    {
                        float lineRight = (ly == 44) ? 34 : 42;
                        if (y >= ly - 1.2f && y <= ly + 1.2f && x >= 22 && x <= lineRight)
                            a = Mathf.Max(a, Mathf.Clamp01(1.2f - Mathf.Abs(y - ly)) * 0.5f);
                    }
                }
                // Fold line
                {
                    float fx = right - corner, fy = top - corner;
                    float fdx = x - fx, fdy = y - fy;
                    float fDist = Mathf.Abs(fdx + fdy) / 1.414f;
                    if (fDist < 1.5f && x >= fx && y >= fy && x <= right && y <= top)
                        a = Mathf.Max(a, Mathf.Clamp01(1.5f - fDist) * 0.8f);
                }
                if (a > 0f) tex.SetPixel(x, y, new Color(1,1,1,Mathf.Clamp01(a)));
            }
            tex.Apply();
            cachedDocumentIcon = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), size);
            cachedDocumentIcon.name = "DocumentIcon";
            return cachedDocumentIcon;
        }

        // ============================================================
        // MAIL ICON - Mektup zarfi
        // ============================================================
        public static Sprite CreateMailIconSprite()
        {
            if (cachedMailIcon != null) return cachedMailIcon;
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] clear = new Color[size * size];
            tex.SetPixels(clear);

            float left = 10, right = 54, bot = 16, top = 48;
            float cx = 32f;
            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float a = 0f;
                // Envelope rectangle border
                if (x >= left && x <= right && y >= bot && y <= top)
                {
                    float dL = x-left, dR = right-x, dB = y-bot, dT = top-y;
                    float dEdge = Mathf.Min(Mathf.Min(dL,dR),Mathf.Min(dB,dT));
                    if (dEdge < 2.5f) a = Mathf.Max(a, Mathf.Clamp01(2.5f - dEdge));
                }
                // V flap (from top-left to center-top to top-right)
                float flapY = top;
                float midY = 30f; // apex of V
                // Left arm: (left, flapY) -> (cx, midY)
                {
                    float dx2 = cx-left, dy2 = midY-flapY;
                    float len = Mathf.Sqrt(dx2*dx2+dy2*dy2);
                    float nx=-dy2/len, ny=dx2/len;
                    float px=x-left, py=y-flapY;
                    float proj=(px*dx2+py*dy2)/len;
                    float perp=Mathf.Abs(px*nx+py*ny);
                    if(proj>=-1&&proj<=len+1&&perp<2.5f)
                        a=Mathf.Max(a,Mathf.Clamp01(2.5f-perp)*Mathf.Clamp01(Mathf.Min(proj+1,len-proj+1)));
                }
                // Right arm: (right, flapY) -> (cx, midY)
                {
                    float dx2 = cx-right, dy2 = midY-flapY;
                    float len = Mathf.Sqrt(dx2*dx2+dy2*dy2);
                    float nx=-dy2/len, ny=dx2/len;
                    float px=x-right, py=y-flapY;
                    float proj=(px*dx2+py*dy2)/len;
                    float perp=Mathf.Abs(px*nx+py*ny);
                    if(proj>=-1&&proj<=len+1&&perp<2.5f)
                        a=Mathf.Max(a,Mathf.Clamp01(2.5f-perp)*Mathf.Clamp01(Mathf.Min(proj+1,len-proj+1)));
                }
                if (a > 0f) tex.SetPixel(x, y, new Color(1,1,1,Mathf.Clamp01(a)));
            }
            tex.Apply();
            cachedMailIcon = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), size);
            cachedMailIcon.name = "MailIcon";
            return cachedMailIcon;
        }

        // ============================================================
        // CHEVRON RIGHT - Sag ok (">")
        // ============================================================
        public static Sprite CreateChevronRightSprite()
        {
            if (cachedChevronRight != null) return cachedChevronRight;
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] clear = new Color[size * size];
            tex.SetPixels(clear);

            float tipX = 42f, tipY = 32f;
            float startX = 22f;
            float topY = 48f, botY = 16f;
            float thick = 4f;
            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float a = 0f;
                // Upper arm: (startX, topY) -> (tipX, tipY)
                {
                    float dx2=tipX-startX, dy2=tipY-topY;
                    float len=Mathf.Sqrt(dx2*dx2+dy2*dy2);
                    float nx=-dy2/len, ny=dx2/len;
                    float px=x-startX, py=y-topY;
                    float proj=(px*dx2+py*dy2)/len;
                    float perp=Mathf.Abs(px*nx+py*ny);
                    if(proj>=-1&&proj<=len+1&&perp<thick)
                    {
                        float aa=1f-Mathf.Clamp01(perp-thick+1.2f);
                        float edge=Mathf.Clamp01(Mathf.Min(proj+1,len-proj+1));
                        a=Mathf.Max(a,Mathf.Min(aa,edge));
                    }
                }
                // Lower arm: (startX, botY) -> (tipX, tipY)
                {
                    float dx2=tipX-startX, dy2=tipY-botY;
                    float len=Mathf.Sqrt(dx2*dx2+dy2*dy2);
                    float nx=-dy2/len, ny=dx2/len;
                    float px=x-startX, py=y-botY;
                    float proj=(px*dx2+py*dy2)/len;
                    float perp=Mathf.Abs(px*nx+py*ny);
                    if(proj>=-1&&proj<=len+1&&perp<thick)
                    {
                        float aa=1f-Mathf.Clamp01(perp-thick+1.2f);
                        float edge=Mathf.Clamp01(Mathf.Min(proj+1,len-proj+1));
                        a=Mathf.Max(a,Mathf.Min(aa,edge));
                    }
                }
                if (a > 0f) tex.SetPixel(x, y, new Color(1,1,1,a));
            }
            tex.Apply();
            cachedChevronRight = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), size);
            cachedChevronRight.name = "ChevronRight";
            return cachedChevronRight;
        }

        // ============================================================
        // EXTERNAL LINK ICON - Dis baglanti (kare + ok)
        // ============================================================
        public static Sprite CreateExternalLinkSprite()
        {
            if (cachedExternalLink != null) return cachedExternalLink;
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] clear = new Color[size * size];
            tex.SetPixels(clear);

            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float a = 0f;
                // Open rectangle (bottom-left, missing top-right corner)
                float l=12,r=42,b=12,t=42;
                bool inRect = x>=l&&x<=r&&y>=b&&y<=t;
                if (inRect)
                {
                    float dL=x-l,dR=r-x,dB=y-b,dT=t-y;
                    float dEdge=Mathf.Min(Mathf.Min(dL,dR),Mathf.Min(dB,dT));
                    // Skip top edge for x > 30 and right edge for y > 30
                    bool skipTop = y > t-3 && x > 30;
                    bool skipRight = x > r-3 && y > 30;
                    if (dEdge < 2.5f && !skipTop && !skipRight)
                        a = Mathf.Max(a, Mathf.Clamp01(2.5f - dEdge));
                }
                // Diagonal arrow line from (28,28) to (50,50)
                {
                    float ax1=28,ay1=28,ax2=50,ay2=50;
                    float dx2=ax2-ax1,dy2=ay2-ay1;
                    float len=Mathf.Sqrt(dx2*dx2+dy2*dy2);
                    float nx=-dy2/len,ny=dx2/len;
                    float px=x-ax1,py=y-ay1;
                    float proj=(px*dx2+py*dy2)/len;
                    float perp=Mathf.Abs(px*nx+py*ny);
                    if(proj>=0&&proj<=len&&perp<2.5f)
                        a=Mathf.Max(a,Mathf.Clamp01(2.5f-perp));
                }
                // Arrow head at (50,50) - horizontal bar
                if (y>=48&&y<=52&&x>=40&&x<=52)
                    a=Mathf.Max(a,Mathf.Clamp01(Mathf.Min(y-48f,52f-y)));
                // Arrow head at (50,50) - vertical bar
                if (x>=48&&x<=52&&y>=40&&y<=52)
                    a=Mathf.Max(a,Mathf.Clamp01(Mathf.Min(x-48f,52f-x)));
                if (a > 0f) tex.SetPixel(x, y, new Color(1,1,1,Mathf.Clamp01(a)));
            }
            tex.Apply();
            cachedExternalLink = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), size);
            cachedExternalLink.name = "ExternalLink";
            return cachedExternalLink;
        }

        // ============================================================
        // TOGGLE PILL SPRITE - iOS-style capsule track for switches
        // ============================================================
        public static Sprite CreateTogglePillSprite()
        {
            if (cachedTogglePill != null) return cachedTogglePill;

            // 2x resolution for crisp rendering. Rendered at 64x38 with PPU=200,
            // so 1 source pixel = 0.5 screen pixels. Border 38 → 19 screen px.
            // Top+Bottom borders (19+19=38) = render height → perfect pill caps.
            int w = 128, h = 76;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float radius = h * 0.5f; // 38 - perfect half-circle ends
            Color white = Color.white;
            Color clear = Color.clear;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float dist;

                    if (px < radius)
                    {
                        // Left cap
                        float dx = px - radius;
                        float dy = py - radius;
                        dist = Mathf.Sqrt(dx * dx + dy * dy) - radius;
                    }
                    else if (px > w - radius)
                    {
                        // Right cap
                        float dx = px - (w - radius);
                        float dy = py - radius;
                        dist = Mathf.Sqrt(dx * dx + dy * dy) - radius;
                    }
                    else
                    {
                        // Middle straight section
                        dist = Mathf.Abs(py - radius) - radius;
                    }

                    if (dist <= -1f)
                        tex.SetPixel(x, y, white);
                    else if (dist <= 0f)
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, -dist));
                    else
                        tex.SetPixel(x, y, clear);
                }
            }

            tex.Apply();

            // 9-slice: border=38 source px → 19 screen px at PPU=200
            Vector4 border = new Vector4(38, 38, 38, 38);
            cachedTogglePill = Sprite.Create(tex, new Rect(0, 0, w, h),
                new Vector2(0.5f, 0.5f), 200f, 0, SpriteMeshType.Tight, border);
            cachedTogglePill.name = "TogglePill";
            return cachedTogglePill;
        }

        // ============================================================
        // CORNER RADIUS HELPER
        // ============================================================
        // ============================================================
        // SPLASH GRID SPRITE - Soluk 8x8 grid deseni (radial fade)
        // ============================================================
        public static Sprite CreateSplashGridSprite()
        {
            if (cachedSplashGrid != null) return cachedSplashGrid;

            int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            int cellSize = 56;
            int gap = 4;
            int gridTotal = 8 * cellSize + 7 * gap; // 476
            int margin = (size - gridTotal) / 2;     // 18
            int cellRadius = 6;
            float center = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Check if inside a grid cell
                    int localX = x - margin;
                    int localY = y - margin;

                    float cellAlpha = 0f;
                    if (localX >= 0 && localX < gridTotal && localY >= 0 && localY < gridTotal)
                    {
                        int cellCol = localX / (cellSize + gap);
                        int cellRow = localY / (cellSize + gap);
                        int inCellX = localX - cellCol * (cellSize + gap);
                        int inCellY = localY - cellRow * (cellSize + gap);

                        if (cellCol < 8 && cellRow < 8 && inCellX < cellSize && inCellY < cellSize)
                        {
                            cellAlpha = GetRoundedRectAlphaCustom(inCellX, inCellY, cellSize, cellRadius);
                        }
                    }

                    // Radial gaussian fade from center
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = dist / (size * 0.45f);
                    float radialMask = Mathf.Exp(-t * t * 2.5f);

                    float finalAlpha = cellAlpha * radialMask * 0.08f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, finalAlpha));
                }
            }

            tex.Apply();
            cachedSplashGrid = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
            return cachedSplashGrid;
        }

        // ============================================================
        // SPLASH BURST SPRITE - Merkezden radial patlama cizgileri
        // ============================================================
        public static Sprite CreateSplashBurstSprite()
        {
            if (cachedSplashBurst != null) return cachedSplashBurst;

            int size = 512;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float center = size / 2f;
            int lineCount = 12;
            float lineThickness = 2.5f;
            float innerR = 80f;
            float outerR = 220f;
            float feather = 1.2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist < innerR - feather || dist > outerR + feather)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float angle = Mathf.Atan2(dy, dx);
                    float bestDist = float.MaxValue;

                    for (int i = 0; i < lineCount; i++)
                    {
                        float lineAngle = i * Mathf.PI * 2f / lineCount;
                        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(angle * Mathf.Rad2Deg, lineAngle * Mathf.Rad2Deg)) * Mathf.Deg2Rad;
                        float perpDist = dist * Mathf.Sin(angleDiff);
                        if (perpDist < bestDist) bestDist = perpDist;
                    }

                    float lineMask = 1f - Mathf.Clamp01((bestDist - lineThickness) / feather);
                    float distT = Mathf.Clamp01((dist - innerR) / (outerR - innerR));
                    float distAlpha = 1f - distT; // taper from inner to outer

                    // Smooth edges at inner/outer boundaries
                    float innerFade = Mathf.Clamp01((dist - innerR + feather) / feather);
                    float outerFade = Mathf.Clamp01((outerR + feather - dist) / feather);

                    float finalAlpha = lineMask * distAlpha * 0.3f * innerFade * outerFade;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, finalAlpha));
                }
            }

            tex.Apply();
            cachedSplashBurst = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
            return cachedSplashBurst;
        }

        // ============================================================
        // APP ICON TEXTURE - 1024x1024 full-color icon for iOS
        // ============================================================
        public static Texture2D CreateAppIconTexture()
        {
            int size = 1024;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            // Colors
            Color bgCenter = new Color(0.118f, 0.063f, 0.282f, 1f);  // #1E1048
            Color bgEdge = new Color(0.039f, 0.059f, 0.118f, 1f);    // #0A0F1E
            Color glowPurple = new Color(0.545f, 0.361f, 0.965f, 1f); // #8B5CF6
            Color gold = new Color(0.961f, 0.620f, 0.043f, 1f);       // #F59E0B

            // Block definitions: (cx, cy, color)
            var blocks = new (int cx, int cy, Color color)[]
            {
                (430, 560, new Color(0.545f, 0.361f, 0.965f, 1f)), // Purple
                (560, 560, new Color(0.231f, 0.510f, 0.965f, 1f)), // Blue
                (430, 430, new Color(0.063f, 0.725f, 0.506f, 1f)), // Green
                (560, 430, new Color(0.961f, 0.620f, 0.043f, 1f)), // Gold
                (560, 690, new Color(0.545f, 0.361f, 0.965f, 1f)), // Purple
            };
            int blockSize = 120;
            int blockRadius = 18;

            // Fragment definitions: (cx, cy, size, angle_deg, color)
            var fragments = new (int cx, int cy, int sz, float angle, Color color)[]
            {
                (220, 300, 28, 25f, glowPurple),
                (780, 350, 24, -15f, gold),
                (250, 720, 22, 40f, new Color(0.231f, 0.510f, 0.965f, 1f)),
                (800, 680, 26, -30f, new Color(0.063f, 0.725f, 0.506f, 1f)),
                (340, 200, 20, 55f, gold),
                (700, 820, 24, -45f, new Color(0.925f, 0.282f, 0.600f, 1f)),
                (180, 520, 22, 15f, new Color(0.937f, 0.267f, 0.267f, 1f)),
                (850, 500, 20, -60f, glowPurple),
            };

            float cx = 512f, cy = 560f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Layer 1: Background gradient
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float bgT = Mathf.Clamp01(dist / 768f);
                    bgT = bgT * bgT;
                    Color pixel = Color.Lerp(bgCenter, bgEdge, bgT);

                    // Layer 2: Center glow
                    float glowDist = Mathf.Sqrt((x - 512f) * (x - 512f) + (y - 512f) * (y - 512f)) / 400f;
                    float glowAlpha = Mathf.Exp(-glowDist * glowDist * 2.5f) * 0.15f;
                    pixel.r += glowPurple.r * glowAlpha;
                    pixel.g += glowPurple.g * glowAlpha;
                    pixel.b += glowPurple.b * glowAlpha;

                    // Layer 3: Star burst (8 points)
                    float starDist = Mathf.Sqrt((x - 495f) * (x - 495f) + (y - 540f) * (y - 540f));
                    if (starDist < 420f && starDist > 50f)
                    {
                        float starAngle = Mathf.Atan2(y - 540f, x - 495f);
                        float cosVal = Mathf.Cos(starAngle * 8f);
                        float starR = Mathf.Lerp(180f, 400f, (cosVal + 1f) * 0.5f);
                        if (starDist < starR)
                        {
                            float starT = starDist / starR;
                            float starAlpha = (1f - starT) * 0.25f;
                            pixel.r += gold.r * starAlpha;
                            pixel.g += gold.g * starAlpha;
                            pixel.b += gold.b * starAlpha;
                        }
                    }

                    // Layer 4: Blocks
                    foreach (var block in blocks)
                    {
                        int bx = x - (block.cx - blockSize / 2);
                        int by = y - (block.cy - blockSize / 2);

                        if (bx >= 0 && bx < blockSize && by >= 0 && by < blockSize)
                        {
                            float bAlpha = GetRoundedRectAlphaCustom(bx, by, blockSize, blockRadius);
                            if (bAlpha > 0f)
                            {
                                // Block shading
                                float distTop = blockSize - 1 - by;
                                float distBottom = by;
                                float minEdge = Mathf.Min(bx, blockSize - 1 - bx, distBottom, distTop);

                                float brightness = Mathf.Lerp(0.82f, 0.95f, (float)by / blockSize);

                                // Border highlight
                                if (minEdge < 3f)
                                    brightness = Mathf.Lerp(1.12f, brightness, minEdge / 3f);

                                // Glossy top zone
                                float glossZone = (float)by / blockSize;
                                if (glossZone > 0.6f)
                                    brightness += (glossZone - 0.6f) * 0.3f;

                                // Inner glow
                                float innerDist = Mathf.Sqrt((bx - blockSize / 2f) * (bx - blockSize / 2f) +
                                    (by - blockSize / 2f) * (by - blockSize / 2f)) / (blockSize * 0.5f);
                                brightness += Mathf.Exp(-innerDist * innerDist * 3f) * 0.08f;

                                brightness = Mathf.Clamp(brightness, 0f, 1.2f);

                                Color blockColor = new Color(
                                    block.color.r * brightness,
                                    block.color.g * brightness,
                                    block.color.b * brightness,
                                    1f
                                );

                                pixel = Color.Lerp(pixel, blockColor, bAlpha);
                            }
                        }
                    }

                    // Layer 5: Fragments
                    foreach (var frag in fragments)
                    {
                        float fdx = x - frag.cx;
                        float fdy = y - frag.cy;
                        float rad = frag.angle * Mathf.Deg2Rad;
                        float rx = fdx * Mathf.Cos(rad) + fdy * Mathf.Sin(rad);
                        float ry = -fdx * Mathf.Sin(rad) + fdy * Mathf.Cos(rad);

                        float halfSz = frag.sz / 2f;
                        float fragR = 4f;
                        if (Mathf.Abs(rx) < halfSz && Mathf.Abs(ry) < halfSz)
                        {
                            int lx = (int)(rx + halfSz);
                            int ly = (int)(ry + halfSz);
                            float fAlpha = GetRoundedRectAlphaCustom(lx, ly, frag.sz, (int)fragR) * 0.22f;
                            if (fAlpha > 0f)
                            {
                                pixel.r += frag.color.r * fAlpha;
                                pixel.g += frag.color.g * fAlpha;
                                pixel.b += frag.color.b * fAlpha;
                            }
                        }
                    }

                    pixel.r = Mathf.Clamp01(pixel.r);
                    pixel.g = Mathf.Clamp01(pixel.g);
                    pixel.b = Mathf.Clamp01(pixel.b);
                    pixel.a = 1f;
                    tex.SetPixel(x, y, pixel);
                }
            }

            tex.Apply();
            return tex;
        }

        // Custom rounded rect alpha for arbitrary sizes (not limited to square textures)
        private static float GetRoundedRectAlphaCustom(int x, int y, int size, int radius)
        {
            Vector2 pixel = new Vector2(x + 0.5f, y + 0.5f);
            Vector2 cornerCenter = Vector2.zero;
            bool inCorner = false;

            if (x < radius && y < radius)
            { cornerCenter = new Vector2(radius, radius); inCorner = true; }
            else if (x >= size - radius && y < radius)
            { cornerCenter = new Vector2(size - radius, radius); inCorner = true; }
            else if (x < radius && y >= size - radius)
            { cornerCenter = new Vector2(radius, size - radius); inCorner = true; }
            else if (x >= size - radius && y >= size - radius)
            { cornerCenter = new Vector2(size - radius, size - radius); inCorner = true; }

            if (!inCorner) return 1f;
            float d = Vector2.Distance(pixel, cornerCenter);
            if (d <= radius - 0.7f) return 1f;
            if (d >= radius + 0.7f) return 0f;
            return Mathf.Clamp01(radius + 0.7f - d);
        }

        // CORNER RADIUS HELPER (original)
        // ============================================================
        private static float GetRoundedRectAlpha(int x, int y, int size, int radius)
        {
            Vector2 pixel = new Vector2(x + 0.5f, y + 0.5f);
            Vector2 cornerCenter;
            bool inCorner = false;

            if (x < radius && y < radius)
            {
                cornerCenter = new Vector2(radius, radius);
                inCorner = true;
            }
            else if (x >= size - radius && y < radius)
            {
                cornerCenter = new Vector2(size - radius, radius);
                inCorner = true;
            }
            else if (x < radius && y >= size - radius)
            {
                cornerCenter = new Vector2(radius, size - radius);
                inCorner = true;
            }
            else if (x >= size - radius && y >= size - radius)
            {
                cornerCenter = new Vector2(size - radius, size - radius);
                inCorner = true;
            }
            else
            {
                return 1f;
            }

            if (!inCorner) return 1f;

            float dist = Vector2.Distance(pixel, cornerCenter);
            if (dist <= radius - 0.7f) return 1f;
            if (dist >= radius + 0.7f) return 0f;
            return Mathf.Clamp01(radius + 0.7f - dist);
        }
    }
}
