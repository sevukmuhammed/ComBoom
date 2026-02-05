using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using ComBoom.Gameplay;

public class AndroidBuilder : EditorWindow
{
    [MenuItem("ComBoom/Build Android (APK)")]
    public static void BuildAndroidAPK()
    {
        BuildAndroid(false);
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
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; // Android 7.0
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34; // Android 14
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.buildAppBundle = buildAAB;

        // App icon ayarla
        SetupAndroidIcon();

        string formatStr = buildAAB ? "AAB (Play Store)" : "APK";
        string ext = buildAAB ? ".aab" : ".apk";

        // Build baslatilsin mi?
        if (!EditorUtility.DisplayDialog("ComBoom Android Build",
            $"Android {formatStr} build baslatilacak.\n\nBundle ID: com.m3studios.comboom\nSahneler: {scenes.Length} sahne\nMin SDK: Android 7.0 (API 24)\nMimari: ARM64\n\nDevam?",
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
