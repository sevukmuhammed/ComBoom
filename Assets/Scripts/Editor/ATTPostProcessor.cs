#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace ComBoom.Editor
{
    public class ATTPostProcessor
    {
        // Google's PListProcessor runs at order=0, we run after it
        [PostProcessBuild(1)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS)
                return;

            AddATTFramework(path);
        }

        private static void AddATTFramework(string path)
        {
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

            // UnityFramework target'a ekle (Unity 2019.3+ yapisi)
            string frameworkGuid = proj.GetUnityFrameworkTargetGuid();
            proj.AddFrameworkToProject(frameworkGuid, "AppTrackingTransparency.framework", true); // true = weak link

            // Ana target'a da ekle
            string mainGuid = proj.GetUnityMainTargetGuid();
            proj.AddFrameworkToProject(mainGuid, "AppTrackingTransparency.framework", true);

            proj.WriteToFile(projPath);

            UnityEngine.Debug.Log("[ATTPostProcessor] AppTrackingTransparency.framework eklendi (weak link).");
        }
    }
}
#endif
