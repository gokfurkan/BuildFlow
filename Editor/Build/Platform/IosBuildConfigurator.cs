using System.IO;
using UnityEditor;
using UnityEditor.Build;

namespace FurkanGok.BuildFlow.Build.Platform
{
    public sealed class IosBuildConfigurator : IPlatformBuildConfigurator
    {
        public BuildFlowTargetPlatform Platform => BuildFlowTargetPlatform.IosXcodeProject;

        public string ResolveLocationPathName(BuildFlowProfile profile, string outputFolder)
        {
            // iOS builds require a directory path for the generated Xcode project.
            return outputFolder;
        }

        public void ApplyPreBuildSettings(BuildFlowProfile profile, out IDisposableBuildState restoreState)
        {
            var previousBundleVersion = PlayerSettings.bundleVersion;
            var previousBuildNumber = PlayerSettings.iOS.buildNumber;
            var previousAppId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS);

            PlayerSettings.bundleVersion = profile.ApplicationVersion;
            PlayerSettings.iOS.buildNumber = profile.BuildNumber;

            if (profile.OverrideIosBundleIdentifier && !string.IsNullOrWhiteSpace(profile.IosBundleIdentifier))
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, profile.IosBundleIdentifier);

            restoreState = new IosSettingsRestoreState(previousBundleVersion, previousBuildNumber, previousAppId);
        }

        public void ValidateGeneratedOutput(string locationPathName, BuildFlowBuildResult result)
        {
            result.IsXcodeExportOnly = true;
            result.Notes = "iOS export produces an Xcode project. This is not an IPA. Compiling, signing, and IPA export require macOS and Xcode.";

            if (!Directory.Exists(locationPathName))
            {
                result.ErrorSummary = string.IsNullOrEmpty(result.ErrorSummary)
                    ? $"Expected Xcode project directory was not found at '{locationPathName}'."
                    : result.ErrorSummary;
                return;
            }

            var hasXcodeProject = Directory.GetFiles(locationPathName, "*.xcodeproj", SearchOption.AllDirectories).Length > 0
                                  || Directory.GetDirectories(locationPathName, "*.xcodeproj", SearchOption.TopDirectoryOnly).Length > 0
                                  || File.Exists(Path.Combine(locationPathName, "Unity-iPhone.xcodeproj", "project.pbxproj"))
                                  || File.Exists(Path.Combine(locationPathName, "Info.plist"));

            if (!hasXcodeProject)
            {
                // Unity may still write useful export content; keep as warning-level note rather than forced failure if BuildReport succeeded.
                result.Notes += " Generated folder did not contain a clearly detectable .xcodeproj; inspect the export manually.";
            }
        }

        private sealed class IosSettingsRestoreState : IDisposableBuildState
        {
            private readonly string _bundleVersion;
            private readonly string _buildNumber;
            private readonly string _applicationId;

            public IosSettingsRestoreState(string bundleVersion, string buildNumber, string applicationId)
            {
                _bundleVersion = bundleVersion;
                _buildNumber = buildNumber;
                _applicationId = applicationId;
            }

            public void Dispose()
            {
                PlayerSettings.bundleVersion = _bundleVersion;
                PlayerSettings.iOS.buildNumber = _buildNumber;
                if (!string.IsNullOrEmpty(_applicationId))
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, _applicationId);
            }
        }
    }
}
