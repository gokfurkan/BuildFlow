using System.IO;
using FurkanGok.BuildFlow.Utilities;
using UnityEditor;

namespace FurkanGok.BuildFlow.Build.Platform
{
    public sealed class WindowsBuildConfigurator : IPlatformBuildConfigurator
    {
        public BuildFlowTargetPlatform Platform => BuildFlowTargetPlatform.WindowsStandaloneX64;

        public string ResolveLocationPathName(BuildFlowProfile profile, string outputFolder)
        {
            var exeBase = string.IsNullOrWhiteSpace(profile.WindowsExecutableName)
                ? ArtifactNaming.Sanitize(profile.ProfileName)
                : ArtifactNaming.Sanitize(profile.WindowsExecutableName);
            return Path.Combine(outputFolder, exeBase + ".exe");
        }

        public void ApplyPreBuildSettings(BuildFlowProfile profile, out IDisposableBuildState restoreState)
        {
            restoreState = new NoopBuildState();
        }

        public void ValidateGeneratedOutput(string locationPathName, BuildFlowBuildResult result)
        {
            if (!File.Exists(locationPathName))
            {
                result.ErrorSummary = string.IsNullOrEmpty(result.ErrorSummary)
                    ? $"Expected Windows EXE was not found at '{locationPathName}'."
                    : result.ErrorSummary;
            }
        }
    }
}
