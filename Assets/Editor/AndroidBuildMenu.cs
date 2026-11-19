#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ApexGTWorld.EditorTools
{
    public static class AndroidBuildMenu
    {
        [MenuItem("APEX GT WORLD/Build/Build Android APK")]
        public static void BuildAndroidApk()
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogError("Could not switch to Android. Install Android Build Support, SDK, NDK and OpenJDK in Unity Hub.");
                return;
            }

            List<string> enabledScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                if (scene.enabled) enabledScenes.Add(scene.path);

            if (enabledScenes.Count == 0)
            {
                Debug.LogError("No enabled scenes in Build Settings. Generate the Starter Car Scene or Motorcycle Test Scene first.");
                return;
            }

            PlayerSettings.companyName = "APEX GT WORLD";
            PlayerSettings.productName = "APEX GT WORLD";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.apexgtworld.game");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = false;

            const string outputDirectory = "Builds/Android";
            Directory.CreateDirectory(outputDirectory);
            string outputPath = Path.Combine(outputDirectory, "APEX_GT_WORLD.apk");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = enabledScenes.ToArray(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"Android APK created: {outputPath} ({report.summary.totalSize / (1024f * 1024f):0.0} MB)");
            else
                Debug.LogError($"Android build failed: {report.summary.result}; errors={report.summary.totalErrors}");
        }
    }
}
#endif
