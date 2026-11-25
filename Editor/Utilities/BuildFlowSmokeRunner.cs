using FurkanGok.BuildFlow.Build;
using FurkanGok.BuildFlow.Profiles;
using UnityEditor;
using UnityEngine;

namespace FurkanGok.BuildFlow.Utilities
{
    /// <summary>
    /// Manual smoke verification entry point for local development hosts.
    /// </summary>
    internal static class BuildFlowSmokeRunner
    {
        [MenuItem(BuildFlowConstants.MenuRoot + "/Internal/Run Windows Smoke Build")]
        private static void RunWindowsSmokeBuild()
        {
            BuildFlowProfile profile = null;
            foreach (var candidate in BuildFlowProfileFactory.FindAllProfiles())
            {
                if (candidate != null && candidate.TargetPlatform == BuildFlowTargetPlatform.WindowsStandaloneX64)
                {
                    profile = candidate;
                    break;
                }
            }

            if (profile == null)
            {
                profile = BuildFlowProfileFactory.CreateProfile("WindowsSmoke");
                profile.TargetPlatform = BuildFlowTargetPlatform.WindowsStandaloneX64;
                profile.BuildConfiguration = BuildFlowBuildConfiguration.Development;
                profile.DevelopmentBuild = true;
                profile.UseEditorBuildSettingsScenes = true;
                profile.OutputDirectory = "Builds/BuildFlowSmoke";
                profile.WindowsExecutableName = "BuildFlowSmoke";
                profile.ApplicationVersion = "0.1.0";
                profile.BuildNumber = "1";
                profile.AllowOverwriteExistingOutput = true;
                profile.CleanOutputBeforeBuild = true;
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
            }

            var service = new BuildExecutionService();
            var validation = service.Validate(profile);
            if (validation.HasBlockingErrors)
            {
                EditorUtility.DisplayDialog(
                    "BuildFlow Smoke Build",
                    $"Validation blocked the smoke build ({validation.ErrorCount} error(s)). Check the Console.",
                    "OK");
                foreach (var item in validation.Results)
                {
                    if (item.IsBlocking)
                        Debug.LogError($"[BuildFlow] {item.RuleName}: {item.DetailedMessage}");
                }
                return;
            }

            EditorUtility.DisplayProgressBar("BuildFlow", "Running Windows smoke build…", 0.2f);
            try
            {
                var result = service.Execute(profile, confirmOverwrite: false);
                Debug.Log(
                    $"[BuildFlow] Smoke build finished. Result={result.Result}, Output={result.OutputPath}, " +
                    $"Report={result.ReportPath}, Duration={result.DurationSeconds:0.##}s");
                EditorUtility.DisplayDialog(
                    "BuildFlow Smoke Build",
                    $"Result: {result.Result}\nOutput: {result.OutputPath}\nReport: {result.ReportPath}",
                    "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
