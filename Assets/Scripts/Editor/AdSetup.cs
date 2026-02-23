using UnityEngine;
using UnityEditor;

public class AdSetup : EditorWindow
{
    private const string DEFINE_SYMBOL = "GOOGLE_MOBILE_ADS";

    [MenuItem("ComBoom/Enable Ad SDK")]
    public static void EnableAdSDK()
    {
        AddDefineSymbol(BuildTargetGroup.iOS);
        AddDefineSymbol(BuildTargetGroup.Android);
        Debug.Log($"[ComBoom] '{DEFINE_SYMBOL}' define symbol eklendi. Kod yeniden derlenecek...");
    }

    [MenuItem("ComBoom/Disable Ad SDK")]
    public static void DisableAdSDK()
    {
        RemoveDefineSymbol(BuildTargetGroup.iOS);
        RemoveDefineSymbol(BuildTargetGroup.Android);
        Debug.Log($"[ComBoom] '{DEFINE_SYMBOL}' define symbol kaldirildi. Mock mode aktif.");
    }

    private static void AddDefineSymbol(BuildTargetGroup group)
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        if (!defines.Contains(DEFINE_SYMBOL))
        {
            defines = string.IsNullOrEmpty(defines) ? DEFINE_SYMBOL : $"{defines};{DEFINE_SYMBOL}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
        }
    }

    private static void RemoveDefineSymbol(BuildTargetGroup group)
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        if (defines.Contains(DEFINE_SYMBOL))
        {
            defines = defines.Replace($";{DEFINE_SYMBOL}", "").Replace($"{DEFINE_SYMBOL};", "").Replace(DEFINE_SYMBOL, "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
        }
    }
}
