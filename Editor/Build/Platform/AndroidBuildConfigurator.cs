using System.IO;
using FurkanGok.BuildFlow.Utilities;
using UnityEditor;
using UnityEditor.Build;

namespace FurkanGok.BuildFlow.Build.Platform
{
    public sealed class AndroidBuildConfigurator : IPlatformBuildConfigurator
    {
        public BuildFlowTargetPlatform Platform => BuildFlowTargetPlatform.AndroidApk;

        public string ResolveLocationPathName(BuildFlowProfile profile, string outputFolder)
        {
            var artifact = ArtifactNaming.Resolve(profile.ArtifactNamingFormat, profile);
            var extension = profile.TargetPlatform == BuildFlowTargetPlatform.AndroidAab ? ".aab" : ".apk";
            return Path.Combine(outputFolder, artifact + extension);
        }

        public void ApplyPreBuildSettings(BuildFlowProfile profile, out IDisposableBuildState restoreState)
        {
            var previousBundle = EditorUserBuildSettings.buildAppBundle;
            var previousBundleVersion = PlayerSettings.bundleVersion;
            var previousVersionCode = PlayerSettings.Android.bundleVersionCode;
            var previousAppId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);

            EditorUserBuildSettings.buildAppBundle = profile.TargetPlatform == BuildFlowTargetPlatform.AndroidAab;
            PlayerSettings.bundleVersion = profile.ApplicationVersion;

            if (int.TryParse(profile.BuildNumber, out var versionCode))
                PlayerSettings.Android.bundleVersionCode = versionCode;

            if (profile.OverrideAndroidBundleIdentifier && !string.IsNullOrWhiteSpace(profile.AndroidBundleIdentifier))
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, profile.AndroidBundleIdentifier);

            restoreState = new AndroidSettingsRestoreState(
                previousBundle,
                previousBundleVersion,
                previousVersionCode,
                previousAppId);
        }

        public void ValidateGeneratedOutput(string locationPathName, BuildFlowBuildResult result)
        {
            if (!File.Exists(locationPathName))
            {
                result.ErrorSummary = string.IsNullOrEmpty(result.ErrorSummary)
                    ? $"Expected Android package was not found at '{locationPathName}'."
                    : result.ErrorSummary;
            }
        }

        private sealed class AndroidSettingsRestoreState : IDisposableBuildState
        {
            private readonly bool _buildAppBundle;
            private readonly string _bundleVersion;
            private readonly int _versionCode;
            private readonly string _applicationId;

            public AndroidSettingsRestoreState(bool buildAppBundle, string bundleVersion, int versionCode, string applicationId)
            {
                _buildAppBundle = buildAppBundle;
                _bundleVersion = bundleVersion;
                _versionCode = versionCode;
                _applicationId = applicationId;
            }

            public void Dispose()
            {
                EditorUserBuildSettings.buildAppBundle = _buildAppBundle;
                PlayerSettings.bundleVersion = _bundleVersion;
                PlayerSettings.Android.bundleVersionCode = _versionCode;
                if (!string.IsNullOrEmpty(_applicationId))
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, _applicationId);
            }
        }
    }
}
