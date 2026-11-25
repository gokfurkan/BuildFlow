using System;
using UnityEditor;
using UnityEngine;

namespace FurkanGok.BuildFlow.Utilities
{
    public static class PlatformSupportUtility
    {
        public static BuildTarget ToUnityBuildTarget(BuildFlowTargetPlatform platform)
        {
            switch (platform)
            {
                case BuildFlowTargetPlatform.WindowsStandaloneX64:
                    return BuildTarget.StandaloneWindows64;
                case BuildFlowTargetPlatform.AndroidApk:
                case BuildFlowTargetPlatform.AndroidAab:
                    return BuildTarget.Android;
                case BuildFlowTargetPlatform.IosXcodeProject:
                    return BuildTarget.iOS;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
        }

        public static bool IsBuildTargetSupported(BuildFlowTargetPlatform platform)
        {
            var target = ToUnityBuildTarget(platform);
            return BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target);
        }

        public static string GetDisplayName(BuildFlowTargetPlatform platform)
        {
            switch (platform)
            {
                case BuildFlowTargetPlatform.WindowsStandaloneX64:
                    return "Windows Standalone x64";
                case BuildFlowTargetPlatform.AndroidApk:
                    return "Android APK";
                case BuildFlowTargetPlatform.AndroidAab:
                    return "Android AAB";
                case BuildFlowTargetPlatform.IosXcodeProject:
                    return "iOS Xcode Project";
                default:
                    return platform.ToString();
            }
        }

        public static bool IsAndroidSdkConfigured()
        {
            var sdk = EditorPrefs.GetString("AndroidSdkRoot");
            if (string.IsNullOrWhiteSpace(sdk))
                sdk = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT")
                      ?? Environment.GetEnvironmentVariable("ANDROID_HOME");

            return !string.IsNullOrWhiteSpace(sdk) && System.IO.Directory.Exists(sdk);
        }

        public static bool IsAndroidNdkConfigured()
        {
            var ndk = EditorPrefs.GetString("AndroidNdkRoot");
            if (string.IsNullOrWhiteSpace(ndk))
                ndk = Environment.GetEnvironmentVariable("ANDROID_NDK_ROOT")
                      ?? Environment.GetEnvironmentVariable("ANDROID_NDK_HOME");

            return !string.IsNullOrWhiteSpace(ndk) && System.IO.Directory.Exists(ndk);
        }

        public static bool IsRunningOnMacOs()
        {
            return Application.platform == RuntimePlatform.OSXEditor;
        }

        public static bool TryGetNativeBuildProfileTarget(UnityEditor.Build.Profile.BuildProfile nativeProfile, out BuildTarget buildTarget)
        {
            buildTarget = BuildTarget.NoTarget;
            if (nativeProfile == null)
                return false;

            var so = new SerializedObject(nativeProfile);
            var prop = so.FindProperty("m_BuildTarget");
            if (prop == null)
                return false;

            buildTarget = (BuildTarget)prop.intValue;
            return buildTarget != BuildTarget.NoTarget;
        }
    }
}
