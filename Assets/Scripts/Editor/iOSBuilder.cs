using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.Callbacks;
using ComBoom.Gameplay;

public class iOSBuilder : EditorWindow
{
    [MenuItem("ComBoom/Build iOS")]
    public static void BuildiOS()
    {
        // Aktif sahneyi KAYDET (en kritik adim!)
        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.isDirty)
        {
            EditorSceneManager.SaveScene(currentScene);
            Debug.Log($"[ComBoom] Sahne kaydedildi: {currentScene.path}");
        }
        else
        {
            // Dirty degilse bile zorla kaydet
            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorSceneManager.SaveScene(currentScene);
            Debug.Log($"[ComBoom] Sahne zorla kaydedildi: {currentScene.path}");
        }

        // Sahne kontrolu
        string[] scenes = GetBuildScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError("[ComBoom] Build Settings'te sahne yok! Lutfen sahnenizi Build Settings > Scenes In Build'e ekleyin.");
            return;
        }

        // Player ayarlarini guncelle
        PlayerSettings.productName = "ComBoom";
        PlayerSettings.companyName = "M3Studios";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.m3studios.comboom");

        // iOS ozel ayarlar
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.targetOSVersionString = "15.0";
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        PlayerSettings.iOS.hideHomeButton = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        // Unity splash screen'i kapat (Made with Unity)
        PlayerSettings.SplashScreen.show = false;

        // Build baslatilsin mi?
        if (!EditorUtility.DisplayDialog("ComBoom iOS Build",
            $"iOS build baslatilacak.\n\nBundle ID: com.m3studios.comboom\nSahneler: {scenes.Length} sahne\nHedef: Builds/ klasoru\n\nDevam?",
            "Build", "Iptal"))
        {
            return;
        }

        string buildPath = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName, "Builds", "iOS");

        // Temiz build icin eski klasoru sil
        if (System.IO.Directory.Exists(buildPath))
            System.IO.Directory.Delete(buildPath, true);
        System.IO.Directory.CreateDirectory(buildPath);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        // App icon olustur ve ata
        SetupAppIcon();

        Debug.Log($"[ComBoom] iOS build baslatiliyor... Hedef: {buildPath}");

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            string xcodeProjPath = System.IO.Path.Combine(buildPath, "Unity-iPhone.xcodeproj");
            Debug.Log($"[ComBoom] iOS build BASARILI! Boyut: {summary.totalSize / (1024 * 1024):F1} MB");
            Debug.Log($"[ComBoom] Xcode aciliyor: {xcodeProjPath}");

            // Xcode'u otomatik ac
            System.Diagnostics.Process.Start("open", xcodeProjPath);
        }
        else
        {
            Debug.LogError($"[ComBoom] iOS build BASARISIZ! Hata sayisi: {summary.totalErrors}");
        }
    }

    [MenuItem("ComBoom/Generate App Icon")]
    public static void SetupAppIcon()
    {
        Debug.Log("[ComBoom] App icon olusturuluyor...");

        // Icon texture olustur (1024x1024)
        Texture2D iconTex = SpriteGenerator.CreateAppIconTexture();

        // PNG olarak kaydet
        string iconPath = "Assets/AppIcon.png";
        string fullPath = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName, iconPath);

        byte[] pngData = iconTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(fullPath, pngData);
        AssetDatabase.ImportAsset(iconPath, ImportAssetOptions.ForceUpdate);

        // TextureImporter ayarlari
        TextureImporter importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 1024;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        // iOS icon boyutlari
        Texture2D importedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
        if (importedTex == null)
        {
            Debug.LogError("[ComBoom] Icon texture yuklenemedi!");
            return;
        }

        // iOS platform icon'larini ayarla
        var platform = BuildTargetGroup.iOS;
        var kind = UnityEditor.iOS.iOSPlatformIconKind.Application;
        var icons = PlayerSettings.GetPlatformIcons(platform, kind);

        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].SetTexture(importedTex, 0);
        }
        PlayerSettings.SetPlatformIcons(platform, kind, icons);

        Debug.Log($"[ComBoom] App icon basariyla olusturuldu ve atandi: {iconPath} ({icons.Length} boyut)");

        // Bellek temizle
        if (Application.isPlaying)
            Object.Destroy(iconTex);
        else
            Object.DestroyImmediate(iconTex);
    }

    [PostProcessBuild(0)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) return;

        string appIconSetPath = System.IO.Path.Combine(
            pathToBuiltProject, "Unity-iPhone", "Images.xcassets", "AppIcon.appiconset");

        if (!System.IO.Directory.Exists(appIconSetPath))
        {
            Debug.LogWarning("[ComBoom] AppIcon.appiconset bulunamadi, 1024x1024 icon eklenemedi.");
            return;
        }

        // 1024x1024 icon'u kopyala
        string srcIcon = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName, "Assets", "AppIcon.png");
        string dstIcon = System.IO.Path.Combine(appIconSetPath, "Icon-AppStore-1024.png");

        if (System.IO.File.Exists(srcIcon))
        {
            System.IO.File.Copy(srcIcon, dstIcon, true);
        }
        else
        {
            Debug.LogWarning("[ComBoom] Assets/AppIcon.png bulunamadi! Once ComBoom > Generate App Icon calistirin.");
            return;
        }

        // Contents.json'a ios-marketing ekle
        string contentsPath = System.IO.Path.Combine(appIconSetPath, "Contents.json");
        if (System.IO.File.Exists(contentsPath))
        {
            string json = System.IO.File.ReadAllText(contentsPath);
            if (!json.Contains("ios-marketing"))
            {
                // Son ] isaretinden once yeni entry ekle
                string marketingEntry =
                    ",\n\t\t{\n\t\t\t\"filename\" : \"Icon-AppStore-1024.png\",\n\t\t\t\"idiom\" : \"ios-marketing\",\n\t\t\t\"scale\" : \"1x\",\n\t\t\t\"size\" : \"1024x1024\"\n\t\t}";
                int lastBracket = json.LastIndexOf(']');
                // ] isaretinden onceki son } isaretini bul
                int lastBrace = json.LastIndexOf('}', lastBracket);
                json = json.Insert(lastBrace + 1, marketingEntry);
                System.IO.File.WriteAllText(contentsPath, json);
            }
        }

        Debug.Log("[ComBoom] 1024x1024 App Store icon basariyla eklendi.");
    }

    // Command-line batch mode build (no dialogs)
    public static void BatchBuild()
    {
        Debug.Log("[ComBoom] iOS batch build başlatılıyor...");

        // Sahne kontrolu
        string[] scenes;
        var editorScenes = EditorBuildSettings.scenes;
        if (editorScenes != null && editorScenes.Length > 0)
        {
            scenes = new string[] { editorScenes[0].path };
        }
        else
        {
            // Fallback: en buyuk sahne dosyasini bul
            var allScenes = new System.Collections.Generic.List<string>();
            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (path.EndsWith(".unity") && !path.Contains("Editor") && !path.Contains("Test"))
                    allScenes.Add(path);
            }
            if (allScenes.Count == 0)
            {
                Debug.LogError("[ComBoom] Hicbir sahne bulunamadi!");
                return;
            }
            allScenes.Sort((a, b) =>
            {
                var fileA = new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, a));
                var fileB = new System.IO.FileInfo(System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, b));
                return fileB.Length.CompareTo(fileA.Length);
            });
            scenes = new string[] { allScenes[0] };
        }

        Debug.Log($"[ComBoom] Build sahnesi: {scenes[0]}");

        // Player ayarlari
        PlayerSettings.productName = "ComBoom";
        PlayerSettings.companyName = "M3Studios";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.m3studios.comboom");
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.targetOSVersionString = "15.0";
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        PlayerSettings.iOS.hideHomeButton = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SplashScreen.show = false;

        string buildPath = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName, "Builds", "iOS");

        if (System.IO.Directory.Exists(buildPath))
            System.IO.Directory.Delete(buildPath, true);
        System.IO.Directory.CreateDirectory(buildPath);

        // App icon
        SetupAppIcon();

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[ComBoom] iOS build BASARILI! Boyut: {summary.totalSize / (1024 * 1024):F1} MB");
            Debug.Log($"[ComBoom] Xcode proje: {buildPath}");
        }
        else
        {
            Debug.LogError($"[ComBoom] iOS build BASARISIZ! Hata: {summary.totalErrors}");
        }
    }

    private static string[] GetBuildScenes()
    {
        // Her zaman aktif sahneyi kullan (Editor'de acik olan)
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        if (!string.IsNullOrEmpty(activeScene))
        {
            // Build Settings'i aktif sahneyle guncelle
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(activeScene, true)
            };
            Debug.Log($"[ComBoom] Build sahnesi: {activeScene}");
            return new string[] { activeScene };
        }

        Debug.LogError("[ComBoom] Aktif sahne bulunamadi!");
        return new string[0];
    }
}
