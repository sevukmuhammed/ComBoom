using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ComBoom.Core;
using ComBoom.Gameplay;
using ComBoom.Input;
using ComBoom.UI;
using ComBoom.Ads;

public class SceneSetup : EditorWindow
{
    [MenuItem("ComBoom/Setup Game Scene")]
    public static void SetupScene()
    {
        if (!EditorUtility.DisplayDialog("ComBoom Scene Setup",
            "Bu islem mevcut sahnedeki tum objeleri silip oyun sahnesini sifirdan kuracak.\n\nDevam etmek istiyor musunuz?",
            "Evet, Kur", "Iptal"))
        {
            return;
        }

        SpriteGenerator.ClearAllCaches();
        ClearScene();
        SetupCamera();
        CreateBackground();
        GameObject gameManagerObj = CreateGameManager();
        GameObject gridManagerObj = CreateGridManager();
        GameObject pieceSpawnerObj = CreatePieceSpawner();
        GameObject dragDropObj = CreateDragDropHandler(gridManagerObj);
        GameObject audioManagerObj = CreateAudioManager();
        GameObject canvasObj = CreateUI();

        // LayoutManager olustur
        GameObject layoutObj = CreateLayoutManager(gridManagerObj, pieceSpawnerObj);

        // PowerUpManager olustur
        GameObject powerUpObj = new GameObject("PowerUpManager");
        powerUpObj.AddComponent<PowerUpManager>();

        // LevelManager olustur
        GameObject levelManagerObj = new GameObject("LevelManager");
        levelManagerObj.AddComponent<LevelManager>();

        // AdManager olustur
        GameObject adManagerObj = CreateAdManager();

        // Splash screen olustur (Canvas altinda, tam ekran)
        CreateSplashScreen(canvasObj.transform);

        // Continue Panel olustur
        CreateContinuePanel(canvasObj.transform);

        // Referanslari bagla
        WireReferences(gameManagerObj, gridManagerObj, pieceSpawnerObj, dragDropObj, audioManagerObj, canvasObj, layoutObj, powerUpObj, levelManagerObj, adManagerObj);

        // Sahneyi kaydet (otomatik - MarkDirty + Save)
        var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);

        Debug.Log("[ComBoom] Sahne kurulumu tamamlandi ve KAYDEDILDI!");
    }

    private static void ClearScene()
    {
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj != null && obj.transform.parent == null)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }

    // ============================================================
    // CAMERA
    // ============================================================
    private static void SetupCamera()
    {
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";

        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.backgroundColor = new Color(0.039f, 0.059f, 0.118f, 1f); // #0A0F1E
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        camObj.transform.position = new Vector3(0, 0, -10f);

        camObj.AddComponent<AudioListener>();
    }

    // ============================================================
    // BACKGROUND
    // ============================================================
    private static void CreateBackground()
    {
        GameObject bgObj = new GameObject("Background");
        SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();

        // Buyuk beyaz sprite olustur, renk ile koyu arka plan yap
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite bgSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        sr.sprite = bgSprite;
        sr.color = new Color(0.039f, 0.059f, 0.118f, 1f); // #0A0F1E
        sr.sortingOrder = -10;
        bgObj.transform.localScale = new Vector3(20f, 20f, 1f);
        bgObj.transform.position = Vector3.zero;
    }

    // ============================================================
    // GAME MANAGER
    // ============================================================
    private static GameObject CreateGameManager()
    {
        GameObject obj = new GameObject("GameManager");
        obj.AddComponent<GameManager>();
        return obj;
    }

    // ============================================================
    // GRID MANAGER
    // ============================================================
    private static GameObject CreateGridManager()
    {
        GameObject obj = new GameObject("GridManager");
        obj.transform.position = new Vector3(0, 1.2f, 0); // Grid biraz yukarda
        obj.AddComponent<GridManager>();

        // Grid arka plan cercevesi (koyu panel)
        GameObject gridBg = new GameObject("GridBackground");
        gridBg.transform.SetParent(obj.transform);
        gridBg.transform.localPosition = Vector3.zero;
        SpriteRenderer bgSr = gridBg.AddComponent<SpriteRenderer>();

        bgSr.sprite = SpriteGenerator.CreateGridBgSprite();
        bgSr.color = new Color(0.06f, 0.08f, 0.15f, 1f); // slate-900/60
        bgSr.sortingOrder = 0;

        float gridVisualSize = 8 * 0.7f + 0.4f; // cellSize + spacing + padding
        gridBg.transform.localScale = new Vector3(gridVisualSize, gridVisualSize, 1f);

        return obj;
    }

    // ============================================================
    // PIECE SPAWNER (3 slot)
    // ============================================================
    private static GameObject CreatePieceSpawner()
    {
        GameObject obj = new GameObject("PieceSpawner");
        obj.transform.position = new Vector3(0, -3.4f, 0);
        obj.AddComponent<PieceSpawner>();

        // 3 slot pozisyonu
        float spacing = 3.2f;
        for (int i = 0; i < 3; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(obj.transform);
            float x = (i - 1) * spacing;
            slot.transform.localPosition = new Vector3(x, 0, 0);

            // Slot gorseli (yuvarlatilmis koseli cerceve)
            SpriteRenderer sr = slot.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateSlotSprite();
            sr.color = new Color(0.08f, 0.11f, 0.18f, 0.25f); // slate-800/20
            sr.sortingOrder = -1;
            slot.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
        }

        return obj;
    }

    // ============================================================
    // LAYOUT MANAGER
    // ============================================================
    private static GameObject CreateLayoutManager(GameObject gridManagerObj, GameObject pieceSpawnerObj)
    {
        GameObject obj = new GameObject("LayoutManager");
        LayoutManager layout = obj.AddComponent<LayoutManager>();

        Camera cam = Camera.main;
        Transform gridTr = gridManagerObj.transform;
        Transform spawnerTr = pieceSpawnerObj.transform;

        // Slot transform'larini topla
        Transform[] slots = new Transform[3];
        for (int i = 0; i < 3; i++)
            slots[i] = spawnerTr.Find($"Slot_{i}");

        SetPrivateField(layout, "mainCamera", cam);
        SetPrivateField(layout, "gridTransform", gridTr);
        SetPrivateField(layout, "pieceSpawnerTransform", spawnerTr);
        SetPrivateField(layout, "slotTransforms", slots);

        // Constraint ayarları
        SetPrivateField(layout, "scoreAreaPadding", 1.6f);
        SetPrivateField(layout, "gridToSpawnGap", 0.5f);      // Grid-Spawn arası
        SetPrivateField(layout, "slotAreaHeight", 2.0f);
        SetPrivateField(layout, "bannerHeight", 1.8f);        // Banner + ActionBar için

        return obj;
    }

    // ============================================================
    // DRAG DROP HANDLER
    // ============================================================
    private static GameObject CreateDragDropHandler(GameObject gridManagerObj)
    {
        GameObject obj = new GameObject("DragDropHandler");
        obj.AddComponent<DragDropHandler>();
        return obj;
    }

    // ============================================================
    // AUDIO MANAGER
    // ============================================================
    private static GameObject CreateAudioManager()
    {
        GameObject obj = new GameObject("AudioManager");
        obj.AddComponent<AudioManager>();
        return obj;
    }

    // ============================================================
    // UI (Canvas + Score + GameOver)
    // ============================================================
    private static GameObject CreateUI()
    {
        // --- CANVAS ---
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // --- UI MANAGER ---
        UIManager uiManager = canvasObj.AddComponent<UIManager>();

        // --- SAFE AREA PANEL ---
        GameObject safeAreaObj = new GameObject("SafeAreaPanel");
        safeAreaObj.transform.SetParent(canvasObj.transform, false);
        RectTransform safeAreaRT = safeAreaObj.AddComponent<RectTransform>();
        safeAreaRT.anchorMin = Vector2.zero;
        safeAreaRT.anchorMax = Vector2.one;
        safeAreaRT.offsetMin = Vector2.zero;
        safeAreaRT.offsetMax = Vector2.zero;
        safeAreaObj.AddComponent<SafeAreaPanel>();

        // --- GAME UI PANEL (inside SafeArea) ---
        GameObject gameUI = CreatePanel(safeAreaObj.transform, "GameUI");

        // === HEADER AREA (2 ayri skor kutusu + Pause butonu) ===
        GameObject headerArea = CreatePanel(gameUI.transform, "HeaderArea");
        RectTransform headerAreaRect = headerArea.GetComponent<RectTransform>();
        headerAreaRect.anchorMin = new Vector2(0, 0.935f);
        headerAreaRect.anchorMax = new Vector2(1, 1);
        headerAreaRect.offsetMin = Vector2.zero;
        headerAreaRect.offsetMax = Vector2.zero;
        Image headerAreaImg = headerArea.GetComponent<Image>();
        headerAreaImg.color = Color.clear;
        headerAreaImg.raycastTarget = false;

        // --- SCORE BOX (Sol %40) ---
        GameObject scoreBox = CreatePanel(headerArea.transform, "ScoreBox");
        RectTransform scoreBoxRect = scoreBox.GetComponent<RectTransform>();
        scoreBoxRect.anchorMin = new Vector2(0.02f, 0.05f);
        scoreBoxRect.anchorMax = new Vector2(0.42f, 0.95f);
        scoreBoxRect.offsetMin = Vector2.zero;
        scoreBoxRect.offsetMax = Vector2.zero;
        Image scoreBoxImg = scoreBox.GetComponent<Image>();
        scoreBoxImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        scoreBoxImg.type = Image.Type.Sliced;
        scoreBoxImg.color = new Color(0.08f, 0.09f, 0.15f, 0.60f); // slate-800/40

        // Score label
        GameObject scoreLabelObj = CreateTextElement(scoreBox.transform, "ScoreLabel", "SCORE",
            22, TextAlignmentOptions.Center, new Color(0.580f, 0.639f, 0.722f, 1f)); // #94A3B8
        RectTransform scoreLabelRect = scoreLabelObj.GetComponent<RectTransform>();
        scoreLabelRect.anchorMin = new Vector2(0.05f, 0f);
        scoreLabelRect.anchorMax = new Vector2(0.95f, 0.40f);
        scoreLabelRect.offsetMin = Vector2.zero;
        scoreLabelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI scoreLabelTMP = scoreLabelObj.GetComponent<TextMeshProUGUI>();
        scoreLabelTMP.characterSpacing = 8f; // tracking-widest
        AddLocKey(scoreLabelObj, "score");

        // Score value
        GameObject scoreValueObj = CreateTextElement(scoreBox.transform, "ScoreValue", "0",
            52, TextAlignmentOptions.Center, Color.white);
        RectTransform scoreValueRect = scoreValueObj.GetComponent<RectTransform>();
        scoreValueRect.anchorMin = new Vector2(0.05f, 0.35f);
        scoreValueRect.anchorMax = new Vector2(0.95f, 1f);
        scoreValueRect.offsetMin = Vector2.zero;
        scoreValueRect.offsetMax = Vector2.zero;

        // --- PAUSE BUTTON (Orta %16) ---
        GameObject pauseBtn = CreateButton(headerArea.transform, "PauseButton", "||",
            new Color(0.08f, 0.09f, 0.15f, 0.60f), new Color(0.580f, 0.639f, 0.722f, 1f));
        RectTransform pauseRect = pauseBtn.GetComponent<RectTransform>();
        pauseRect.anchorMin = new Vector2(0.43f, 0.15f);
        pauseRect.anchorMax = new Vector2(0.57f, 0.85f);
        pauseRect.offsetMin = Vector2.zero;
        pauseRect.offsetMax = Vector2.zero;
        pauseBtn.GetComponentInChildren<TextMeshProUGUI>().fontSize = 36;

        // --- BEST BOX (Sag %40) ---
        GameObject bestBox = CreatePanel(headerArea.transform, "BestBox");
        RectTransform bestBoxRect = bestBox.GetComponent<RectTransform>();
        bestBoxRect.anchorMin = new Vector2(0.58f, 0.05f);
        bestBoxRect.anchorMax = new Vector2(0.98f, 0.95f);
        bestBoxRect.offsetMin = Vector2.zero;
        bestBoxRect.offsetMax = Vector2.zero;
        Image bestBoxImg = bestBox.GetComponent<Image>();
        bestBoxImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        bestBoxImg.type = Image.Type.Sliced;
        bestBoxImg.color = new Color(0.08f, 0.09f, 0.15f, 0.60f); // slate-800/40

        // Best label
        GameObject highLabelObj = CreateTextElement(bestBox.transform, "HighScoreLabel", "BEST",
            22, TextAlignmentOptions.Center, new Color(0.580f, 0.639f, 0.722f, 1f)); // #94A3B8
        RectTransform highLabelRect = highLabelObj.GetComponent<RectTransform>();
        highLabelRect.anchorMin = new Vector2(0.05f, 0f);
        highLabelRect.anchorMax = new Vector2(0.95f, 0.40f);
        highLabelRect.offsetMin = Vector2.zero;
        highLabelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI highLabelTMP = highLabelObj.GetComponent<TextMeshProUGUI>();
        highLabelTMP.characterSpacing = 8f; // tracking-widest
        AddLocKey(highLabelObj, "best");

        // Best value
        GameObject highValueObj = CreateTextElement(bestBox.transform, "HighScoreValue", "0",
            52, TextAlignmentOptions.Center, new Color(0.961f, 0.620f, 0.043f, 1f)); // #F59E0B
        RectTransform highValueRect = highValueObj.GetComponent<RectTransform>();
        highValueRect.anchorMin = new Vector2(0.05f, 0.35f);
        highValueRect.anchorMax = new Vector2(0.95f, 1f);
        highValueRect.offsetMin = Vector2.zero;
        highValueRect.offsetMax = Vector2.zero;

        // ScoreDisplay component - HeaderArea uzerine
        ScoreDisplay scoreDisplay = headerArea.AddComponent<ScoreDisplay>();

        // === LEVEL PROGRESS BAR (header altinda ince serit) ===
        GameObject levelBarArea = CreatePanel(gameUI.transform, "LevelBarArea");
        RectTransform levelBarAreaRect = levelBarArea.GetComponent<RectTransform>();
        levelBarAreaRect.anchorMin = new Vector2(0.02f, 0.915f);
        levelBarAreaRect.anchorMax = new Vector2(0.98f, 0.935f);
        levelBarAreaRect.offsetMin = Vector2.zero;
        levelBarAreaRect.offsetMax = Vector2.zero;
        Image levelBarAreaImg = levelBarArea.GetComponent<Image>();
        levelBarAreaImg.color = Color.clear;
        levelBarAreaImg.raycastTarget = false;

        // Level badge (sol taraf) - "LV.1"
        GameObject levelBadge = new GameObject("LevelBadge");
        levelBadge.transform.SetParent(levelBarArea.transform, false);
        RectTransform badgeRT = levelBadge.AddComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(0f, 0f);
        badgeRT.anchorMax = new Vector2(0.18f, 1f);
        badgeRT.offsetMin = Vector2.zero;
        badgeRT.offsetMax = Vector2.zero;
        Image badgeBg = levelBadge.AddComponent<Image>();
        badgeBg.sprite = SpriteGenerator.CreateRoundedUISprite();
        badgeBg.type = Image.Type.Sliced;
        badgeBg.color = new Color(0.08f, 0.09f, 0.15f, 0.80f);

        GameObject levelLabelObj = CreateTextElement(levelBadge.transform, "LevelLabel", "LV.1",
            20, TextAlignmentOptions.Center, new Color(0.545f, 0.361f, 0.965f, 1f));
        RectTransform levelLabelRT = levelLabelObj.GetComponent<RectTransform>();
        levelLabelRT.anchorMin = Vector2.zero;
        levelLabelRT.anchorMax = Vector2.one;
        levelLabelRT.offsetMin = Vector2.zero;
        levelLabelRT.offsetMax = Vector2.zero;

        // Progress bar background (sag taraf - badge ile ayni corner radius)
        GameObject barBg = new GameObject("BarBackground");
        barBg.transform.SetParent(levelBarArea.transform, false);
        RectTransform barBgRT = barBg.AddComponent<RectTransform>();
        barBgRT.anchorMin = new Vector2(0.20f, 0f);
        barBgRT.anchorMax = new Vector2(1f, 1f);
        barBgRT.offsetMin = Vector2.zero;
        barBgRT.offsetMax = Vector2.zero;
        Image barBgImg = barBg.AddComponent<Image>();
        barBgImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        barBgImg.type = Image.Type.Sliced;
        barBgImg.color = new Color(0.118f, 0.161f, 0.231f, 1f); // slate-800

        // Mask: fill cocuklarini ayni seklinde kirp
        Mask barMask = barBg.AddComponent<Mask>();
        barMask.showMaskGraphic = true;

        // Progress bar fill (ayni sprite + sliced = ayni corner radius)
        GameObject barFill = new GameObject("BarFill");
        barFill.transform.SetParent(barBg.transform, false);
        RectTransform barFillRT = barFill.AddComponent<RectTransform>();
        barFillRT.anchorMin = Vector2.zero;
        barFillRT.anchorMax = new Vector2(0.3f, 1f); // baslangic %30
        barFillRT.offsetMin = Vector2.zero;
        barFillRT.offsetMax = Vector2.zero;
        Image barFillImg = barFill.AddComponent<Image>();
        barFillImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        barFillImg.type = Image.Type.Sliced;
        barFillImg.color = new Color(0.545f, 0.361f, 0.965f, 1f); // purple #8B5CF6

        // Progress bar glow (level-up flash icin, tam genislik)
        GameObject barGlow = new GameObject("BarGlow");
        barGlow.transform.SetParent(barBg.transform, false);
        RectTransform barGlowRT = barGlow.AddComponent<RectTransform>();
        barGlowRT.anchorMin = Vector2.zero;
        barGlowRT.anchorMax = Vector2.one;
        barGlowRT.offsetMin = Vector2.zero;
        barGlowRT.offsetMax = Vector2.zero;
        Image barGlowImg = barGlow.AddComponent<Image>();
        barGlowImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        barGlowImg.type = Image.Type.Sliced;
        barGlowImg.color = Color.clear; // baslangicta gizli

        // LevelProgressBar component (bar referanslari)
        LevelProgressBar levelProgressBar = levelBarArea.AddComponent<LevelProgressBar>();
        SetPrivateField(levelProgressBar, "levelLabel", levelLabelObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(levelProgressBar, "fillBar", barFillImg);
        SetPrivateField(levelProgressBar, "fillBarGlow", barGlowImg);

        // === ACTION BUTTONS (UNDO / BOMB / SHUFFLE) ===
        // ActionBar - Banner icin %8 yukarda baslıyor
        GameObject actionBar = CreatePanel(gameUI.transform, "ActionBar");
        RectTransform actionBarRect = actionBar.GetComponent<RectTransform>();
        actionBarRect.anchorMin = new Vector2(0.05f, 0.08f);
        actionBarRect.anchorMax = new Vector2(0.95f, 0.14f);
        actionBarRect.offsetMin = Vector2.zero;
        actionBarRect.offsetMax = Vector2.zero;
        Image actionBarImg = actionBar.GetComponent<Image>();
        actionBarImg.color = Color.clear;
        actionBarImg.raycastTarget = false;

        string[] actionLabels = { "UNDO", "BOMB", "SHUFFLE" };
        string[] actionLocKeys = { "undo", "bomb", "shuffle" };
        Sprite[] actionIcons = {
            SpriteGenerator.CreateUndoIconSprite(),
            SpriteGenerator.CreateBombIconSprite(),
            SpriteGenerator.CreateShuffleIconSprite()
        };

        for (int i = 0; i < 3; i++)
        {
            float btnStart = i * 0.34f;
            float btnEnd = btnStart + 0.30f;

            // Undo ikonu kendi rengine sahip, diğerleri tint alıyor
            Color iconColor = (i == 0) ? Color.white : new Color(0.580f, 0.639f, 0.722f, 1f);

            GameObject actionBtn = CreateActionButtonWithIcon(actionBar.transform,
                $"ActionBtn_{actionLabels[i]}", actionLabels[i], actionIcons[i],
                new Color(0.08f, 0.09f, 0.15f, 0.55f), iconColor);
            RectTransform actionBtnRect = actionBtn.GetComponent<RectTransform>();
            actionBtnRect.anchorMin = new Vector2(btnStart, 0f);
            actionBtnRect.anchorMax = new Vector2(btnEnd, 1f);
            actionBtnRect.offsetMin = Vector2.zero;
            actionBtnRect.offsetMax = Vector2.zero;

            // Undo ikonu boyutunu küçült (diğerleriyle aynı görünsün)
            if (i == 0)
            {
                Transform iconTr = actionBtn.transform.Find("Icon");
                if (iconTr != null)
                {
                    RectTransform iconRt = iconTr.GetComponent<RectTransform>();
                    iconRt.anchorMin = new Vector2(0.30f, 0.42f);
                    iconRt.anchorMax = new Vector2(0.70f, 0.90f);
                }
            }

            // Undo ikonu için label rengini ayrı ayarla
            TextMeshProUGUI actionLabelTMP = actionBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (actionLabelTMP != null)
            {
                AddLocKey(actionLabelTMP.gameObject, actionLocKeys[i]);
                if (i == 0) actionLabelTMP.color = new Color(0.580f, 0.639f, 0.722f, 1f);
            }

            // Kirmizi badge (sag ust kose, kalan sayi)
            CreateCountBadge(actionBtn.transform, $"Badge_{actionLabels[i]}");
        }

        // === COMBO DISPLAY (ekranin ortasinda, combo gostergesi) ===
        GameObject comboPanel = CreatePanel(gameUI.transform, "ComboDisplay");
        Image comboPanelImg = comboPanel.GetComponent<Image>();
        comboPanelImg.color = Color.clear;
        comboPanelImg.raycastTarget = false;
        comboPanel.SetActive(false);

        // Combo content container
        GameObject comboContent = new GameObject("ComboContent");
        comboContent.transform.SetParent(comboPanel.transform, false);
        RectTransform comboContentRT = comboContent.AddComponent<RectTransform>();
        comboContentRT.anchorMin = new Vector2(0.15f, 0.45f);
        comboContentRT.anchorMax = new Vector2(0.85f, 0.65f);
        comboContentRT.offsetMin = Vector2.zero;
        comboContentRT.offsetMax = Vector2.zero;

        // Combo glow
        GameObject comboGlowObj = new GameObject("ComboGlow");
        comboGlowObj.transform.SetParent(comboContent.transform, false);
        RectTransform comboGlowRT = comboGlowObj.AddComponent<RectTransform>();
        comboGlowRT.anchorMin = new Vector2(-0.3f, -0.5f);
        comboGlowRT.anchorMax = new Vector2(1.3f, 1.5f);
        comboGlowRT.offsetMin = Vector2.zero;
        comboGlowRT.offsetMax = Vector2.zero;
        Image comboGlowImg = comboGlowObj.AddComponent<Image>();
        comboGlowImg.sprite = SpriteGenerator.CreateGlowOrbSprite();
        comboGlowImg.color = Color.clear;
        comboGlowImg.raycastTarget = false;

        // "COMBO" text
        GameObject comboTextObj = CreateTextElement(comboContent.transform, "ComboText", "COMBO",
            52, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.1f, 1f));
        RectTransform comboTextRT = comboTextObj.GetComponent<RectTransform>();
        comboTextRT.anchorMin = new Vector2(0f, 0.45f);
        comboTextRT.anchorMax = new Vector2(1f, 1f);
        comboTextRT.offsetMin = Vector2.zero;
        comboTextRT.offsetMax = Vector2.zero;
        comboTextObj.GetComponent<TextMeshProUGUI>().characterSpacing = 12f;

        // Multiplier text "x4!"
        GameObject comboMultObj = CreateTextElement(comboContent.transform, "MultiplierText", "x2!",
            72, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.1f, 1f));
        RectTransform comboMultRT = comboMultObj.GetComponent<RectTransform>();
        comboMultRT.anchorMin = new Vector2(0f, 0f);
        comboMultRT.anchorMax = new Vector2(1f, 0.55f);
        comboMultRT.offsetMin = Vector2.zero;
        comboMultRT.offsetMax = Vector2.zero;

        // ComboDisplay component + wiring
        ComboDisplay comboDisplay = comboPanel.AddComponent<ComboDisplay>();
        SetPrivateField(comboDisplay, "panel", comboPanel);
        SetPrivateField(comboDisplay, "contentRT", comboContentRT);
        SetPrivateField(comboDisplay, "comboText", comboTextObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(comboDisplay, "multiplierText", comboMultObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(comboDisplay, "glowImage", comboGlowImg);

        // === LEVEL UP BANNER (tam ekran overlay, GameUI icinde en ustte) ===
        GameObject bannerPanel = CreatePanel(gameUI.transform, "LevelUpBanner");
        Image bannerOverlayImg = bannerPanel.GetComponent<Image>();
        bannerOverlayImg.color = new Color(0f, 0f, 0f, 0f);
        bannerOverlayImg.raycastTarget = true; // animasyon sirasinda input engelle
        bannerPanel.SetActive(false);

        // Banner content container (ortada, scale animasyonu icin)
        GameObject bannerContent = new GameObject("BannerContent");
        bannerContent.transform.SetParent(bannerPanel.transform, false);
        RectTransform bannerContentRT = bannerContent.AddComponent<RectTransform>();
        bannerContentRT.anchorMin = new Vector2(0.05f, 0.30f);
        bannerContentRT.anchorMax = new Vector2(0.95f, 0.70f);
        bannerContentRT.offsetMin = Vector2.zero;
        bannerContentRT.offsetMax = Vector2.zero;

        // Purple glow (arka plan pariltisi)
        GameObject bannerGlowObj = new GameObject("BannerGlow");
        bannerGlowObj.transform.SetParent(bannerContent.transform, false);
        RectTransform bannerGlowRT = bannerGlowObj.AddComponent<RectTransform>();
        bannerGlowRT.anchorMin = new Vector2(-0.2f, -0.3f);
        bannerGlowRT.anchorMax = new Vector2(1.2f, 1.3f);
        bannerGlowRT.offsetMin = Vector2.zero;
        bannerGlowRT.offsetMax = Vector2.zero;
        Image bannerGlowImg = bannerGlowObj.AddComponent<Image>();
        bannerGlowImg.sprite = SpriteGenerator.CreateGlowOrbSprite();
        bannerGlowImg.color = Color.clear;
        bannerGlowImg.raycastTarget = false;

        // Star left (dekoratif)
        GameObject starLeftObj = CreateTextElement(bannerContent.transform, "StarLeft", "\u2605  \u2605  \u2605",
            32, TextAlignmentOptions.Center, new Color(0.961f, 0.620f, 0.043f, 1f));
        RectTransform starLeftRT = starLeftObj.GetComponent<RectTransform>();
        starLeftRT.anchorMin = new Vector2(0f, 0.72f);
        starLeftRT.anchorMax = new Vector2(1f, 0.95f);
        starLeftRT.offsetMin = Vector2.zero;
        starLeftRT.offsetMax = Vector2.zero;
        starLeftObj.GetComponent<TextMeshProUGUI>().characterSpacing = 16f;

        // "LEVEL UP!" title (buyuk, altin rengi)
        GameObject bannerTitleObj = CreateTextElement(bannerContent.transform, "BannerTitle", "LEVEL UP!",
            72, TextAlignmentOptions.Center, new Color(0.961f, 0.620f, 0.043f, 1f));
        RectTransform bannerTitleRT = bannerTitleObj.GetComponent<RectTransform>();
        bannerTitleRT.anchorMin = new Vector2(0f, 0.35f);
        bannerTitleRT.anchorMax = new Vector2(1f, 0.75f);
        bannerTitleRT.offsetMin = Vector2.zero;
        bannerTitleRT.offsetMax = Vector2.zero;

        // Level number ("LEVEL 2")
        GameObject bannerLevelObj = CreateTextElement(bannerContent.transform, "BannerLevel", "LEVEL 2",
            44, TextAlignmentOptions.Center, Color.white);
        RectTransform bannerLevelRT = bannerLevelObj.GetComponent<RectTransform>();
        bannerLevelRT.anchorMin = new Vector2(0f, 0.10f);
        bannerLevelRT.anchorMax = new Vector2(1f, 0.38f);
        bannerLevelRT.offsetMin = Vector2.zero;
        bannerLevelRT.offsetMax = Vector2.zero;
        bannerLevelObj.GetComponent<TextMeshProUGUI>().characterSpacing = 8f;

        // Star right (alt dekoratif)
        GameObject starRightObj = CreateTextElement(bannerContent.transform, "StarRight", "\u2605  \u2605  \u2605",
            32, TextAlignmentOptions.Center, new Color(0.961f, 0.620f, 0.043f, 1f));
        RectTransform starRightRT = starRightObj.GetComponent<RectTransform>();
        starRightRT.anchorMin = new Vector2(0f, 0.0f);
        starRightRT.anchorMax = new Vector2(1f, 0.18f);
        starRightRT.offsetMin = Vector2.zero;
        starRightRT.offsetMax = Vector2.zero;
        starRightObj.GetComponent<TextMeshProUGUI>().characterSpacing = 16f;

        // Wire banner references to LevelProgressBar
        SetPrivateField(levelProgressBar, "bannerPanel", bannerPanel);
        SetPrivateField(levelProgressBar, "bannerContent", bannerContentRT);
        SetPrivateField(levelProgressBar, "bannerOverlay", bannerOverlayImg);
        SetPrivateField(levelProgressBar, "bannerGlow", bannerGlowImg);
        SetPrivateField(levelProgressBar, "bannerTitle", bannerTitleObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(levelProgressBar, "bannerLevel", bannerLevelObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(levelProgressBar, "bannerStarLeft", starLeftObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(levelProgressBar, "bannerStarRight", starRightObj.GetComponent<TextMeshProUGUI>());

        // GameUI baslangicta gizli (ana menudan baslanacak)
        gameUI.SetActive(false);

        // --- GAME OVER PANEL ---
        GameObject gameOverObj = CreatePanel(canvasObj.transform, "GameOverPanel");
        Image goImage = gameOverObj.GetComponent<Image>();
        goImage.color = new Color(0.03f, 0.04f, 0.08f, 0.90f);
        CanvasGroup goCanvasGroup = gameOverObj.AddComponent<CanvasGroup>();
        gameOverObj.SetActive(false);

        GameOverPanel gameOverPanel = gameOverObj.AddComponent<GameOverPanel>();

        // Game Over title
        GameObject goTitle = CreateTextElement(gameOverObj.transform, "GameOverTitle", "GAME OVER",
            72, TextAlignmentOptions.Center, Color.white);
        RectTransform goTitleRect = goTitle.GetComponent<RectTransform>();
        goTitleRect.anchorMin = new Vector2(0.1f, 0.65f);
        goTitleRect.anchorMax = new Vector2(0.9f, 0.85f);
        goTitleRect.offsetMin = Vector2.zero;
        goTitleRect.offsetMax = Vector2.zero;
        AddLocKey(goTitle, "game_over");

        // Final score
        GameObject finalScoreObj = CreateTextElement(gameOverObj.transform, "FinalScore", "0",
            96, TextAlignmentOptions.Center, new Color(0.961f, 0.620f, 0.043f, 1f)); // #F59E0B
        RectTransform finalScoreRect = finalScoreObj.GetComponent<RectTransform>();
        finalScoreRect.anchorMin = new Vector2(0.1f, 0.48f);
        finalScoreRect.anchorMax = new Vector2(0.9f, 0.65f);
        finalScoreRect.offsetMin = Vector2.zero;
        finalScoreRect.offsetMax = Vector2.zero;

        // Best score
        GameObject bestScoreObj = CreateTextElement(gameOverObj.transform, "BestScore", "BEST: 0",
            42, TextAlignmentOptions.Center, new Color(0.7f, 0.7f, 0.7f, 1f));
        RectTransform bestScoreRect = bestScoreObj.GetComponent<RectTransform>();
        bestScoreRect.anchorMin = new Vector2(0.1f, 0.40f);
        bestScoreRect.anchorMax = new Vector2(0.9f, 0.50f);
        bestScoreRect.offsetMin = Vector2.zero;
        bestScoreRect.offsetMax = Vector2.zero;

        // Restart button
        GameObject restartBtn = CreateButton(gameOverObj.transform, "RestartButton", "RESTART",
            new Color(0.545f, 0.361f, 0.965f, 1f), new Color(1f, 1f, 1f, 1f)); // #8B5CF6 purple
        RectTransform restartRect = restartBtn.GetComponent<RectTransform>();
        restartRect.anchorMin = new Vector2(0.2f, 0.22f);
        restartRect.anchorMax = new Vector2(0.8f, 0.32f);
        restartRect.offsetMin = Vector2.zero;
        restartRect.offsetMax = Vector2.zero;
        AddLocKey(restartBtn.GetComponentInChildren<TextMeshProUGUI>().gameObject, "restart");

        // Restart buton onClick (runtime'da baglanacak)
        Button restartButton = restartBtn.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            restartButton.onClick,
            gameOverPanel.OnRestartButton);

        // --- MAIN MENU ---
        CreateMainMenu(canvasObj.transform);

        // --- RANKS PANEL ---
        CreateRanksPanel(canvasObj.transform);

        // --- SETTINGS PANEL ---
        CreateSettingsPanel(canvasObj.transform);

        // --- PAUSE MENU ---
        CreatePauseMenu(canvasObj.transform);

        // --- BANNER AD PLACEHOLDER (Canvas'a direkt, SafeArea disinda) ---
        GameObject bannerObj = new GameObject("BannerAdPlaceholder");
        bannerObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bannerRT = bannerObj.AddComponent<RectTransform>();
        bannerRT.anchorMin = new Vector2(0, 0);
        bannerRT.anchorMax = new Vector2(1, 0);
        bannerRT.pivot = new Vector2(0.5f, 0);
        bannerRT.sizeDelta = new Vector2(0, 90);
        Image bannerImg = bannerObj.AddComponent<Image>();
        bannerImg.color = new Color(0.06f, 0.07f, 0.12f, 0.95f);
        bannerImg.raycastTarget = false;

        GameObject bannerText = new GameObject("BannerText");
        bannerText.transform.SetParent(bannerObj.transform, false);
        RectTransform bannerTextRT = bannerText.AddComponent<RectTransform>();
        bannerTextRT.anchorMin = Vector2.zero;
        bannerTextRT.anchorMax = Vector2.one;
        bannerTextRT.offsetMin = Vector2.zero;
        bannerTextRT.offsetMax = Vector2.zero;
        TextMeshProUGUI bannerTMP = bannerText.AddComponent<TextMeshProUGUI>();
        bannerTMP.text = "AD SPACE";
        bannerTMP.fontSize = 20;
        bannerTMP.alignment = TextAlignmentOptions.Center;
        bannerTMP.color = new Color(0.4f, 0.4f, 0.5f, 0.5f);

        // Baslangicta gizli — reklam entegrasyonu yapilinca aktif edilecek
        bannerObj.SetActive(false);

        // --- EVENT SYSTEM ---
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // SerializedObject ile private alanlari bagla
        SetPrivateField(uiManager, "gameUI", gameUI);
        SetPrivateField(uiManager, "gameOverPanel", gameOverPanel);
        SetPrivateField(uiManager, "scoreDisplay", scoreDisplay);
        SetPrivateField(gameOverPanel, "panel", gameOverObj);
        SetPrivateField(gameOverPanel, "canvasGroup", goCanvasGroup);
        SetPrivateField(gameOverPanel, "finalScoreText", finalScoreObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(gameOverPanel, "bestScoreText", bestScoreObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(scoreDisplay, "scoreText", scoreValueObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(scoreDisplay, "highScoreText", highValueObj.GetComponent<TextMeshProUGUI>());

        return canvasObj;
    }

    // ============================================================
    // WIRE ALL REFERENCES
    // ============================================================
    private static void WireReferences(GameObject gameManagerObj, GameObject gridManagerObj,
        GameObject pieceSpawnerObj, GameObject dragDropObj, GameObject audioManagerObj, GameObject canvasObj,
        GameObject layoutManagerObj, GameObject powerUpObj, GameObject levelManagerObj, GameObject adManagerObj)
    {
        GameManager gm = gameManagerObj.GetComponent<GameManager>();
        GridManager gridMgr = gridManagerObj.GetComponent<GridManager>();
        PieceSpawner spawner = pieceSpawnerObj.GetComponent<PieceSpawner>();
        DragDropHandler drag = dragDropObj.GetComponent<DragDropHandler>();
        AudioManager audio = audioManagerObj.GetComponent<AudioManager>();
        UIManager uiMgr = canvasObj.GetComponent<UIManager>();
        LayoutManager layout = layoutManagerObj.GetComponent<LayoutManager>();
        PowerUpManager powerUp = powerUpObj.GetComponent<PowerUpManager>();
        ScoreManager scoreMgr = gameManagerObj.AddComponent<ScoreManager>();
        Camera mainCam = Camera.main;

        // GameManager referanslari
        SetPrivateField(gm, "gridManager", gridMgr);
        SetPrivateField(gm, "scoreManager", scoreMgr);
        SetPrivateField(gm, "pieceSpawner", spawner);
        SetPrivateField(gm, "dragDropHandler", drag);
        SetPrivateField(gm, "audioManager", audio);
        SetPrivateField(gm, "uiManager", uiMgr);
        SetPrivateField(gm, "layoutManager", layout);
        SetPrivateField(gm, "powerUpManager", powerUp);

        // ContinuePanel referansi
        Transform continuePanelTr = canvasObj.transform.Find("SafeAreaPanel/ContinuePanel");
        if (continuePanelTr != null)
        {
            ContinuePanel continuePanel = continuePanelTr.GetComponent<ContinuePanel>();
            SetPrivateField(gm, "continuePanel", continuePanel);
        }

        // PowerUpManager referanslari
        SetPrivateField(powerUp, "gridManager", gridMgr);
        SetPrivateField(powerUp, "scoreManager", scoreMgr);
        SetPrivateField(powerUp, "pieceSpawner", spawner);
        SetPrivateField(powerUp, "dragDropHandler", drag);
        SetPrivateField(powerUp, "mainCamera", mainCam);

        // DragDropHandler referanslari
        SetPrivateField(drag, "gridManager", gridMgr);
        SetPrivateField(drag, "mainCamera", mainCam);
        SetPrivateField(drag, "audioManager", audio);

        // UIManager referanslari
        SetPrivateField(uiMgr, "scoreManager", scoreMgr);

        // PieceSpawner slot pozisyonlari
        Transform[] slots = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            slots[i] = pieceSpawnerObj.transform.Find($"Slot_{i}");
        }
        SetPrivateField(spawner, "slotPositions", slots);

        // Pause button → GameManager.TogglePause
        Transform pauseBtnTr = canvasObj.transform.Find("SafeAreaPanel/GameUI/HeaderArea/PauseButton");
        if (pauseBtnTr != null)
        {
            Button pauseButton = pauseBtnTr.GetComponent<Button>();
            if (pauseButton != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(
                    pauseButton.onClick, gm.TogglePause);
            }
        }

        // Action buttons → PowerUpManager
        Transform actionBar = canvasObj.transform.Find("SafeAreaPanel/GameUI/ActionBar");
        if (actionBar != null)
        {
            // UNDO button + badge
            Transform undoBtnTr = actionBar.Find("ActionBtn_UNDO");
            if (undoBtnTr != null)
            {
                Button undoBtn = undoBtnTr.GetComponent<Button>();
                if (undoBtn != null)
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(undoBtn.onClick, powerUp.UseUndo);
                    SetPrivateField(powerUp, "undoButton", undoBtn);
                }
                Transform undoBadge = undoBtnTr.Find("Badge_UNDO/BadgeText");
                if (undoBadge != null)
                    SetPrivateField(powerUp, "undoCountText", undoBadge.GetComponent<TextMeshProUGUI>());
            }

            // BOMB button + badge
            Transform bombBtnTr = actionBar.Find("ActionBtn_BOMB");
            if (bombBtnTr != null)
            {
                Button bombBtn = bombBtnTr.GetComponent<Button>();
                if (bombBtn != null)
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(bombBtn.onClick, powerUp.UseBomb);
                    SetPrivateField(powerUp, "bombButton", bombBtn);
                }
                Transform bombBadge = bombBtnTr.Find("Badge_BOMB/BadgeText");
                if (bombBadge != null)
                    SetPrivateField(powerUp, "bombCountText", bombBadge.GetComponent<TextMeshProUGUI>());
            }

            // SHUFFLE button + badge
            Transform shuffleBtnTr = actionBar.Find("ActionBtn_SHUFFLE");
            if (shuffleBtnTr != null)
            {
                Button shuffleBtn = shuffleBtnTr.GetComponent<Button>();
                if (shuffleBtn != null)
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(shuffleBtn.onClick, powerUp.UseShuffle);
                    SetPrivateField(powerUp, "shuffleButton", shuffleBtn);
                }
                Transform shuffleBadge = shuffleBtnTr.Find("Badge_SHUFFLE/BadgeText");
                if (shuffleBadge != null)
                    SetPrivateField(powerUp, "shuffleCountText", shuffleBadge.GetComponent<TextMeshProUGUI>());
            }
        }

        // Pause menu → GameManager + AudioManager
        Transform pauseOverlayTr = canvasObj.transform.Find("PauseOverlay");
        if (pauseOverlayTr != null)
        {
            PausePanel pausePanelComp = pauseOverlayTr.GetComponent<PausePanel>();
            if (pausePanelComp != null)
            {
                SetPrivateField(gm, "pausePanel", pausePanelComp);
                SetPrivateField(pausePanelComp, "audioManager", audio);
            }
        }

        // SplashPanel → GameManager
        Transform splashTr = canvasObj.transform.Find("SplashPanel");
        if (splashTr != null)
        {
            SplashPanel splashPanelComp = splashTr.GetComponent<SplashPanel>();
            if (splashPanelComp != null)
                SetPrivateField(gm, "splashPanel", splashPanelComp);
        }

        // LevelManager → GameManager
        LevelManager levelMgr = levelManagerObj.GetComponent<LevelManager>();
        SetPrivateField(gm, "levelManager", levelMgr);

        // LevelProgressBar → GameManager
        Transform levelBarTr = canvasObj.transform.Find("SafeAreaPanel/GameUI/LevelBarArea");
        if (levelBarTr != null)
        {
            LevelProgressBar lpb = levelBarTr.GetComponent<LevelProgressBar>();
            if (lpb != null)
                SetPrivateField(gm, "levelProgressBar", lpb);
        }

        // ComboDisplay → GameManager
        Transform comboDisplayTr = canvasObj.transform.Find("SafeAreaPanel/GameUI/ComboDisplay");
        if (comboDisplayTr != null)
        {
            ComboDisplay cd = comboDisplayTr.GetComponent<ComboDisplay>();
            if (cd != null)
                SetPrivateField(gm, "comboDisplay", cd);
        }

        // Main menu → UIManager + AudioManager
        Transform mainMenuTr = canvasObj.transform.Find("MainMenuPanel");
        if (mainMenuTr != null)
        {
            MainMenuPanel mainMenuComp = mainMenuTr.GetComponent<MainMenuPanel>();
            if (mainMenuComp != null)
            {
                SetPrivateField(uiMgr, "mainMenuPanel", mainMenuComp);
                SetPrivateField(mainMenuComp, "audioManager", audio);

                // RanksPanel → MainMenuPanel
                Transform ranksTr = canvasObj.transform.Find("RanksPanel");
                if (ranksTr != null)
                {
                    RanksPanel ranksComp = ranksTr.GetComponent<RanksPanel>();
                    if (ranksComp != null)
                    {
                        SetPrivateField(mainMenuComp, "ranksPanel", ranksComp);
                        SetPrivateField(ranksComp, "audioManager", audio);
                    }
                }

                // SettingsPanel → MainMenuPanel
                Transform settingsTr = canvasObj.transform.Find("SettingsPanel");
                if (settingsTr != null)
                {
                    SettingsPanel settingsComp = settingsTr.GetComponent<SettingsPanel>();
                    if (settingsComp != null)
                    {
                        SetPrivateField(mainMenuComp, "settingsPanel", settingsComp);
                        SetPrivateField(settingsComp, "audioManager", audio);
                    }
                }
            }
        }
    }

    // ============================================================
    // SPLASH SCREEN
    // ============================================================
    private static void CreateSplashScreen(Transform canvasParent)
    {
        // --- SPLASH ROOT (tam ekran, Canvas altinda, en ust katman) ---
        GameObject splashRoot = new GameObject("SplashPanel");
        splashRoot.transform.SetParent(canvasParent, false);
        splashRoot.transform.SetAsLastSibling();

        RectTransform splashRT = splashRoot.AddComponent<RectTransform>();
        splashRT.anchorMin = Vector2.zero;
        splashRT.anchorMax = Vector2.one;
        splashRT.offsetMin = Vector2.zero;
        splashRT.offsetMax = Vector2.zero;

        // CanvasGroup (tum panel icin fade kontrolu)
        CanvasGroup splashCG = splashRoot.AddComponent<CanvasGroup>();
        splashCG.alpha = 1f;
        splashCG.blocksRaycasts = true;

        // Background (radyal gradient)
        Image splashBg = splashRoot.AddComponent<Image>();
        splashBg.sprite = SpriteGenerator.CreateMenuBackgroundSprite();
        splashBg.type = Image.Type.Simple;
        splashBg.color = Color.white;
        splashBg.raycastTarget = true;

        // SplashPanel component
        SplashPanel splashPanel = splashRoot.AddComponent<SplashPanel>();

        // --- TITLE CONTAINER (title + glow icin ortak parent, merkez) ---
        GameObject titleContainer = new GameObject("TitleContainer");
        titleContainer.transform.SetParent(splashRoot.transform, false);
        RectTransform titleContainerRT = titleContainer.AddComponent<RectTransform>();
        titleContainerRT.anchorMin = new Vector2(0.05f, 0.46f);
        titleContainerRT.anchorMax = new Vector2(0.95f, 0.58f);
        titleContainerRT.offsetMin = Vector2.zero;
        titleContainerRT.offsetMax = Vector2.zero;

        // --- TITLE GLOW (mor, arkada) ---
        GameObject titleGlow = CreateTextElement(titleContainer.transform, "TitleGlow", "COMBOOM",
            80, TextAlignmentOptions.Center, new Color(0.545f, 0.361f, 0.965f, 0.55f));
        RectTransform titleGlowRT = titleGlow.GetComponent<RectTransform>();
        titleGlowRT.anchorMin = Vector2.zero;
        titleGlowRT.anchorMax = Vector2.one;
        titleGlowRT.offsetMin = Vector2.zero;
        titleGlowRT.offsetMax = Vector2.zero;
        titleGlow.GetComponent<TextMeshProUGUI>().characterSpacing = 16f;

        // --- TITLE TEXT "COMBOOM" (beyaz) ---
        GameObject titleText = CreateTextElement(titleContainer.transform, "TitleText", "COMBOOM",
            80, TextAlignmentOptions.Center, Color.white);
        RectTransform titleTextRT = titleText.GetComponent<RectTransform>();
        titleTextRT.anchorMin = Vector2.zero;
        titleTextRT.anchorMax = Vector2.one;
        titleTextRT.offsetMin = Vector2.zero;
        titleTextRT.offsetMax = Vector2.zero;
        titleText.GetComponent<TextMeshProUGUI>().characterSpacing = 16f;

        // --- TAGLINE "BLOCK PUZZLE" (gold) ---
        GameObject tagline = CreateTextElement(splashRoot.transform, "Tagline", "BLOCK PUZZLE",
            24, TextAlignmentOptions.Center, new Color(0.961f, 0.620f, 0.043f, 1f));
        RectTransform taglineRT = tagline.GetComponent<RectTransform>();
        taglineRT.anchorMin = new Vector2(0.1f, 0.39f);
        taglineRT.anchorMax = new Vector2(0.9f, 0.46f);
        taglineRT.offsetMin = Vector2.zero;
        taglineRT.offsetMax = Vector2.zero;
        tagline.GetComponent<TextMeshProUGUI>().characterSpacing = 12f;

        // --- STUDIO TEXT "M3STUDIOS" ---
        GameObject studio = CreateTextElement(splashRoot.transform, "StudioText", "M3STUDIOS",
            18, TextAlignmentOptions.Center, new Color(0.580f, 0.639f, 0.722f, 0.5f));
        RectTransform studioRT = studio.GetComponent<RectTransform>();
        studioRT.anchorMin = new Vector2(0.2f, 0.05f);
        studioRT.anchorMax = new Vector2(0.8f, 0.10f);
        studioRT.offsetMin = Vector2.zero;
        studioRT.offsetMax = Vector2.zero;
        studio.GetComponent<TextMeshProUGUI>().characterSpacing = 8f;

        // Wire SplashPanel references (sadelestirilmis)
        SetPrivateField(splashPanel, "panel", splashRoot);
        SetPrivateField(splashPanel, "canvasGroup", splashCG);
        SetPrivateField(splashPanel, "titleRT", titleContainerRT);
        SetPrivateField(splashPanel, "titleText", titleText.GetComponent<TextMeshProUGUI>());
        SetPrivateField(splashPanel, "titleGlowText", titleGlow.GetComponent<TextMeshProUGUI>());
        SetPrivateField(splashPanel, "taglineText", tagline.GetComponent<TextMeshProUGUI>());
        SetPrivateField(splashPanel, "studioText", studio.GetComponent<TextMeshProUGUI>());

        Debug.Log("[ComBoom] Splash screen olusturuldu");
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================
    private static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = false;

        return panel;
    }

    private static GameObject CreateTextElement(Transform parent, string name, string text,
        int fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.font = GetDefaultTMPFont();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = false;

        return obj;
    }

    private static TMP_FontAsset _cachedFont;
    private static TMP_FontAsset GetDefaultTMPFont()
    {
        if (_cachedFont != null) return _cachedFont;
        _cachedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (_cachedFont == null)
            _cachedFont = TMP_Settings.defaultFontAsset;
        return _cachedFont;
    }

    private static GameObject CreateButton(Transform parent, string name, string label,
        Color bgColor, Color textColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = btnObj.AddComponent<Image>();
        img.sprite = SpriteGenerator.CreateRoundedUISprite();
        img.type = Image.Type.Sliced;
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = bgColor * 1.1f;
        colors.pressedColor = bgColor * 0.9f;
        btn.colors = colors;

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.font = GetDefaultTMPFont();
        tmp.text = label;
        tmp.fontSize = 48;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = textColor;
        tmp.fontStyle = FontStyles.Bold;

        return btnObj;
    }

    private static GameObject CreateActionButtonWithIcon(Transform parent, string name, string label,
        Sprite iconSprite, Color bgColor, Color contentColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = btnObj.AddComponent<Image>();
        img.sprite = SpriteGenerator.CreateRoundedUISprite();
        img.type = Image.Type.Sliced;
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = bgColor * 1.2f;
        colors.pressedColor = bgColor * 0.8f;
        btn.colors = colors;

        // Icon (ust %60)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(btnObj.transform, false);

        RectTransform iconRt = iconObj.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.25f, 0.35f);
        iconRt.anchorMax = new Vector2(0.75f, 0.95f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;

        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.sprite = iconSprite;
        iconImg.color = contentColor;
        iconImg.raycastTarget = false;
        iconImg.preserveAspect = true;

        // Label (alt %35)
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0f, 0f);
        textRt.anchorMax = new Vector2(1f, 0.38f);
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.font = GetDefaultTMPFont();
        tmp.text = label;
        tmp.fontSize = 16;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = contentColor;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = false;

        return btnObj;
    }

    // ============================================================
    // PAUSE MENU (HTML referans: bg-black/40 overlay, neon-border panel)
    // ============================================================
    private static void CreatePauseMenu(Transform parent)
    {
        // --- OVERLAY (bg-black/40) ---
        GameObject overlay = new GameObject("PauseOverlay");
        overlay.transform.SetParent(parent, false);
        RectTransform overlayRT = overlay.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;
        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.85f); // dark overlay
        overlayImg.raycastTarget = true;

        PausePanel pausePanel = overlay.AddComponent<PausePanel>();

        // --- GLOW (box-shadow: 0 0 15px rgba(139,92,246,0.5)) ---
        GameObject glow = new GameObject("PauseGlow");
        glow.transform.SetParent(overlay.transform, false);
        RectTransform glowRT = glow.AddComponent<RectTransform>();
        glowRT.anchorMin = new Vector2(0.08f, 0.155f);
        glowRT.anchorMax = new Vector2(0.92f, 0.845f);
        glowRT.offsetMin = Vector2.zero;
        glowRT.offsetMax = Vector2.zero;
        Image glowImg = glow.AddComponent<Image>();
        glowImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        glowImg.type = Image.Type.Sliced;
        glowImg.color = new Color(0.545f, 0.361f, 0.965f, 0.18f); // purple glow
        glowImg.raycastTarget = false;

        // --- NEON BORDER (border: 2px solid #8B5CF6) ---
        GameObject border = new GameObject("PauseBorder");
        border.transform.SetParent(overlay.transform, false);
        RectTransform borderRT = border.AddComponent<RectTransform>();
        borderRT.anchorMin = new Vector2(0.10f, 0.18f);
        borderRT.anchorMax = new Vector2(0.90f, 0.82f);
        borderRT.offsetMin = Vector2.zero;
        borderRT.offsetMax = Vector2.zero;
        Image borderImg = border.AddComponent<Image>();
        borderImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        borderImg.type = Image.Type.Sliced;
        borderImg.color = new Color(0.545f, 0.361f, 0.965f, 1f); // #8B5CF6 solid
        borderImg.raycastTarget = false;

        // --- CONTENT (bg-slate-900/90, child of border with inset) ---
        GameObject content = new GameObject("PauseContent");
        content.transform.SetParent(border.transform, false);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = Vector2.zero;
        contentRT.anchorMax = Vector2.one;
        contentRT.offsetMin = new Vector2(3f, 3f);   // 3px inset = visible border
        contentRT.offsetMax = new Vector2(-3f, -3f);
        Image contentImg = content.AddComponent<Image>();
        contentImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        contentImg.type = Image.Type.Sliced;
        contentImg.color = new Color(0.059f, 0.090f, 0.165f, 0.95f); // slate-900/90

        // --- PAUSED TITLE (text-5xl, tracking-[0.2em], neon-glow-text) ---
        GameObject title = CreateTextElement(content.transform, "PausedTitle", "PAUSED",
            60, TextAlignmentOptions.Center, Color.white);
        RectTransform titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.05f, 0.85f);
        titleRT.anchorMax = new Vector2(0.95f, 0.97f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;
        title.GetComponent<TextMeshProUGUI>().characterSpacing = 12f;
        AddLocKey(title, "paused");

        // --- RESUME BUTTON (bg-primary, shadow-neon-purple) ---
        GameObject resumeBtn = CreateButton(content.transform, "ResumeButton", "RESUME",
            new Color(0.545f, 0.361f, 0.965f, 1f), Color.white); // #8B5CF6
        RectTransform resumeRT = resumeBtn.GetComponent<RectTransform>();
        resumeRT.anchorMin = new Vector2(0.10f, 0.69f);
        resumeRT.anchorMax = new Vector2(0.90f, 0.80f);
        resumeRT.offsetMin = Vector2.zero;
        resumeRT.offsetMax = Vector2.zero;
        resumeBtn.GetComponentInChildren<TextMeshProUGUI>().fontSize = 36;
        resumeBtn.GetComponentInChildren<TextMeshProUGUI>().characterSpacing = 6f;
        AddLocKey(resumeBtn.GetComponentInChildren<TextMeshProUGUI>().gameObject, "resume");

        // --- RESTART BUTTON (bg-slate-800, border-slate-700, icon orange) ---
        GameObject restartPauseBtn = CreatePauseMenuButton(content.transform, "RestartButton", "RESTART",
            SpriteGenerator.CreateRefreshIconSprite(),
            new Color(0.961f, 0.620f, 0.043f, 1f)); // #F59E0B jewel-orange
        RectTransform restartPRT = restartPauseBtn.GetComponent<RectTransform>();
        restartPRT.anchorMin = new Vector2(0.10f, 0.55f);
        restartPRT.anchorMax = new Vector2(0.90f, 0.66f);
        restartPRT.offsetMin = Vector2.zero;
        restartPRT.offsetMax = Vector2.zero;
        AddLocKey(restartPauseBtn.GetComponentInChildren<TextMeshProUGUI>().gameObject, "restart");

        // --- HOME BUTTON (bg-slate-800, border-slate-700, icon blue) ---
        GameObject homeBtn = CreatePauseMenuButton(content.transform, "HomeButton", "HOME",
            SpriteGenerator.CreateHomeIconSprite(),
            new Color(0.231f, 0.510f, 0.965f, 1f)); // #3B82F6 jewel-blue
        RectTransform homeRT = homeBtn.GetComponent<RectTransform>();
        homeRT.anchorMin = new Vector2(0.10f, 0.41f);
        homeRT.anchorMax = new Vector2(0.90f, 0.52f);
        homeRT.offsetMin = Vector2.zero;
        homeRT.offsetMax = Vector2.zero;
        AddLocKey(homeBtn.GetComponentInChildren<TextMeshProUGUI>().gameObject, "home");

        // --- DIVIDER (border-t border-slate-800) ---
        GameObject divider = new GameObject("Divider");
        divider.transform.SetParent(content.transform, false);
        RectTransform dividerRT = divider.AddComponent<RectTransform>();
        dividerRT.anchorMin = new Vector2(0.10f, 0.375f);
        dividerRT.anchorMax = new Vector2(0.90f, 0.377f);
        dividerRT.offsetMin = Vector2.zero;
        dividerRT.offsetMax = Vector2.zero;
        Image dividerImg = divider.AddComponent<Image>();
        dividerImg.color = new Color(0.118f, 0.161f, 0.231f, 1f); // slate-800
        dividerImg.raycastTarget = false;

        // --- SETTINGS LABEL (text-[10px], slate-500, tracking-widest) ---
        GameObject settingsLabel = CreateTextElement(content.transform, "SettingsLabel", "SETTINGS",
            16, TextAlignmentOptions.Center, new Color(0.392f, 0.455f, 0.545f, 1f)); // #64748B slate-500
        RectTransform settingsRT = settingsLabel.GetComponent<RectTransform>();
        settingsRT.anchorMin = new Vector2(0.1f, 0.31f);
        settingsRT.anchorMax = new Vector2(0.9f, 0.37f);
        settingsRT.offsetMin = Vector2.zero;
        settingsRT.offsetMax = Vector2.zero;
        settingsLabel.GetComponent<TextMeshProUGUI>().characterSpacing = 8f;
        AddLocKey(settingsLabel, "settings");

        // --- SETTINGS TOGGLES (w-12 h-12 rounded-xl bg-slate-800 border-slate-700) ---
        Image soundIcon = CreateSettingsToggle(content.transform, "SoundToggle",
            SpriteGenerator.CreateSoundIconSprite(), "SOUND",
            new Color(0.063f, 0.725f, 0.506f, 1f), // #10B981 jewel-green
            new Vector2(0.08f, 0.05f), new Vector2(0.36f, 0.29f),
            out Button soundBtn);

        Image musicIcon = CreateSettingsToggle(content.transform, "MusicToggle",
            SpriteGenerator.CreateMusicIconSprite(), "MUSIC",
            new Color(0.925f, 0.282f, 0.600f, 1f), // #EC4899 jewel-pink
            new Vector2(0.36f, 0.05f), new Vector2(0.64f, 0.29f),
            out Button musicBtn);

        Image vibeIcon = CreateSettingsToggle(content.transform, "VibeToggle",
            SpriteGenerator.CreateVibeIconSprite(), "VIBE",
            new Color(0.231f, 0.510f, 0.965f, 1f), // #3B82F6 jewel-blue
            new Vector2(0.64f, 0.05f), new Vector2(0.92f, 0.29f),
            out Button vibeBtn);

        // Wire PausePanel internal references
        SetPrivateField(pausePanel, "panel", overlay);
        SetPrivateField(pausePanel, "soundIcon", soundIcon);
        SetPrivateField(pausePanel, "musicIcon", musicIcon);
        SetPrivateField(pausePanel, "vibeIcon", vibeIcon);

        // Wire button onClick events
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            resumeBtn.GetComponent<Button>().onClick, pausePanel.OnResumeButton);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            restartPauseBtn.GetComponent<Button>().onClick, pausePanel.OnRestartButton);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            homeBtn.GetComponent<Button>().onClick, pausePanel.OnHomeButton);

        // Wire toggle onClick events
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            soundBtn.onClick, pausePanel.OnToggleSound);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            musicBtn.onClick, pausePanel.OnToggleMusic);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            vibeBtn.onClick, pausePanel.OnToggleVibe);

        // Start hidden
        overlay.SetActive(false);
    }

    // bg-slate-800 border border-slate-700 rounded-2xl with icon + text
    private static GameObject CreatePauseMenuButton(Transform parent, string name, string label,
        Sprite iconSprite, Color iconColor)
    {
        Color borderColor = new Color(0.200f, 0.255f, 0.333f, 1f); // #334155 slate-700
        Color bgColor = new Color(0.118f, 0.161f, 0.231f, 1f);     // #1E293B slate-800

        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Border frame
        Image borderImg = btnObj.AddComponent<Image>();
        borderImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        borderImg.type = Image.Type.Sliced;
        borderImg.color = borderColor;

        Button btn = btnObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;

        // Inner background (inset for visible border)
        GameObject bgObj = new GameObject("Bg");
        bgObj.transform.SetParent(btnObj.transform, false);
        RectTransform bgRT = bgObj.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = new Vector2(2f, 2f);
        bgRT.offsetMax = new Vector2(-2f, -2f);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        bgImg.type = Image.Type.Sliced;
        bgImg.color = bgColor;
        bgImg.raycastTarget = false;

        // Icon (left side, centered)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(btnObj.transform, false);
        RectTransform iconRT = iconObj.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.25f, 0.18f);
        iconRT.anchorMax = new Vector2(0.40f, 0.82f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;

        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.sprite = iconSprite;
        iconImg.color = iconColor;
        iconImg.raycastTarget = false;
        iconImg.preserveAspect = true;

        // Text (center-right)
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.38f, 0f);
        textRT.anchorMax = new Vector2(0.85f, 1f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.font = GetDefaultTMPFont();
        tmp.text = label;
        tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.characterSpacing = 6f;

        return btnObj;
    }

    // w-12 h-12 rounded-xl bg-slate-800 border-slate-700 + colored icon + label
    private static Image CreateSettingsToggle(Transform parent, string name, Sprite iconSprite,
        string label, Color iconColor, Vector2 anchorMin, Vector2 anchorMax, out Button button)
    {
        Color borderColor = new Color(0.200f, 0.255f, 0.333f, 1f); // #334155 slate-700
        Color bgColor = new Color(0.118f, 0.161f, 0.231f, 1f);     // #1E293B slate-800

        // Toggle button container
        GameObject toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent, false);
        RectTransform toggleRT = toggleObj.AddComponent<RectTransform>();
        toggleRT.anchorMin = anchorMin;
        toggleRT.anchorMax = anchorMax;
        toggleRT.offsetMin = Vector2.zero;
        toggleRT.offsetMax = Vector2.zero;

        Image toggleBg = toggleObj.AddComponent<Image>();
        toggleBg.color = Color.clear;
        toggleBg.raycastTarget = true;
        button = toggleObj.AddComponent<Button>();
        button.transition = Selectable.Transition.None;

        // Icon box border (slate-700)
        GameObject iconBorderObj = new GameObject("IconBorder");
        iconBorderObj.transform.SetParent(toggleObj.transform, false);
        RectTransform iconBorderRT = iconBorderObj.AddComponent<RectTransform>();
        iconBorderRT.anchorMin = new Vector2(0.15f, 0.30f);
        iconBorderRT.anchorMax = new Vector2(0.85f, 0.95f);
        iconBorderRT.offsetMin = Vector2.zero;
        iconBorderRT.offsetMax = Vector2.zero;

        Image iconBorderImg = iconBorderObj.AddComponent<Image>();
        iconBorderImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        iconBorderImg.type = Image.Type.Sliced;
        iconBorderImg.color = borderColor;

        // Icon box background (slate-800, inset for border)
        GameObject iconBgObj = new GameObject("IconBg");
        iconBgObj.transform.SetParent(iconBorderObj.transform, false);
        RectTransform iconBgRT = iconBgObj.AddComponent<RectTransform>();
        iconBgRT.anchorMin = Vector2.zero;
        iconBgRT.anchorMax = Vector2.one;
        iconBgRT.offsetMin = new Vector2(2f, 2f);
        iconBgRT.offsetMax = new Vector2(-2f, -2f);

        Image iconBgImg = iconBgObj.AddComponent<Image>();
        iconBgImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        iconBgImg.type = Image.Type.Sliced;
        iconBgImg.color = bgColor;

        // Icon sprite (colored, inside the dark bg)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(iconBgObj.transform, false);
        RectTransform iconRT = iconObj.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.15f, 0.15f);
        iconRT.anchorMax = new Vector2(0.85f, 0.85f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;

        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.sprite = iconSprite;
        iconImg.color = iconColor; // colored icon on dark bg
        iconImg.raycastTarget = false;
        iconImg.preserveAspect = true;

        // Label (text-[9px], slate-400)
        GameObject labelObj = CreateTextElement(toggleObj.transform, "Label", label,
            14, TextAlignmentOptions.Center, new Color(0.580f, 0.639f, 0.722f, 1f)); // #94A3B8 slate-400
        RectTransform labelRT = labelObj.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0f, 0f);
        labelRT.anchorMax = new Vector2(1f, 0.28f);
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;
        labelObj.GetComponent<TextMeshProUGUI>().characterSpacing = 2f;

        return iconImg; // Return icon Image for color toggling
    }

    // ============================================================
    // MAIN MENU (HTML referans: radyal gradient bg, accent orbs, neon title)
    // ============================================================
    private static void CreateMainMenu(Transform parent)
    {
        // --- MENU ROOT (tam ekran, radyal gradient arka plan) ---
        GameObject menuRoot = new GameObject("MainMenuPanel");
        menuRoot.transform.SetParent(parent, false);
        RectTransform menuRT = menuRoot.AddComponent<RectTransform>();
        menuRT.anchorMin = Vector2.zero;
        menuRT.anchorMax = Vector2.one;
        menuRT.offsetMin = Vector2.zero;
        menuRT.offsetMax = Vector2.zero;

        Image menuBg = menuRoot.AddComponent<Image>();
        menuBg.sprite = SpriteGenerator.CreateMenuBackgroundSprite();
        menuBg.type = Image.Type.Simple;
        menuBg.color = Color.white;
        menuBg.raycastTarget = true;

        MainMenuPanel menuPanel = menuRoot.AddComponent<MainMenuPanel>();

        // --- ACCENT ORB PURPLE (sol ust, buyuk, dusuk opaklik) ---
        GameObject orbPurple = new GameObject("AccentOrb_Purple");
        orbPurple.transform.SetParent(menuRoot.transform, false);
        RectTransform orbPurpleRT = orbPurple.AddComponent<RectTransform>();
        orbPurpleRT.anchorMin = new Vector2(-0.1f, 0.55f);
        orbPurpleRT.anchorMax = new Vector2(0.5f, 1.1f);
        orbPurpleRT.offsetMin = Vector2.zero;
        orbPurpleRT.offsetMax = Vector2.zero;
        Image orbPurpleImg = orbPurple.AddComponent<Image>();
        orbPurpleImg.sprite = SpriteGenerator.CreateGlowOrbSprite();
        orbPurpleImg.color = new Color(0.545f, 0.361f, 0.965f, 0.15f);
        orbPurpleImg.raycastTarget = false;

        // --- ACCENT ORB BLUE (sag alt, buyuk, dusuk opaklik) ---
        GameObject orbBlue = new GameObject("AccentOrb_Blue");
        orbBlue.transform.SetParent(menuRoot.transform, false);
        RectTransform orbBlueRT = orbBlue.AddComponent<RectTransform>();
        orbBlueRT.anchorMin = new Vector2(0.5f, -0.1f);
        orbBlueRT.anchorMax = new Vector2(1.1f, 0.45f);
        orbBlueRT.offsetMin = Vector2.zero;
        orbBlueRT.offsetMax = Vector2.zero;
        Image orbBlueImg = orbBlue.AddComponent<Image>();
        orbBlueImg.sprite = SpriteGenerator.CreateGlowOrbSprite();
        orbBlueImg.color = new Color(0.231f, 0.510f, 0.965f, 0.12f);
        orbBlueImg.raycastTarget = false;

        // --- LEVEL BADGE (sag ust, pill shape, slate-900/40 bg) ---
        GameObject levelBadge = new GameObject("LevelBadge");
        levelBadge.transform.SetParent(menuRoot.transform, false);
        RectTransform badgeRT = levelBadge.AddComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(0.55f, 0.91f);
        badgeRT.anchorMax = new Vector2(0.95f, 0.96f);
        badgeRT.offsetMin = Vector2.zero;
        badgeRT.offsetMax = Vector2.zero;
        Image badgeImg = levelBadge.AddComponent<Image>();
        badgeImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        badgeImg.type = Image.Type.Sliced;
        badgeImg.color = new Color(0.059f, 0.090f, 0.165f, 0.40f);

        // Star icon
        GameObject starIcon = new GameObject("StarIcon");
        starIcon.transform.SetParent(levelBadge.transform, false);
        RectTransform starRT = starIcon.AddComponent<RectTransform>();
        starRT.anchorMin = new Vector2(0.05f, 0.10f);
        starRT.anchorMax = new Vector2(0.25f, 0.90f);
        starRT.offsetMin = Vector2.zero;
        starRT.offsetMax = Vector2.zero;
        Image starImg = starIcon.AddComponent<Image>();
        starImg.sprite = SpriteGenerator.CreateStarSprite();
        starImg.color = new Color(0.231f, 0.510f, 0.965f, 1f); // jewel-blue
        starImg.raycastTarget = false;
        starImg.preserveAspect = true;

        // Level text
        GameObject levelTextObj = CreateTextElement(levelBadge.transform, "LevelText", "LEVEL 1",
            28, TextAlignmentOptions.Center, Color.white);
        RectTransform levelTextRT = levelTextObj.GetComponent<RectTransform>();
        levelTextRT.anchorMin = new Vector2(0.25f, 0f);
        levelTextRT.anchorMax = new Vector2(1f, 1f);
        levelTextRT.offsetMin = Vector2.zero;
        levelTextRT.offsetMax = Vector2.zero;
        levelTextObj.GetComponent<TextMeshProUGUI>().characterSpacing = 4f;

        // --- TITLE GLOW (mor glow efekti, ayni pozisyonda title altinda) ---
        GameObject titleGlow = CreateTextElement(menuRoot.transform, "TitleGlow", "COMBOOM",
            64, TextAlignmentOptions.Center, new Color(0.545f, 0.361f, 0.965f, 0.3f));
        RectTransform titleGlowRT = titleGlow.GetComponent<RectTransform>();
        titleGlowRT.anchorMin = new Vector2(0.05f, 0.60f);
        titleGlowRT.anchorMax = new Vector2(0.95f, 0.72f);
        titleGlowRT.offsetMin = Vector2.zero;
        titleGlowRT.offsetMax = Vector2.zero;
        titleGlow.GetComponent<TextMeshProUGUI>().characterSpacing = 16f;

        // --- TITLE TEXT "COMBOOM" ---
        GameObject titleText = CreateTextElement(menuRoot.transform, "TitleText", "COMBOOM",
            64, TextAlignmentOptions.Center, Color.white);
        RectTransform titleRT = titleText.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.05f, 0.60f);
        titleRT.anchorMax = new Vector2(0.95f, 0.72f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;
        titleText.GetComponent<TextMeshProUGUI>().characterSpacing = 16f;

        // --- PLAY BUTTON (buyuk, mor #8B5CF6, play ikonu + "PLAY") ---
        GameObject playBtn = new GameObject("PlayButton");
        playBtn.transform.SetParent(menuRoot.transform, false);
        RectTransform playBtnRT = playBtn.AddComponent<RectTransform>();
        playBtnRT.anchorMin = new Vector2(0.15f, 0.45f);
        playBtnRT.anchorMax = new Vector2(0.85f, 0.56f);
        playBtnRT.offsetMin = Vector2.zero;
        playBtnRT.offsetMax = Vector2.zero;

        Image playImg = playBtn.AddComponent<Image>();
        playImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        playImg.type = Image.Type.Sliced;
        playImg.color = new Color(0.545f, 0.361f, 0.965f, 1f); // #8B5CF6

        Button playButton = playBtn.AddComponent<Button>();
        var playColors = playButton.colors;
        playColors.normalColor = Color.white;
        playColors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
        playColors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        playButton.colors = playColors;

        // Play icon (sol taraf)
        GameObject playIcon = new GameObject("PlayIcon");
        playIcon.transform.SetParent(playBtn.transform, false);
        RectTransform playIconRT = playIcon.AddComponent<RectTransform>();
        playIconRT.anchorMin = new Vector2(0.28f, 0.15f);
        playIconRT.anchorMax = new Vector2(0.42f, 0.85f);
        playIconRT.offsetMin = Vector2.zero;
        playIconRT.offsetMax = Vector2.zero;
        Image playIconImg = playIcon.AddComponent<Image>();
        playIconImg.sprite = SpriteGenerator.CreatePlayArrowSprite();
        playIconImg.color = Color.white;
        playIconImg.raycastTarget = false;
        playIconImg.preserveAspect = true;

        // Play text (sag taraf)
        GameObject playText = CreateTextElement(playBtn.transform, "PlayText", "PLAY",
            44, TextAlignmentOptions.Center, Color.white);
        RectTransform playTextRT = playText.GetComponent<RectTransform>();
        playTextRT.anchorMin = new Vector2(0.40f, 0f);
        playTextRT.anchorMax = new Vector2(0.80f, 1f);
        playTextRT.offsetMin = Vector2.zero;
        playTextRT.offsetMax = Vector2.zero;
        playText.GetComponent<TextMeshProUGUI>().characterSpacing = 8f;
        AddLocKey(playText, "play");

        // --- BUTTON ROW (RANKS + SETTINGS) ---
        // Ranks button (sol)
        GameObject ranksBtn = CreateMenuSecondaryButton(menuRoot.transform, "RanksButton", "RANKS",
            SpriteGenerator.CreateLeaderboardSprite(),
            new Color(0.063f, 0.725f, 0.506f, 1f)); // #10B981 green
        RectTransform ranksRT = ranksBtn.GetComponent<RectTransform>();
        ranksRT.anchorMin = new Vector2(0.08f, 0.33f);
        ranksRT.anchorMax = new Vector2(0.48f, 0.42f);
        ranksRT.offsetMin = Vector2.zero;
        ranksRT.offsetMax = Vector2.zero;
        AddLocKey(ranksBtn.GetComponentInChildren<TextMeshProUGUI>().gameObject, "ranks");

        // Settings button (sag)
        GameObject settingsBtn = CreateMenuSecondaryButton(menuRoot.transform, "SettingsButton", "SETTINGS",
            SpriteGenerator.CreateSettingsGearSprite(),
            new Color(0.231f, 0.510f, 0.965f, 1f)); // #3B82F6 blue
        RectTransform settingsRT = settingsBtn.GetComponent<RectTransform>();
        settingsRT.anchorMin = new Vector2(0.52f, 0.33f);
        settingsRT.anchorMax = new Vector2(0.92f, 0.42f);
        settingsRT.offsetMin = Vector2.zero;
        settingsRT.offsetMax = Vector2.zero;
        AddLocKey(settingsBtn.GetComponentInChildren<TextMeshProUGUI>().gameObject, "settings");

        // --- BEST SCORE AREA (Banner üstünde, dinamik konumlandırma) ---
        // Sıralama: Banner (en alt) -> Score Value -> Score Label (üstte)

        // Best score value (skor sayısı) - Banner'ın üstünde
        GameObject bestValue = CreateTextElement(menuRoot.transform, "BestScoreValue", "0",
            48, TextAlignmentOptions.Center, new Color(0.961f, 0.620f, 0.043f, 1f));
        RectTransform bestValueRT = bestValue.GetComponent<RectTransform>();
        bestValueRT.anchorMin = new Vector2(0.1f, 0.06f);  // Banner alanının üstünden başla
        bestValueRT.anchorMax = new Vector2(0.9f, 0.11f);
        bestValueRT.offsetMin = Vector2.zero;
        bestValueRT.offsetMax = Vector2.zero;

        // Best score label ("EN İYİ SKOR") - Score value'nun üstünde
        GameObject bestLabel = CreateTextElement(menuRoot.transform, "BestScoreLabel", "BEST SCORE",
            18, TextAlignmentOptions.Center, new Color(0.6f, 0.6f, 0.65f, 1f)); // Gri renk
        RectTransform bestLabelRT = bestLabel.GetComponent<RectTransform>();
        bestLabelRT.anchorMin = new Vector2(0.1f, 0.11f);  // Score value'nun üstünden başla
        bestLabelRT.anchorMax = new Vector2(0.9f, 0.14f);
        bestLabelRT.offsetMin = Vector2.zero;
        bestLabelRT.offsetMax = Vector2.zero;
        bestLabel.GetComponent<TextMeshProUGUI>().characterSpacing = 4f;
        AddLocKey(bestLabel, "best_score");

        // Wire MainMenuPanel internal references
        SetPrivateField(menuPanel, "panel", menuRoot);
        SetPrivateField(menuPanel, "levelText", levelTextObj.GetComponent<TextMeshProUGUI>());
        SetPrivateField(menuPanel, "bestScoreText", bestValue.GetComponent<TextMeshProUGUI>());

        // Wire button onClick events
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            playButton.onClick, menuPanel.OnPlayButton);

        Button ranksButton = ranksBtn.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            ranksButton.onClick, menuPanel.OnRanksButton);

        Button settingsButton = settingsBtn.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            settingsButton.onClick, menuPanel.OnSettingsButton);

        // Baslangicta gizli (ShowMainMenu aktive edecek)
        menuRoot.SetActive(false);
    }

    // Ana menu ikincil buton (RANKS, SETTINGS - koyu bg, ikonlu)
    private static GameObject CreateMenuSecondaryButton(Transform parent, string name, string label,
        Sprite iconSprite, Color iconColor)
    {
        Color bgColor = new Color(0.059f, 0.090f, 0.165f, 0.70f);      // slate-900/70
        Color borderColor = new Color(0.200f, 0.255f, 0.333f, 0.50f);  // slate-700/50

        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image borderImg = btnObj.AddComponent<Image>();
        borderImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        borderImg.type = Image.Type.Sliced;
        borderImg.color = borderColor;

        Button btn = btnObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;

        // Inner background
        GameObject bgObj = new GameObject("Bg");
        bgObj.transform.SetParent(btnObj.transform, false);
        RectTransform bgRT = bgObj.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = new Vector2(2f, 2f);
        bgRT.offsetMax = new Vector2(-2f, -2f);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        bgImg.type = Image.Type.Sliced;
        bgImg.color = bgColor;
        bgImg.raycastTarget = false;

        // Icon (sol taraf)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(btnObj.transform, false);
        RectTransform iconRT = iconObj.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.10f, 0.15f);
        iconRT.anchorMax = new Vector2(0.35f, 0.85f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.sprite = iconSprite;
        iconImg.color = iconColor;
        iconImg.raycastTarget = false;
        iconImg.preserveAspect = true;

        // Label text (orta-sag)
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.30f, 0f);
        textRT.anchorMax = new Vector2(0.95f, 1f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.font = GetDefaultTMPFont();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.characterSpacing = 4f;

        return btnObj;
    }

    // Kirmizi yuvarlak badge (sag ust kose, sayi gosterir, cok haneli ise pill seklinde uzar)
    private static void CreateCountBadge(Transform parent, string name)
    {
        GameObject badge = new GameObject(name);
        badge.transform.SetParent(parent, false);
        RectTransform badgeRT = badge.AddComponent<RectTransform>();
        // Sag ust koseye sabit anchor
        badgeRT.anchorMin = new Vector2(1f, 1f);
        badgeRT.anchorMax = new Vector2(1f, 1f);
        badgeRT.pivot = new Vector2(0.5f, 0.5f);
        badgeRT.anchoredPosition = new Vector2(2f, 2f);
        badgeRT.sizeDelta = new Vector2(24f, 24f);

        Image badgeBg = badge.AddComponent<Image>();
        badgeBg.sprite = SpriteGenerator.CreateRoundedUISprite();
        badgeBg.type = Image.Type.Sliced;
        badgeBg.color = new Color(0.937f, 0.267f, 0.267f, 1f); // red #EF4444
        badgeBg.raycastTarget = false;

        // ContentSizeFitter ile icerige gore genisle (pill shape)
        HorizontalLayoutGroup hlg = badge.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(6, 6, 1, 1);
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        ContentSizeFitter csf = badge.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Badge text
        GameObject textObj = new GameObject("BadgeText");
        textObj.transform.SetParent(badge.transform, false);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.font = GetDefaultTMPFont();
        tmp.text = "3";
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = false;

        // Minimum boyut (tek hane icin yuvarlak kalsin)
        LayoutElement le = textObj.AddComponent<LayoutElement>();
        le.minWidth = 12;
        le.minHeight = 18;
    }

    // ============================================================
    // SETTINGS PANEL (tam ekran, gradient bg, header, scrollable settings rows)
    // ============================================================
    private static void CreateSettingsPanel(Transform parent)
    {
        // --- PANEL ROOT ---
        GameObject panelRoot = new GameObject("SettingsPanel");
        panelRoot.transform.SetParent(parent, false);
        RectTransform panelRT = panelRoot.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        Image panelBg = panelRoot.AddComponent<Image>();
        panelBg.sprite = SpriteGenerator.CreateMenuBackgroundSprite();
        panelBg.type = Image.Type.Simple;
        panelBg.color = Color.white;
        panelBg.raycastTarget = true;

        SettingsPanel settingsPanel = panelRoot.AddComponent<SettingsPanel>();

        // Accent orbs
        GameObject orbP = new GameObject("AccentOrb_Purple");
        orbP.transform.SetParent(panelRoot.transform, false);
        RectTransform orbPRT = orbP.AddComponent<RectTransform>();
        orbPRT.anchorMin = new Vector2(0.55f, 0.55f);
        orbPRT.anchorMax = new Vector2(1.15f, 1.05f);
        orbPRT.offsetMin = Vector2.zero; orbPRT.offsetMax = Vector2.zero;
        Image orbPImg = orbP.AddComponent<Image>();
        orbPImg.sprite = SpriteGenerator.CreateGlowOrbSprite();
        orbPImg.color = new Color(0.545f, 0.361f, 0.965f, 0.10f);
        orbPImg.raycastTarget = false;

        GameObject orbB = new GameObject("AccentOrb_Blue");
        orbB.transform.SetParent(panelRoot.transform, false);
        RectTransform orbBRT = orbB.AddComponent<RectTransform>();
        orbBRT.anchorMin = new Vector2(-0.15f, -0.05f);
        orbBRT.anchorMax = new Vector2(0.45f, 0.45f);
        orbBRT.offsetMin = Vector2.zero; orbBRT.offsetMax = Vector2.zero;
        Image orbBImg = orbB.AddComponent<Image>();
        orbBImg.sprite = SpriteGenerator.CreateGlowOrbSprite();
        orbBImg.color = new Color(0.231f, 0.510f, 0.965f, 0.10f);
        orbBImg.raycastTarget = false;

        // --- SAFE AREA ---
        GameObject safeArea = new GameObject("SafeArea");
        safeArea.transform.SetParent(panelRoot.transform, false);
        RectTransform safeRT = safeArea.AddComponent<RectTransform>();
        safeRT.anchorMin = Vector2.zero; safeRT.anchorMax = Vector2.one;
        safeRT.offsetMin = Vector2.zero; safeRT.offsetMax = Vector2.zero;
        safeArea.AddComponent<SafeAreaPanel>();

        // --- HEADER ---
        GameObject header = new GameObject("Header");
        header.transform.SetParent(safeArea.transform, false);
        RectTransform headerRT = header.AddComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0, 0.91f);
        headerRT.anchorMax = new Vector2(1, 1);
        headerRT.offsetMin = Vector2.zero; headerRT.offsetMax = Vector2.zero;
        Image headerBg = header.AddComponent<Image>();
        headerBg.color = new Color(0f, 0f, 0f, 0.20f);
        headerBg.raycastTarget = false;

        // Back button
        GameObject backBtn = new GameObject("BackButton");
        backBtn.transform.SetParent(header.transform, false);
        RectTransform backBtnRT = backBtn.AddComponent<RectTransform>();
        backBtnRT.anchorMin = new Vector2(0.03f, 0.18f);
        backBtnRT.anchorMax = new Vector2(0.12f, 0.82f);
        backBtnRT.offsetMin = Vector2.zero; backBtnRT.offsetMax = Vector2.zero;
        Image backBtnBg = backBtn.AddComponent<Image>();
        backBtnBg.sprite = SpriteGenerator.CreateRoundedUISprite();
        backBtnBg.type = Image.Type.Sliced;
        backBtnBg.color = new Color(1f, 1f, 1f, 0.05f);
        Button backButton = backBtn.AddComponent<Button>();
        backButton.transition = Selectable.Transition.None;

        GameObject backArrow = new GameObject("BackArrow");
        backArrow.transform.SetParent(backBtn.transform, false);
        RectTransform baRT = backArrow.AddComponent<RectTransform>();
        baRT.anchorMin = new Vector2(0.15f, 0.15f); baRT.anchorMax = new Vector2(0.85f, 0.85f);
        baRT.offsetMin = Vector2.zero; baRT.offsetMax = Vector2.zero;
        Image baImg = backArrow.AddComponent<Image>();
        baImg.sprite = SpriteGenerator.CreateBackArrowSprite();
        baImg.color = Color.white; baImg.raycastTarget = false; baImg.preserveAspect = true;

        // Title
        GameObject titleGlow = CreateTextElement(header.transform, "TitleGlow", "SETTINGS",
            48, TextAlignmentOptions.Center, new Color(0.545f, 0.361f, 0.965f, 0.3f));
        RectTransform tgRT = titleGlow.GetComponent<RectTransform>();
        tgRT.anchorMin = new Vector2(0.15f, 0f); tgRT.anchorMax = new Vector2(0.85f, 1f);
        tgRT.offsetMin = Vector2.zero; tgRT.offsetMax = Vector2.zero;
        titleGlow.GetComponent<TextMeshProUGUI>().characterSpacing = 10f;
        AddLocKey(titleGlow, "settings");

        GameObject titleText = CreateTextElement(header.transform, "TitleText", "SETTINGS",
            48, TextAlignmentOptions.Center, Color.white);
        RectTransform ttRT = titleText.GetComponent<RectTransform>();
        ttRT.anchorMin = new Vector2(0.15f, 0f); ttRT.anchorMax = new Vector2(0.85f, 1f);
        ttRT.offsetMin = Vector2.zero; ttRT.offsetMax = Vector2.zero;
        titleText.GetComponent<TextMeshProUGUI>().characterSpacing = 10f;
        AddLocKey(titleText, "settings");

        // --- SCROLL AREA ---
        GameObject scrollArea = new GameObject("ScrollArea");
        scrollArea.transform.SetParent(safeArea.transform, false);
        RectTransform scrollRT = scrollArea.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.04f, 0.02f);
        scrollRT.anchorMax = new Vector2(0.96f, 0.90f);
        scrollRT.offsetMin = Vector2.zero; scrollRT.offsetMax = Vector2.zero;

        ScrollRect scrollRect = scrollArea.AddComponent<ScrollRect>();
        scrollRect.horizontal = false; scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.03f;
        scrollRect.scrollSensitivity = 20f;
        Image scrollBg = scrollArea.AddComponent<Image>();
        scrollBg.color = Color.clear;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollArea.transform, false);
        RectTransform vpRT = viewport.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero; vpRT.offsetMax = Vector2.zero;
        Image vpImg = viewport.AddComponent<Image>();
        vpImg.color = Color.white; vpImg.raycastTarget = true;
        Mask vpMask = viewport.AddComponent<Mask>();
        vpMask.showMaskGraphic = false;
        scrollRect.viewport = vpRT;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.offsetMin = Vector2.zero; contentRT.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(6, 6, 14, 14);
        vlg.spacing = 14;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true; vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRT;

        Color borderColor = new Color(0.545f, 0.361f, 0.965f, 0.30f); // primary/30
        Color rowBgColor = new Color(0.059f, 0.090f, 0.165f, 0.40f);  // slate-900/40
        Color slateIcon = new Color(0.392f, 0.455f, 0.545f, 1f);      // slate-500
        Color purpleIcon = new Color(0.545f, 0.361f, 0.965f, 1f);     // primary

        // === SHARE ROW ===
        Button shareBtn;
        CreateSettingsRow(content.transform, "ShareRow", SpriteGenerator.CreateShareIconSprite(),
            slateIcon, "SHARE WITH FRIENDS", 32, out shareBtn, "share_friends");
        AddRowChevron(shareBtn.transform);

        // === SPACER ===
        CreateSpacer(content.transform, 10);

        // === SOUND ROW (toggle) ===
        Button soundBtn;
        CreateSettingsRow(content.transform, "SoundRow", SpriteGenerator.CreateSoundIconSprite(),
            purpleIcon, "SOUND", 36, out soundBtn, "sound");
        var (soundTrack, soundThumb) = AddRowToggle(soundBtn.transform);

        // === MUSIC ROW (toggle) ===
        Button musicBtn;
        CreateSettingsRow(content.transform, "MusicRow", SpriteGenerator.CreateMusicIconSprite(),
            purpleIcon, "MUSIC", 36, out musicBtn, "music");
        var (musicTrack, musicThumb) = AddRowToggle(musicBtn.transform);

        // === VIBE ROW (toggle) ===
        Button vibeBtn;
        CreateSettingsRow(content.transform, "VibeRow", SpriteGenerator.CreateVibeIconSprite(),
            purpleIcon, "VIBRATION", 36, out vibeBtn, "vibration");
        var (vibeTrack, vibeThumb) = AddRowToggle(vibeBtn.transform);

        // === SPACER ===
        CreateSpacer(content.transform, 18);

        // === LANGUAGE ROW ===
        Button langBtn;
        CreateSettingsRow(content.transform, "LanguageRow", SpriteGenerator.CreateGlobeIconSprite(),
            slateIcon, "LANGUAGE", 32, out langBtn, "language");
        TextMeshProUGUI langLabel = AddRowTextRight(langBtn.transform, "ENGLISH", purpleIcon);

        // === TERMS ROW ===
        Button termsBtn;
        CreateSettingsRow(content.transform, "TermsRow", SpriteGenerator.CreateDocumentIconSprite(),
            slateIcon, "TERMS OF SERVICE", 32, out termsBtn, "terms");
        AddRowExternalLink(termsBtn.transform);

        // === CONTACT ROW ===
        Button contactBtn;
        CreateSettingsRow(content.transform, "ContactRow", SpriteGenerator.CreateMailIconSprite(),
            slateIcon, "CONTACT US", 32, out contactBtn, "contact_us");
        AddRowChevron(contactBtn.transform);

        // --- WIRE REFERENCES ---
        SetPrivateField(settingsPanel, "panel", panelRoot);
        SetPrivateField(settingsPanel, "audioManager",
            Object.FindFirstObjectByType<AudioManager>());
        SetPrivateField(settingsPanel, "soundToggleTrack", soundTrack);
        SetPrivateField(settingsPanel, "soundToggleThumb", soundThumb);
        SetPrivateField(settingsPanel, "musicToggleTrack", musicTrack);
        SetPrivateField(settingsPanel, "musicToggleThumb", musicThumb);
        SetPrivateField(settingsPanel, "vibeToggleTrack", vibeTrack);
        SetPrivateField(settingsPanel, "vibeToggleThumb", vibeThumb);
        SetPrivateField(settingsPanel, "languageLabel", langLabel);

        // Wire button onClick events
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            backButton.onClick, settingsPanel.OnBackButton);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            soundBtn.onClick, settingsPanel.OnToggleSound);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            musicBtn.onClick, settingsPanel.OnToggleMusic);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            vibeBtn.onClick, settingsPanel.OnToggleVibe);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            shareBtn.onClick, settingsPanel.OnShareButton);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            langBtn.onClick, settingsPanel.OnLanguageButton);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            termsBtn.onClick, settingsPanel.OnTermsButton);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            contactBtn.onClick, settingsPanel.OnContactButton);

        panelRoot.SetActive(false);
    }

    // --- Settings row helper ---
    private static void CreateSettingsRow(Transform parent, string name, Sprite icon,
        Color iconColor, string label, int fontSize, out Button button, string locKey = null)
    {
        Color borderColor = new Color(0.545f, 0.361f, 0.965f, 0.30f);
        Color rowBgColor = new Color(0.059f, 0.090f, 0.165f, 0.40f);

        GameObject row = new GameObject(name);
        row.transform.SetParent(parent, false);
        RectTransform rowRT = row.AddComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(0, 110);

        // Border image
        Image borderImg = row.AddComponent<Image>();
        borderImg.sprite = SpriteGenerator.CreateRoundedUISprite();
        borderImg.type = Image.Type.Sliced;
        borderImg.color = borderColor;

        // Inner bg (inset 1px for border effect)
        GameObject inner = new GameObject("Inner");
        inner.transform.SetParent(row.transform, false);
        RectTransform innerRT = inner.AddComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero; innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(1, 1); innerRT.offsetMax = new Vector2(-1, -1);
        Image innerBg = inner.AddComponent<Image>();
        innerBg.sprite = SpriteGenerator.CreateRoundedUISprite();
        innerBg.type = Image.Type.Sliced;
        innerBg.color = rowBgColor;

        // Make button on whole row
        button = row.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = borderImg;

        // Layout
        HorizontalLayoutGroup hlg = inner.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(28, 28, 16, 16);
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(inner.transform, false);
        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.sprite = icon;
        iconImg.color = iconColor;
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;
        LayoutElement iconLE = iconObj.AddComponent<LayoutElement>();
        iconLE.minWidth = 48; iconLE.preferredWidth = 48;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(inner.transform, false);
        TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
        labelTMP.font = GetDefaultTMPFont();
        labelTMP.text = label;
        labelTMP.fontSize = fontSize;
        labelTMP.alignment = TextAlignmentOptions.Left;
        labelTMP.color = new Color(0.847f, 0.863f, 0.894f, 1f); // slate-200
        labelTMP.fontStyle = FontStyles.Bold;
        labelTMP.characterSpacing = 4f;
        LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
        labelLE.flexibleWidth = 1;

        if (!string.IsNullOrEmpty(locKey))
        {
            LocalizedText lt = labelObj.AddComponent<LocalizedText>();
            lt.localizationKey = locKey;
        }
    }

    // Toggle switch (iOS-style)
    private static (Image track, RectTransform thumb) AddRowToggle(Transform rowTransform)
    {
        Transform inner = rowTransform.Find("Inner");
        if (inner == null) inner = rowTransform;

        // Toggle container - pill shaped track
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(inner, false);
        LayoutElement toggleLE = toggleObj.AddComponent<LayoutElement>();
        toggleLE.minWidth = 82; toggleLE.preferredWidth = 82;
        toggleLE.minHeight = 48; toggleLE.preferredHeight = 48;

        // Track - pill shaped capsule
        Image trackImg = toggleObj.AddComponent<Image>();
        trackImg.sprite = SpriteGenerator.CreateTogglePillSprite();
        trackImg.type = Image.Type.Sliced;
        trackImg.color = new Color(0.545f, 0.361f, 0.965f, 1f); // purple, default ON
        trackImg.raycastTarget = false;

        // Thumb container (holds glow + circle together, moves as one unit)
        GameObject thumbObj = new GameObject("Thumb");
        thumbObj.transform.SetParent(toggleObj.transform, false);
        RectTransform thumbRT = thumbObj.AddComponent<RectTransform>();
        thumbRT.anchorMin = new Vector2(0.5f, 0.5f);
        thumbRT.anchorMax = new Vector2(0.5f, 0.5f);
        thumbRT.pivot = new Vector2(0.5f, 0.5f);
        thumbRT.sizeDelta = new Vector2(40, 40);
        thumbRT.anchoredPosition = new Vector2(18, 0); // default ON (right side)

        // Purple glow behind thumb (child of thumb so it moves together)
        GameObject glowObj = new GameObject("ThumbGlow");
        glowObj.transform.SetParent(thumbObj.transform, false);
        RectTransform glowRT = glowObj.AddComponent<RectTransform>();
        glowRT.anchorMin = new Vector2(0.5f, 0.5f);
        glowRT.anchorMax = new Vector2(0.5f, 0.5f);
        glowRT.pivot = new Vector2(0.5f, 0.5f);
        glowRT.sizeDelta = new Vector2(66, 66); // larger than thumb for glow spread
        glowRT.anchoredPosition = Vector2.zero;

        Image glowImg = glowObj.AddComponent<Image>();
        glowImg.sprite = SpriteGenerator.CreateGlowOrbSprite();
        glowImg.color = new Color(0.545f, 0.361f, 0.965f, 0.55f); // purple glow
        glowImg.raycastTarget = false;

        // White thumb circle (on top of glow)
        GameObject circleObj = new GameObject("ThumbCircle");
        circleObj.transform.SetParent(thumbObj.transform, false);
        RectTransform circleRT = circleObj.AddComponent<RectTransform>();
        circleRT.anchorMin = new Vector2(0.5f, 0.5f);
        circleRT.anchorMax = new Vector2(0.5f, 0.5f);
        circleRT.pivot = new Vector2(0.5f, 0.5f);
        circleRT.sizeDelta = new Vector2(36, 36);
        circleRT.anchoredPosition = Vector2.zero;

        Image circleImg = circleObj.AddComponent<Image>();
        circleImg.sprite = SpriteGenerator.CreateAvatarPlaceholderSprite();
        circleImg.color = Color.white;
        circleImg.raycastTarget = false;

        return (trackImg, thumbRT);
    }

    // Chevron right for navigation rows
    private static void AddRowChevron(Transform rowTransform)
    {
        Transform inner = rowTransform.Find("Inner");
        if (inner == null) inner = rowTransform;

        GameObject chevObj = new GameObject("Chevron");
        chevObj.transform.SetParent(inner, false);
        Image chevImg = chevObj.AddComponent<Image>();
        chevImg.sprite = SpriteGenerator.CreateChevronRightSprite();
        chevImg.color = new Color(0.392f, 0.455f, 0.545f, 0.6f); // slate-500/60
        chevImg.preserveAspect = true;
        chevImg.raycastTarget = false;
        LayoutElement chevLE = chevObj.AddComponent<LayoutElement>();
        chevLE.minWidth = 32; chevLE.preferredWidth = 32;
    }

    // External link icon
    private static void AddRowExternalLink(Transform rowTransform)
    {
        Transform inner = rowTransform.Find("Inner");
        if (inner == null) inner = rowTransform;

        GameObject extObj = new GameObject("ExternalLink");
        extObj.transform.SetParent(inner, false);
        Image extImg = extObj.AddComponent<Image>();
        extImg.sprite = SpriteGenerator.CreateExternalLinkSprite();
        extImg.color = new Color(0.392f, 0.455f, 0.545f, 0.6f);
        extImg.preserveAspect = true;
        extImg.raycastTarget = false;
        LayoutElement extLE = extObj.AddComponent<LayoutElement>();
        extLE.minWidth = 32; extLE.preferredWidth = 32;
    }

    // Right-side text label (e.g. "ENGLISH")
    private static TextMeshProUGUI AddRowTextRight(Transform rowTransform, string text, Color color)
    {
        Transform inner = rowTransform.Find("Inner");
        if (inner == null) inner = rowTransform;

        GameObject textObj = new GameObject("RightLabel");
        textObj.transform.SetParent(inner, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.font = GetDefaultTMPFont();
        tmp.text = text;
        tmp.fontSize = 30;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = false;
        LayoutElement le = textObj.AddComponent<LayoutElement>();
        le.minWidth = 150; le.preferredWidth = 170;
        return tmp;
    }

    // Spacer for visual separation between groups
    private static void CreateSpacer(Transform parent, float height)
    {
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(parent, false);
        RectTransform spacerRT = spacer.AddComponent<RectTransform>();
        spacerRT.sizeDelta = new Vector2(0, height);
    }

    // Localization helper - adds LocalizedText component to a text GameObject
    private static void AddLocKey(GameObject textObj, string key)
    {
        LocalizedText lt = textObj.AddComponent<LocalizedText>();
        lt.localizationKey = key;
    }

    // ============================================================
    // RANKS PANEL (tam ekran, gradient bg, header, scrollable liste, player bar)
    // ============================================================
    private static void CreateRanksPanel(Transform parent)
    {
        // --- PANEL ROOT (tam ekran, radyal gradient arka plan) ---
        GameObject panelRoot = new GameObject("RanksPanel");
        panelRoot.transform.SetParent(parent, false);
        RectTransform panelRT = panelRoot.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        Image panelBg = panelRoot.AddComponent<Image>();
        panelBg.sprite = SpriteGenerator.CreateMenuBackgroundSprite();
        panelBg.type = Image.Type.Simple;
        panelBg.color = Color.white;
        panelBg.raycastTarget = true;

        RanksPanel ranksPanel = panelRoot.AddComponent<RanksPanel>();

        // --- ACCENT ORB PURPLE ---
        GameObject orbPurple = new GameObject("AccentOrb_Purple");
        orbPurple.transform.SetParent(panelRoot.transform, false);
        RectTransform orbPurpleRT = orbPurple.AddComponent<RectTransform>();
        orbPurpleRT.anchorMin = new Vector2(-0.15f, 0.6f);
        orbPurpleRT.anchorMax = new Vector2(0.45f, 1.1f);
        orbPurpleRT.offsetMin = Vector2.zero;
        orbPurpleRT.offsetMax = Vector2.zero;
        Image orbPurpleImg = orbPurple.AddComponent<Image>();
        orbPurpleImg.sprite = SpriteGenerator.CreateGlowOrbSprite();
        orbPurpleImg.color = new Color(0.545f, 0.361f, 0.965f, 0.12f);
        orbPurpleImg.raycastTarget = false;

        // --- ACCENT ORB BLUE ---
        GameObject orbBlue = new GameObject("AccentOrb_Blue");
        orbBlue.transform.SetParent(panelRoot.transform, false);
        RectTransform orbBlueRT = orbBlue.AddComponent<RectTransform>();
        orbBlueRT.anchorMin = new Vector2(0.55f, -0.1f);
        orbBlueRT.anchorMax = new Vector2(1.15f, 0.4f);
        orbBlueRT.offsetMin = Vector2.zero;
        orbBlueRT.offsetMax = Vector2.zero;
        Image orbBlueImg = orbBlue.AddComponent<Image>();
        orbBlueImg.sprite = SpriteGenerator.CreateGlowOrbSprite();
        orbBlueImg.color = new Color(0.231f, 0.510f, 0.965f, 0.10f);
        orbBlueImg.raycastTarget = false;

        // --- SAFE AREA CONTAINER (notch + home indicator icin) ---
        GameObject safeArea = new GameObject("SafeArea");
        safeArea.transform.SetParent(panelRoot.transform, false);
        RectTransform safeAreaRT = safeArea.AddComponent<RectTransform>();
        safeAreaRT.anchorMin = Vector2.zero;
        safeAreaRT.anchorMax = Vector2.one;
        safeAreaRT.offsetMin = Vector2.zero;
        safeAreaRT.offsetMax = Vector2.zero;
        safeArea.AddComponent<SafeAreaPanel>();

        // --- HEADER (ust kisim: geri butonu + RANKS baslik) ---
        GameObject header = new GameObject("Header");
        header.transform.SetParent(safeArea.transform, false);
        RectTransform headerRT = header.AddComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0, 0.91f);
        headerRT.anchorMax = new Vector2(1, 1);
        headerRT.offsetMin = Vector2.zero;
        headerRT.offsetMax = Vector2.zero;
        Image headerBg = header.AddComponent<Image>();
        headerBg.color = new Color(0f, 0f, 0f, 0.20f);
        headerBg.raycastTarget = false;

        // Back button (sol, yuvarlak)
        GameObject backBtn = new GameObject("BackButton");
        backBtn.transform.SetParent(header.transform, false);
        RectTransform backBtnRT = backBtn.AddComponent<RectTransform>();
        backBtnRT.anchorMin = new Vector2(0.03f, 0.18f);
        backBtnRT.anchorMax = new Vector2(0.12f, 0.82f);
        backBtnRT.offsetMin = Vector2.zero;
        backBtnRT.offsetMax = Vector2.zero;

        Image backBtnBg = backBtn.AddComponent<Image>();
        backBtnBg.sprite = SpriteGenerator.CreateRoundedUISprite();
        backBtnBg.type = Image.Type.Sliced;
        backBtnBg.color = new Color(1f, 1f, 1f, 0.05f); // white/5

        Button backButton = backBtn.AddComponent<Button>();
        backButton.transition = Selectable.Transition.None;

        // Back arrow icon
        GameObject backArrow = new GameObject("BackArrow");
        backArrow.transform.SetParent(backBtn.transform, false);
        RectTransform backArrowRT = backArrow.AddComponent<RectTransform>();
        backArrowRT.anchorMin = new Vector2(0.15f, 0.15f);
        backArrowRT.anchorMax = new Vector2(0.85f, 0.85f);
        backArrowRT.offsetMin = Vector2.zero;
        backArrowRT.offsetMax = Vector2.zero;
        Image backArrowImg = backArrow.AddComponent<Image>();
        backArrowImg.sprite = SpriteGenerator.CreateBackArrowSprite();
        backArrowImg.color = Color.white;
        backArrowImg.raycastTarget = false;
        backArrowImg.preserveAspect = true;

        // Title "RANKS"
        GameObject titleGlow = CreateTextElement(header.transform, "TitleGlow", "RANKS",
            48, TextAlignmentOptions.Center, new Color(0.545f, 0.361f, 0.965f, 0.3f));
        RectTransform titleGlowRT = titleGlow.GetComponent<RectTransform>();
        titleGlowRT.anchorMin = new Vector2(0.15f, 0f);
        titleGlowRT.anchorMax = new Vector2(0.85f, 1f);
        titleGlowRT.offsetMin = Vector2.zero;
        titleGlowRT.offsetMax = Vector2.zero;
        titleGlow.GetComponent<TextMeshProUGUI>().characterSpacing = 12f;
        AddLocKey(titleGlow, "ranks");

        GameObject titleText = CreateTextElement(header.transform, "TitleText", "RANKS",
            48, TextAlignmentOptions.Center, Color.white);
        RectTransform titleRT = titleText.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.15f, 0f);
        titleRT.anchorMax = new Vector2(0.85f, 1f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;
        titleText.GetComponent<TextMeshProUGUI>().characterSpacing = 12f;
        AddLocKey(titleText, "ranks");

        // --- USER SCORE BAR (Banner üstünde sabit) ---
        GameObject userScoreBar = new GameObject("UserScoreBar");
        userScoreBar.transform.SetParent(safeArea.transform, false);
        RectTransform userScoreBarRT = userScoreBar.AddComponent<RectTransform>();
        userScoreBarRT.anchorMin = new Vector2(0f, 0.06f);   // Banner üstünde
        userScoreBarRT.anchorMax = new Vector2(1f, 0.13f);   // %7 yükseklik
        userScoreBarRT.offsetMin = Vector2.zero;
        userScoreBarRT.offsetMax = Vector2.zero;

        Image userScoreBarBg = userScoreBar.AddComponent<Image>();
        userScoreBarBg.color = new Color(0.545f, 0.361f, 0.965f, 0.25f); // Mor vurgulu arka plan

        // User rank (#42)
        GameObject userRankObj = CreateTextElement(userScoreBar.transform, "UserRank", "#--",
            24, TextAlignmentOptions.Left, new Color(0.7f, 0.7f, 0.75f, 1f));
        RectTransform userRankRT = userRankObj.GetComponent<RectTransform>();
        userRankRT.anchorMin = new Vector2(0.04f, 0f);
        userRankRT.anchorMax = new Vector2(0.15f, 1f);
        userRankRT.offsetMin = Vector2.zero;
        userRankRT.offsetMax = Vector2.zero;

        // User name ("Sen" / "You")
        GameObject userNameObj = CreateTextElement(userScoreBar.transform, "UserName", "Sen",
            26, TextAlignmentOptions.Left, Color.white);
        RectTransform userNameRT = userNameObj.GetComponent<RectTransform>();
        userNameRT.anchorMin = new Vector2(0.16f, 0f);
        userNameRT.anchorMax = new Vector2(0.55f, 1f);
        userNameRT.offsetMin = Vector2.zero;
        userNameRT.offsetMax = Vector2.zero;
        userNameObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        AddLocKey(userNameObj, "you");

        // User score
        GameObject userScoreObj = CreateTextElement(userScoreBar.transform, "UserScore", "0",
            26, TextAlignmentOptions.Right, new Color(0.545f, 0.361f, 0.965f, 1f)); // Mor
        RectTransform userScoreRT = userScoreObj.GetComponent<RectTransform>();
        userScoreRT.anchorMin = new Vector2(0.55f, 0f);
        userScoreRT.anchorMax = new Vector2(0.96f, 1f);
        userScoreRT.offsetMin = Vector2.zero;
        userScoreRT.offsetMax = Vector2.zero;
        userScoreObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // --- SCROLL AREA (UserScoreBar üstünde) ---
        GameObject scrollArea = new GameObject("ScrollArea");
        scrollArea.transform.SetParent(safeArea.transform, false);
        RectTransform scrollAreaRT = scrollArea.AddComponent<RectTransform>();
        scrollAreaRT.anchorMin = new Vector2(0.03f, 0.14f);  // UserScoreBar üstünden başla
        scrollAreaRT.anchorMax = new Vector2(0.97f, 0.90f);
        scrollAreaRT.offsetMin = Vector2.zero;
        scrollAreaRT.offsetMax = Vector2.zero;

        ScrollRect scrollRect = scrollArea.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.03f;
        scrollRect.scrollSensitivity = 20f;

        Image scrollBg = scrollArea.AddComponent<Image>();
        scrollBg.color = Color.clear;

        // Viewport (maskeli alan)
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollArea.transform, false);
        RectTransform viewportRT = viewport.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = Vector2.zero;

        Image viewportImg = viewport.AddComponent<Image>();
        viewportImg.color = Color.white;
        viewportImg.raycastTarget = true;
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        scrollRect.viewport = viewportRT;

        // Content (VerticalLayoutGroup)
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(8, 8, 10, 10);
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter contentCSF = content.AddComponent<ContentSizeFitter>();
        contentCSF.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRT;

        // Wire RanksPanel internal references (UserScoreBar elementlerini kullan)
        TextMeshProUGUI userRankTMP = userRankObj.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI userScoreTMP = userScoreObj.GetComponent<TextMeshProUGUI>();

        SetPrivateField(ranksPanel, "panel", panelRoot);
        SetPrivateField(ranksPanel, "contentParent", content.transform);
        SetPrivateField(ranksPanel, "playerRankText", userRankTMP);
        SetPrivateField(ranksPanel, "playerScoreText", userScoreTMP);

        // Wire back button onClick
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            backButton.onClick, ranksPanel.OnBackButton);

        // Start hidden
        panelRoot.SetActive(false);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var so = new SerializedObject(target as UnityEngine.Object);
        var prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogWarning($"[ComBoom] '{fieldName}' alani bulunamadi: {target.GetType().Name}");
            return;
        }

        switch (value)
        {
            case UnityEngine.Object unityObj:
                prop.objectReferenceValue = unityObj;
                break;
            case Transform[] transforms:
                prop.arraySize = transforms.Length;
                for (int i = 0; i < transforms.Length; i++)
                {
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = transforms[i];
                }
                break;
        }

        so.ApplyModifiedProperties();
    }

    // ============================================================
    // AD MANAGER
    // ============================================================
    private static GameObject CreateAdManager()
    {
        GameObject adManagerObj = new GameObject("AdManager");
        AdManager adMgr = adManagerObj.AddComponent<AdManager>();

        // AdConfig ScriptableObject olustur veya mevcut olani bul
        string configPath = "Assets/AdConfig.asset";
        AdConfig config = AssetDatabase.LoadAssetAtPath<AdConfig>(configPath);

        if (config == null)
        {
            config = ScriptableObject.CreateInstance<AdConfig>();
            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[ComBoom] AdConfig.asset olusturuldu: " + configPath);
        }

        // AdManager'a config'i bagla
        SetPrivateField(adMgr, "config", config);

        return adManagerObj;
    }

    // ============================================================
    // CONTINUE PANEL
    // ============================================================
    private static void CreateContinuePanel(Transform canvasTransform)
    {
        Transform safeArea = canvasTransform.Find("SafeAreaPanel");
        if (safeArea == null) return;

        // Panel root
        GameObject panelRoot = new GameObject("ContinuePanel");
        panelRoot.transform.SetParent(safeArea, false);
        RectTransform panelRT = panelRoot.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // Background overlay
        Image bgImage = panelRoot.AddComponent<Image>();
        bgImage.color = new Color(0.02f, 0.03f, 0.06f, 0.95f);

        // CanvasGroup for fade
        CanvasGroup cg = panelRoot.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        // ContinuePanel script
        ContinuePanel continuePanel = panelRoot.AddComponent<ContinuePanel>();

        // Content container
        GameObject content = new GameObject("Content");
        content.transform.SetParent(panelRoot.transform, false);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.5f, 0.5f);
        contentRT.anchorMax = new Vector2(0.5f, 0.5f);
        contentRT.sizeDelta = new Vector2(600, 500);

        // Title: "OYUN BITTI!"
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(content.transform, false);
        RectTransform titleRT = titleObj.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0, -50);
        titleRT.sizeDelta = new Vector2(500, 60);
        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "OYUN BITTI!";
        titleTMP.fontSize = 48;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;

        // Score text
        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(content.transform, false);
        RectTransform scoreRT = scoreObj.AddComponent<RectTransform>();
        scoreRT.anchorMin = new Vector2(0.5f, 1f);
        scoreRT.anchorMax = new Vector2(0.5f, 1f);
        scoreRT.anchoredPosition = new Vector2(0, -120);
        scoreRT.sizeDelta = new Vector2(400, 50);
        TextMeshProUGUI scoreTMP = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreTMP.text = "12,450";
        scoreTMP.fontSize = 42;
        scoreTMP.fontStyle = FontStyles.Bold;
        scoreTMP.alignment = TextAlignmentOptions.Center;
        scoreTMP.color = new Color(0.976f, 0.773f, 0.043f); // Gold

        // Countdown text
        GameObject countdownObj = new GameObject("CountdownText");
        countdownObj.transform.SetParent(content.transform, false);
        RectTransform countdownRT = countdownObj.AddComponent<RectTransform>();
        countdownRT.anchorMin = new Vector2(0.5f, 0.5f);
        countdownRT.anchorMax = new Vector2(0.5f, 0.5f);
        countdownRT.anchoredPosition = new Vector2(0, 30);
        countdownRT.sizeDelta = new Vector2(100, 80);
        TextMeshProUGUI countdownTMP = countdownObj.AddComponent<TextMeshProUGUI>();
        countdownTMP.text = "5";
        countdownTMP.fontSize = 64;
        countdownTMP.fontStyle = FontStyles.Bold;
        countdownTMP.alignment = TextAlignmentOptions.Center;
        countdownTMP.color = Color.white;

        // Continue Button
        GameObject continueBtn = new GameObject("ContinueButton");
        continueBtn.transform.SetParent(content.transform, false);
        RectTransform continueBtnRT = continueBtn.AddComponent<RectTransform>();
        continueBtnRT.anchorMin = new Vector2(0.5f, 0.5f);
        continueBtnRT.anchorMax = new Vector2(0.5f, 0.5f);
        continueBtnRT.anchoredPosition = new Vector2(0, -60);
        continueBtnRT.sizeDelta = new Vector2(450, 80);

        Image continueBtnImg = continueBtn.AddComponent<Image>();
        continueBtnImg.color = new Color(0.063f, 0.725f, 0.506f); // Green

        Button continueBtnComp = continueBtn.AddComponent<Button>();
        continueBtnComp.targetGraphic = continueBtnImg;

        // Continue button text
        GameObject continueBtnTextObj = new GameObject("Text");
        continueBtnTextObj.transform.SetParent(continueBtn.transform, false);
        RectTransform continueBtnTextRT = continueBtnTextObj.AddComponent<RectTransform>();
        continueBtnTextRT.anchorMin = Vector2.zero;
        continueBtnTextRT.anchorMax = Vector2.one;
        continueBtnTextRT.offsetMin = new Vector2(10, 5);
        continueBtnTextRT.offsetMax = new Vector2(-10, -5);
        TextMeshProUGUI continueBtnTMP = continueBtnTextObj.AddComponent<TextMeshProUGUI>();
        continueBtnTMP.text = "REKLAM IZLE VE DEVAM ET";
        continueBtnTMP.fontSize = 24;
        continueBtnTMP.fontStyle = FontStyles.Bold;
        continueBtnTMP.alignment = TextAlignmentOptions.Center;
        continueBtnTMP.color = Color.white;

        // Continue description
        GameObject descObj = new GameObject("DescText");
        descObj.transform.SetParent(content.transform, false);
        RectTransform descRT = descObj.AddComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0.5f, 0.5f);
        descRT.anchorMax = new Vector2(0.5f, 0.5f);
        descRT.anchoredPosition = new Vector2(0, -115);
        descRT.sizeDelta = new Vector2(400, 30);
        TextMeshProUGUI descTMP = descObj.AddComponent<TextMeshProUGUI>();
        descTMP.text = "2 satir temizle ve oynamaya devam et";
        descTMP.fontSize = 16;
        descTMP.alignment = TextAlignmentOptions.Center;
        descTMP.color = new Color(0.6f, 0.6f, 0.7f);

        // Skip Button
        GameObject skipBtn = new GameObject("SkipButton");
        skipBtn.transform.SetParent(content.transform, false);
        RectTransform skipBtnRT = skipBtn.AddComponent<RectTransform>();
        skipBtnRT.anchorMin = new Vector2(0.5f, 0f);
        skipBtnRT.anchorMax = new Vector2(0.5f, 0f);
        skipBtnRT.anchoredPosition = new Vector2(0, 60);
        skipBtnRT.sizeDelta = new Vector2(200, 50);

        Image skipBtnImg = skipBtn.AddComponent<Image>();
        skipBtnImg.color = new Color(0.3f, 0.3f, 0.35f, 0.8f);

        Button skipBtnComp = skipBtn.AddComponent<Button>();
        skipBtnComp.targetGraphic = skipBtnImg;

        // Skip button text
        GameObject skipBtnTextObj = new GameObject("Text");
        skipBtnTextObj.transform.SetParent(skipBtn.transform, false);
        RectTransform skipBtnTextRT = skipBtnTextObj.AddComponent<RectTransform>();
        skipBtnTextRT.anchorMin = Vector2.zero;
        skipBtnTextRT.anchorMax = Vector2.one;
        skipBtnTextRT.offsetMin = Vector2.zero;
        skipBtnTextRT.offsetMax = Vector2.zero;
        TextMeshProUGUI skipBtnTMP = skipBtnTextObj.AddComponent<TextMeshProUGUI>();
        skipBtnTMP.text = "VAZGEC";
        skipBtnTMP.fontSize = 20;
        skipBtnTMP.alignment = TextAlignmentOptions.Center;
        skipBtnTMP.color = Color.white;

        // Wire button events
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            continueBtnComp.onClick, continuePanel.OnContinueButton);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            skipBtnComp.onClick, continuePanel.OnSkipButton);

        // Set references via SerializedObject
        SetPrivateField(continuePanel, "panel", panelRoot);
        SetPrivateField(continuePanel, "canvasGroup", cg);
        SetPrivateField(continuePanel, "titleText", titleTMP);
        SetPrivateField(continuePanel, "scoreText", scoreTMP);
        SetPrivateField(continuePanel, "countdownText", countdownTMP);
        SetPrivateField(continuePanel, "continueButtonText", continueBtnTMP);
        SetPrivateField(continuePanel, "continueDescText", descTMP);
        SetPrivateField(continuePanel, "continueButton", continueBtnComp);
        SetPrivateField(continuePanel, "skipButton", skipBtnComp);

        // Start hidden
        panelRoot.SetActive(false);

        Debug.Log("[ComBoom] ContinuePanel olusturuldu");
    }
}
