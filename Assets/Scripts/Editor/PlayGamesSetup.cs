using UnityEngine;
using UnityEditor;
using ComBoom.Social;

public class PlayGamesSetup : EditorWindow
{
    private string appId = "";
    private string leaderboardId = "";

    [MenuItem("ComBoom/Setup Play Games")]
    public static void ShowWindow()
    {
        // Mevcut degerleri oku
        string currentAppId = ReadCurrentAppId();
        string currentLeaderboardId = ReadCurrentLeaderboardId();

        var window = GetWindow<PlayGamesSetup>("Play Games Setup");
        window.appId = currentAppId ?? "";
        window.leaderboardId = currentLeaderboardId ?? "";
        window.minSize = new Vector2(400, 200);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Google Play Games Yapilandirmasi", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Play Console'dan App ID ve Leaderboard ID degerlerini girin.\n" +
            "App ID: Play Console > Oyun Hizmetleri > Yapilandirma\n" +
            "Leaderboard ID: Play Console > Skor Tablolari",
            MessageType.Info);

        GUILayout.Space(10);

        appId = EditorGUILayout.TextField("App ID", appId);
        leaderboardId = EditorGUILayout.TextField("Leaderboard ID", leaderboardId);

        GUILayout.Space(20);

        if (GUILayout.Button("Kaydet ve Uygula", GUILayout.Height(30)))
        {
            ApplySettings();
        }
    }

    private void ApplySettings()
    {
        if (string.IsNullOrEmpty(appId))
        {
            EditorUtility.DisplayDialog("Hata", "App ID bos olamaz!", "Tamam");
            return;
        }

        bool success = true;
        int updatedCount = 0;

        // 1. GameInfo.cs - ApplicationId guncelle
        if (UpdateGameInfo(appId))
        {
            updatedCount++;
            Debug.Log("[ComBoom] GameInfo.cs guncellendi - ApplicationId: " + appId);
        }
        else
        {
            Debug.LogWarning("[ComBoom] GameInfo.cs guncellenemedi!");
            success = false;
        }

        // 2. AndroidManifest.xml - APP_ID guncelle
        if (UpdateAndroidManifest(appId))
        {
            updatedCount++;
            Debug.Log("[ComBoom] AndroidManifest.xml guncellendi - APP_ID: " + appId);
        }
        else
        {
            Debug.LogWarning("[ComBoom] AndroidManifest.xml guncellenemedi!");
            success = false;
        }

        // 3. SocialConfig.asset - androidLeaderboardId guncelle
        if (!string.IsNullOrEmpty(leaderboardId))
        {
            if (UpdateSocialConfig(leaderboardId))
            {
                updatedCount++;
                Debug.Log("[ComBoom] SocialConfig.asset guncellendi - androidLeaderboardId: " + leaderboardId);
            }
            else
            {
                Debug.LogWarning("[ComBoom] SocialConfig.asset guncellenemedi!");
                success = false;
            }
        }

        // 4. GooglePlayGameSettings.txt guncelle
        if (UpdateGooglePlayGameSettings(appId))
        {
            updatedCount++;
            Debug.Log("[ComBoom] GooglePlayGameSettings.txt guncellendi");
        }

        AssetDatabase.Refresh();

        if (success)
        {
            EditorUtility.DisplayDialog("Basarili",
                $"{updatedCount} dosya guncellendi.\n\n" +
                $"App ID: {appId}\n" +
                $"Leaderboard ID: {leaderboardId}",
                "Tamam");
            Close();
        }
        else
        {
            EditorUtility.DisplayDialog("Kismi Basari",
                $"{updatedCount} dosya guncellendi, ancak bazi dosyalar guncellenemedi.\nDetaylar icin Console'u kontrol edin.",
                "Tamam");
        }
    }

    // ============================================================
    // GameInfo.cs - ApplicationId guncelle
    // ============================================================
    private static bool UpdateGameInfo(string appId)
    {
        string path = "Assets/GooglePlayGames/com.google.play.games/Runtime/Scripts/GameInfo.cs";
        string fullPath = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName,
            path.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString()));

        if (!System.IO.File.Exists(fullPath))
        {
            Debug.LogError("[ComBoom] GameInfo.cs bulunamadi: " + fullPath);
            return false;
        }

        string content = System.IO.File.ReadAllText(fullPath);

        // ApplicationId satirini bul ve guncelle
        string pattern = "public const string ApplicationId = \"";
        int startIdx = content.IndexOf(pattern);
        if (startIdx < 0)
        {
            Debug.LogError("[ComBoom] GameInfo.cs'de ApplicationId alani bulunamadi");
            return false;
        }

        startIdx += pattern.Length;
        int endIdx = content.IndexOf("\";", startIdx);
        if (endIdx < 0) return false;

        content = content.Substring(0, startIdx) + appId + content.Substring(endIdx);
        System.IO.File.WriteAllText(fullPath, content);
        return true;
    }

    // ============================================================
    // AndroidManifest.xml - APP_ID guncelle
    // ============================================================
    private static bool UpdateAndroidManifest(string appId)
    {
        string path = "Assets/Plugins/Android/GooglePlayGamesManifest.androidlib/AndroidManifest.xml";
        string fullPath = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName,
            path.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString()));

        if (!System.IO.File.Exists(fullPath))
        {
            Debug.LogError("[ComBoom] AndroidManifest.xml bulunamadi: " + fullPath);
            return false;
        }

        string content = System.IO.File.ReadAllText(fullPath);

        // APP_ID degerini guncelle - \u003 prefix'i korunmali
        // Mevcut: android:value="\u003" veya android:value="\u003XXXXX"
        string searchPattern = "com.google.android.gms.games.APP_ID";
        int metaIdx = content.IndexOf(searchPattern);
        if (metaIdx < 0)
        {
            Debug.LogError("[ComBoom] AndroidManifest'te APP_ID meta-data bulunamadi");
            return false;
        }

        // value="..." kismini bul
        string valuePattern = "android:value=\"";
        int valueStart = content.IndexOf(valuePattern, metaIdx);
        if (valueStart < 0) return false;

        valueStart += valuePattern.Length;
        int valueEnd = content.IndexOf("\"", valueStart);
        if (valueEnd < 0) return false;

        // \u003 prefix ile App ID'yi yaz
        content = content.Substring(0, valueStart) + "\\u003" + appId + content.Substring(valueEnd);
        System.IO.File.WriteAllText(fullPath, content);
        return true;
    }

    // ============================================================
    // SocialConfig.asset - androidLeaderboardId guncelle
    // ============================================================
    private static bool UpdateSocialConfig(string leaderboardId)
    {
        string configPath = "Assets/Resources/SocialConfig.asset";
        SocialConfig config = AssetDatabase.LoadAssetAtPath<SocialConfig>(configPath);

        if (config == null)
        {
            // Olustur
            config = ScriptableObject.CreateInstance<SocialConfig>();
            AssetDatabase.CreateAsset(config, configPath);
        }

        config.androidLeaderboardId = leaderboardId;
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        return true;
    }

    // ============================================================
    // GooglePlayGameSettings.txt guncelle
    // ============================================================
    private static bool UpdateGooglePlayGameSettings(string appId)
    {
        string settingsPath = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName,
            "ProjectSettings", "GooglePlayGameSettings.txt");

        if (!System.IO.File.Exists(settingsPath))
        {
            Debug.Log("[ComBoom] GooglePlayGameSettings.txt bulunamadi, atlanıyor");
            return false;
        }

        string content = System.IO.File.ReadAllText(settingsPath);

        // proj.AppId satirini ekle veya guncelle
        if (content.Contains("proj.AppId="))
        {
            // Mevcut satiri guncelle
            var lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("proj.AppId="))
                {
                    lines[i] = "proj.AppId=" + appId;
                }
            }
            content = string.Join("\n", lines);
        }
        else
        {
            // Yeni satir ekle
            content = content.TrimEnd() + "\nproj.AppId=" + appId + "\n";
        }

        System.IO.File.WriteAllText(settingsPath, content);
        return true;
    }

    // ============================================================
    // Mevcut degerleri oku
    // ============================================================
    private static string ReadCurrentAppId()
    {
        string path = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath)?.FullName ?? "",
            "Assets", "GooglePlayGames", "com.google.play.games", "Runtime", "Scripts", "GameInfo.cs");

        if (!System.IO.File.Exists(path)) return "";

        string content = System.IO.File.ReadAllText(path);
        string pattern = "public const string ApplicationId = \"";
        int startIdx = content.IndexOf(pattern);
        if (startIdx < 0) return "";

        startIdx += pattern.Length;
        int endIdx = content.IndexOf("\";", startIdx);
        if (endIdx < 0) return "";

        return content.Substring(startIdx, endIdx - startIdx);
    }

    private static string ReadCurrentLeaderboardId()
    {
        SocialConfig config = AssetDatabase.LoadAssetAtPath<SocialConfig>("Assets/Resources/SocialConfig.asset");
        return config != null ? config.androidLeaderboardId : "";
    }
}
