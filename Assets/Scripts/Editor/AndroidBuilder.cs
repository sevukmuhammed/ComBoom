using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using ComBoom.Gameplay;
using ComBoom.Social;
#if UNITY_ANDROID
using GooglePlayGames;
#endif

public class AndroidBuilder : EditorWindow
{
    [MenuItem("ComBoom/Build Android (APK)")]
    public static void BuildAndroidAPK()
    {
        BuildAndroid(false);
    }

    // Command-line batch mode build (no dialogs)
    public static void BatchBuildAPK()
    {
        Debug.Log("[ComBoom] Batch APK build baslatiliyor...");

        PlayerSettings.productName = "ComBoom";
        PlayerSettings.companyName = "M3Studios";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.m3studios.comboom");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.buildAppBundle = false;
        PlayerSettings.Android.minifyRelease = true;

        // Keystore ayarlari (environment variable veya fallback)
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName, "comboom.keystore");
        PlayerSettings.Android.keystorePass = GetKeystorePassword("COMBOOM_KEYSTORE_PASS");
        PlayerSettings.Android.keyaliasName = "comboom";
        PlayerSettings.Android.keyaliasPass = GetKeystorePassword("COMBOOM_KEY_PASS");

        // Sahne bul
        string[] scenes = null;
        var buildScenes = EditorBuildSettings.scenes;
        if (buildScenes != null && buildScenes.Length > 0)
        {
            var enabledScenes = new System.Collections.Generic.List<string>();
            foreach (var s in buildScenes)
            {
                if (s.enabled) enabledScenes.Add(s.path);
            }
            if (enabledScenes.Count > 0) scenes = enabledScenes.ToArray();
        }

        if (scenes == null || scenes.Length == 0)
        {
            // Fallback: tum .unity dosyalarini ara
            string[] allScenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            var sceneList = new System.Collections.Generic.List<string>();
            foreach (string guid in allScenes)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains("Editor") && !path.Contains("Test"))
                {
                    sceneList.Add(path);
                    Debug.Log($"[ComBoom] Bulunan sahne: {path}");
                }
            }
            if (sceneList.Count > 0)
            {
                // En buyuk sahne dosyasini sec (ana sahne genellikle en buyuktur)
                sceneList.Sort((a, b) =>
                {
                    var fileA = new System.IO.FileInfo(System.IO.Path.Combine(
                        System.IO.Directory.GetParent(Application.dataPath).FullName, a));
                    var fileB = new System.IO.FileInfo(System.IO.Path.Combine(
                        System.IO.Directory.GetParent(Application.dataPath).FullName, b));
                    return fileB.Length.CompareTo(fileA.Length);
                });
                scenes = new string[] { sceneList[0] };
                Debug.Log($"[ComBoom] Ana sahne secildi: {scenes[0]}");
            }
        }

        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogError("[ComBoom] Hic sahne bulunamadi! Build iptal.");
            EditorApplication.Exit(1);
            return;
        }

        string buildDir = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName, "Builds", "Android");
        if (!System.IO.Directory.Exists(buildDir))
            System.IO.Directory.CreateDirectory(buildDir);

        string buildPath = System.IO.Path.Combine(buildDir, "ComBoom.apk");
        if (System.IO.File.Exists(buildPath))
            System.IO.File.Delete(buildPath);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[ComBoom] APK build BASARILI! Boyut: {report.summary.totalSize / (1024 * 1024):F1} MB");
            Debug.Log($"[ComBoom] Dosya: {buildPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[ComBoom] APK build BASARISIZ! Hatalar: {report.summary.totalErrors}");
            EditorApplication.Exit(1);
        }
    }

    public static void BatchBuildAAB()
    {
        Debug.Log("[ComBoom] Batch AAB build baslatiliyor...");

        PlayerSettings.productName = "ComBoom";
        PlayerSettings.companyName = "M3Studios";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.m3studios.comboom");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.Android.minifyRelease = true;

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName, "comboom.keystore");
        PlayerSettings.Android.keystorePass = GetKeystorePassword("COMBOOM_KEYSTORE_PASS");
        PlayerSettings.Android.keyaliasName = "comboom";
        PlayerSettings.Android.keyaliasPass = GetKeystorePassword("COMBOOM_KEY_PASS");

        string[] scenes = null;
        var buildScenes = EditorBuildSettings.scenes;
        if (buildScenes != null && buildScenes.Length > 0)
        {
            var enabledScenes = new System.Collections.Generic.List<string>();
            foreach (var s in buildScenes)
            {
                if (s.enabled) enabledScenes.Add(s.path);
            }
            if (enabledScenes.Count > 0) scenes = enabledScenes.ToArray();
        }

        if (scenes == null || scenes.Length == 0)
        {
            string[] allScenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            var sceneList = new System.Collections.Generic.List<string>();
            foreach (string guid in allScenes)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains("Editor") && !path.Contains("Test"))
                    sceneList.Add(path);
            }
            if (sceneList.Count > 0)
            {
                sceneList.Sort((a, b) =>
                {
                    var fileA = new System.IO.FileInfo(System.IO.Path.Combine(
                        System.IO.Directory.GetParent(Application.dataPath).FullName, a));
                    var fileB = new System.IO.FileInfo(System.IO.Path.Combine(
                        System.IO.Directory.GetParent(Application.dataPath).FullName, b));
                    return fileB.Length.CompareTo(fileA.Length);
                });
                scenes = new string[] { sceneList[0] };
                Debug.Log($"[ComBoom] Ana sahne secildi: {scenes[0]}");
            }
        }

        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogError("[ComBoom] Hic sahne bulunamadi! Build iptal.");
            EditorApplication.Exit(1);
            return;
        }

        string buildDir = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName, "Builds", "Android");
        if (!System.IO.Directory.Exists(buildDir))
            System.IO.Directory.CreateDirectory(buildDir);

        string buildPath = System.IO.Path.Combine(buildDir, "ComBoom.aab");
        if (System.IO.File.Exists(buildPath))
            System.IO.File.Delete(buildPath);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[ComBoom] AAB build BASARILI! Boyut: {report.summary.totalSize / (1024 * 1024):F1} MB");
            Debug.Log($"[ComBoom] Dosya: {buildPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[ComBoom] AAB build BASARISIZ! Hatalar: {report.summary.totalErrors}");
            EditorApplication.Exit(1);
        }
    }

    [MenuItem("ComBoom/Build Android (AAB - Play Store)")]
    public static void BuildAndroidAAB()
    {
        BuildAndroid(true);
    }

    private static void BuildAndroid(bool buildAAB)
    {
        // Aktif sahneyi kaydet
        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.isDirty)
        {
            EditorSceneManager.SaveScene(currentScene);
            Debug.Log($"[ComBoom] Sahne kaydedildi: {currentScene.path}");
        }
        else
        {
            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorSceneManager.SaveScene(currentScene);
            Debug.Log($"[ComBoom] Sahne zorla kaydedildi: {currentScene.path}");
        }

        // GPGS yapilandirma kontrolu
        ValidatePlayGamesSetup();

        // Sahne kontrolu
        string[] scenes = GetBuildScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError("[ComBoom] Build Settings'te sahne yok!");
            return;
        }

        // Player ayarlarini guncelle
        PlayerSettings.productName = "ComBoom";
        PlayerSettings.companyName = "M3Studios";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.m3studios.comboom");

        // Android ozel ayarlar
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SplashScreen.show = false;

        // Android spesifik
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25; // Android 7.1
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35; // Android 14
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.buildAppBundle = buildAAB;
        PlayerSettings.Android.minifyRelease = true;

        // App icon ayarla
        SetupAndroidIcon();

        string formatStr = buildAAB ? "AAB (Play Store)" : "APK";
        string ext = buildAAB ? ".aab" : ".apk";

        // Build baslatilsin mi?
        if (!EditorUtility.DisplayDialog("ComBoom Android Build",
            $"Android {formatStr} build baslatilacak.\n\nBundle ID: com.m3studios.comboom\nSahneler: {scenes.Length} sahne\nMin SDK: Android 7.1 (API 25)\nTarget SDK: API 35\nMimari: ARM64\n\nDevam?",
            "Build", "Iptal"))
        {
            return;
        }

        string buildDir = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName, "Builds", "Android");

        if (!System.IO.Directory.Exists(buildDir))
            System.IO.Directory.CreateDirectory(buildDir);

        string buildPath = System.IO.Path.Combine(buildDir, $"ComBoom{ext}");

        // Eski build dosyasini sil
        if (System.IO.File.Exists(buildPath))
            System.IO.File.Delete(buildPath);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        Debug.Log($"[ComBoom] Android {formatStr} build baslatiliyor... Hedef: {buildPath}");

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[ComBoom] Android {formatStr} build BASARILI! Boyut: {summary.totalSize / (1024 * 1024):F1} MB");
            Debug.Log($"[ComBoom] Dosya: {buildPath}");

            // Finder'da goster
            EditorUtility.RevealInFinder(buildPath);
        }
        else
        {
            Debug.LogError($"[ComBoom] Android build BASARISIZ! Hata sayisi: {summary.totalErrors}");
        }
    }

    private static void ValidatePlayGamesSetup()
    {
        var warnings = new System.Collections.Generic.List<string>();

#if UNITY_ANDROID
        if (!GameInfo.ApplicationIdInitialized())
        {
            warnings.Add("Google Play Games APP_ID ayarlanmamis (GameInfo.cs)");
        }
#endif

        SocialConfig config = AssetDatabase.LoadAssetAtPath<SocialConfig>("Assets/Resources/SocialConfig.asset");
        if (config == null)
        {
            warnings.Add("SocialConfig.asset bulunamadi (Assets/Resources/)");
        }
        else if (string.IsNullOrEmpty(config.androidLeaderboardId))
        {
            warnings.Add("Android Leaderboard ID bos (SocialConfig.asset)");
        }

        if (warnings.Count > 0)
        {
            string msg = "Google Play Games yapilandirma uyarilari:\n\n";
            foreach (var w in warnings)
            {
                msg += "- " + w + "\n";
            }
            msg += "\nDuzeltmek icin: ComBoom > Setup Play Games\nBuild'e devam edebilirsiniz.";

            Debug.LogWarning("[ComBoom] GPGS Yapilandirma Uyarisi:\n" + msg);
            EditorUtility.DisplayDialog("GPGS Uyarisi", msg, "Tamam, Devam Et");
        }
    }

    private static void SetupAndroidIcon()
    {
        // Icon texture olustur (1024x1024)
        Texture2D iconTex = SpriteGenerator.CreateAppIconTexture();

        string iconPath = "Assets/AppIcon.png";
        string fullPath = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName, iconPath);

        // Dosya yoksa olustur
        if (!System.IO.File.Exists(fullPath))
        {
            byte[] pngData = iconTex.EncodeToPNG();
            System.IO.File.WriteAllBytes(fullPath, pngData);
            AssetDatabase.ImportAsset(iconPath, ImportAssetOptions.ForceUpdate);

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
        }

        Texture2D importedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
        if (importedTex == null)
        {
            Debug.LogError("[ComBoom] Icon texture yuklenemedi!");
            if (Application.isPlaying)
                Object.Destroy(iconTex);
            else
                Object.DestroyImmediate(iconTex);
            return;
        }

        // Android platform icon'larini ayarla
        var platform = BuildTargetGroup.Android;
        var kinds = new[]
        {
            UnityEditor.Android.AndroidPlatformIconKind.Round,
            UnityEditor.Android.AndroidPlatformIconKind.Legacy,
            UnityEditor.Android.AndroidPlatformIconKind.Adaptive
        };

        foreach (var kind in kinds)
        {
            var icons = PlayerSettings.GetPlatformIcons(platform, kind);
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i].SetTexture(importedTex, 0);
            }
            PlayerSettings.SetPlatformIcons(platform, kind, icons);
        }

        Debug.Log("[ComBoom] Android app icon atandi.");

        // Bellek temizle
        if (Application.isPlaying)
            Object.Destroy(iconTex);
        else
            Object.DestroyImmediate(iconTex);
    }

    private static string GetKeystorePassword(string envVar)
    {
        string envValue = System.Environment.GetEnvironmentVariable(envVar);
        if (!string.IsNullOrEmpty(envValue))
            return envValue;

        Debug.LogWarning($"[ComBoom] {envVar} environment variable bulunamadi, varsayilan sifre kullaniliyor.");
        return "comboom";
    }

    private static string[] GetBuildScenes()
    {
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        if (!string.IsNullOrEmpty(activeScene))
        {
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
