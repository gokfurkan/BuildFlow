using System;
using System.IO;
using FurkanGok.BuildFlow.Build.Platform;
using FurkanGok.BuildFlow.History;
using FurkanGok.BuildFlow.Profiles;
using FurkanGok.BuildFlow.Utilities;
using FurkanGok.BuildFlow.Validation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using BuildResult = UnityEditor.Build.Reporting.BuildResult;

namespace FurkanGok.BuildFlow.Build
{
    public sealed class BuildExecutionService
    {
        private readonly BuildValidator _validator;
        private readonly BuildHistoryService _historyService;
        private readonly WindowsBuildConfigurator _windows = new WindowsBuildConfigurator();
        private readonly AndroidBuildConfigurator _android = new AndroidBuildConfigurator();
        private readonly IosBuildConfigurator _ios = new IosBuildConfigurator();

        public BuildExecutionService()
            : this(new BuildValidator(), new BuildHistoryService())
        {
        }

        public BuildExecutionService(BuildValidator validator, BuildHistoryService historyService)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        public event Action<string> OperationChanged;
        public event Action<BuildFlowBuildResult> BuildCompleted;

        public string CurrentOperation { get; private set; } = "Idle";
        public bool IsBuilding { get; private set; }

        public ValidationReport Validate(BuildFlowProfile profile)
        {
            return _validator.Validate(profile);
        }

        public BuildFlowBuildResult Execute(BuildFlowProfile profile, bool confirmOverwrite = true)
        {
            if (IsBuilding)
                throw new InvalidOperationException("A BuildFlow build is already running.");

            IsBuilding = true;
            var result = BuildFlowBuildResult.CreateSkeleton(profile);

            try
            {
                SetOperation("Validating profile");
                var validation = _validator.Validate(profile);
                if (validation.HasBlockingErrors)
                {
                    result.Result = BuildFlowResultStatus.ValidationBlocked.ToString();
                    result.ErrorSummary = $"Validation blocked the build ({validation.ErrorCount} error(s)).";
                    result.ErrorCount = validation.ErrorCount;
                    result.WarningCount = validation.WarningCount;
                    result.CompleteTiming();
                    Persist(result);
                    return result;
                }

                SetOperation("Preparing output path");
                var outputFolder = PrepareOutputFolder(profile, confirmOverwrite, out var prepareError);
                if (!string.IsNullOrEmpty(prepareError))
                {
                    result.Result = BuildFlowResultStatus.Failed.ToString();
                    result.ErrorSummary = prepareError;
                    result.CompleteTiming();
                    Persist(result);
                    return result;
                }

                var configurator = GetConfigurator(profile.TargetPlatform);
                var locationPathName = configurator.ResolveLocationPathName(profile, outputFolder);
                result.OutputPath = locationPathName;

                ApplyPlayerVersion(profile);

                SetOperation($"Building {PlatformSupportUtility.GetDisplayName(profile.TargetPlatform)}");
                IDisposableBuildState restoreState = new NoopBuildState();
                BuildReport report = null;

                try
                {
                    configurator.ApplyPreBuildSettings(profile, out restoreState);

                    if (profile.NativeBuildProfile != null)
                    {
                        var profileOptions = new BuildPlayerWithProfileOptions
                        {
                            buildProfile = profile.NativeBuildProfile,
                            locationPathName = locationPathName,
                            options = BuildBuildOptions(profile)
                        };
                        report = BuildPipeline.BuildPlayer(profileOptions);
                    }
                    else
                    {
                        var scenes = BuildFlowSceneResolver.ResolveEnabledScenePaths(profile);
                        var options = new BuildPlayerOptions
                        {
                            scenes = scenes,
                            locationPathName = locationPathName,
                            target = PlatformSupportUtility.ToUnityBuildTarget(profile.TargetPlatform),
                            options = BuildBuildOptions(profile)
                        };
                        report = BuildPipeline.BuildPlayer(options);
                    }
                }
                finally
                {
                    restoreState?.Dispose();
                }

                SetOperation("Processing build report");
                MapReport(report, result);
                configurator.ValidateGeneratedOutput(locationPathName, result);

                var sizePath = Directory.Exists(outputFolder) ? outputFolder : locationPathName;
                result.SetOutput(locationPathName, DirectorySizeUtility.Calculate(sizePath));

                if (profile.TargetPlatform == BuildFlowTargetPlatform.IosXcodeProject &&
                    result.Status == BuildFlowResultStatus.Succeeded)
                {
                    result.Notes = string.IsNullOrEmpty(result.Notes)
                        ? "Xcode project export completed. This is not an IPA build."
                        : result.Notes;
                }

                result.CompleteTiming();
                Persist(result);
                return result;
            }
            catch (Exception ex)
            {
                result.Result = BuildFlowResultStatus.Failed.ToString();
                result.ErrorSummary = ex.Message;
                result.ErrorCount = Math.Max(1, result.ErrorCount);
                result.CompleteTiming();
                Persist(result);
                Debug.LogException(ex);
                return result;
            }
            finally
            {
                IsBuilding = false;
                SetOperation("Idle");
            }
        }

        private void Persist(BuildFlowBuildResult result)
        {
            _historyService.SaveResult(result);
            BuildCompleted?.Invoke(result);
        }

        private void SetOperation(string operation)
        {
            CurrentOperation = operation;
            OperationChanged?.Invoke(operation);
        }

        private static void ApplyPlayerVersion(BuildFlowProfile profile)
        {
            if (profile == null)
                return;

            PlayerSettings.bundleVersion = profile.ApplicationVersion;
        }

        private string PrepareOutputFolder(BuildFlowProfile profile, bool confirmOverwrite, out string error)
        {
            error = null;
            var root = PathUtility.ToAbsolutePath(profile.OutputDirectory);
            Directory.CreateDirectory(root);

            var artifactFolderName = ArtifactNaming.Resolve(profile.ArtifactNamingFormat, profile);
            var desired = Path.Combine(root, artifactFolderName);

            var exists = Directory.Exists(desired) || File.Exists(desired);
            if (!exists)
            {
                Directory.CreateDirectory(desired);
                return desired;
            }

            if (profile.CleanOutputBeforeBuild)
            {
                if (confirmOverwrite)
                {
                    var ok = EditorUtility.DisplayDialog(
                        "Clean Output",
                        $"Delete existing output at:\n{desired}\n\nand create a fresh build folder?",
                        "Clean and Build",
                        "Cancel");
                    if (!ok)
                    {
                        error = "Build cancelled: existing output was not cleaned.";
                        return null;
                    }
                }

                try
                {
                    if (Directory.Exists(desired))
                        Directory.Delete(desired, true);
                    if (File.Exists(desired))
                        File.Delete(desired);
                }
                catch (Exception ex)
                {
                    error = $"Failed to clean output directory: {ex.Message}";
                    return null;
                }

                Directory.CreateDirectory(desired);
                return desired;
            }

            if (profile.AllowOverwriteExistingOutput)
            {
                if (confirmOverwrite)
                {
                    var ok = EditorUtility.DisplayDialog(
                        "Overwrite Output",
                        $"Output already exists at:\n{desired}\n\nOverwrite existing build files?",
                        "Overwrite",
                        "Cancel");
                    if (!ok)
                    {
                        error = "Build cancelled: overwrite was not confirmed.";
                        return null;
                    }
                }

                Directory.CreateDirectory(desired);
                return desired;
            }

            var unique = PathUtility.EnsureUniqueDirectory(desired);
            Directory.CreateDirectory(unique);
            return unique;
        }

        private static BuildOptions BuildBuildOptions(BuildFlowProfile profile)
        {
            var options = BuildOptions.None;

            if (profile.DevelopmentBuild || profile.BuildConfiguration == BuildFlowBuildConfiguration.Development)
                options |= BuildOptions.Development;

            if (profile.ScriptDebugging)
                options |= BuildOptions.AllowDebugging;

            if (profile.AutoconnectProfiler)
                options |= BuildOptions.ConnectWithProfiler;

            switch (profile.CompressionMethod)
            {
                case BuildFlowCompressionMethod.Lz4:
                    options |= BuildOptions.CompressWithLz4;
                    break;
                case BuildFlowCompressionMethod.Lz4HC:
                    options |= BuildOptions.CompressWithLz4HC;
                    break;
            }

            return options;
        }

        private static void MapReport(BuildReport report, BuildFlowBuildResult result)
        {
            if (report == null)
            {
                result.Result = BuildFlowResultStatus.Failed.ToString();
                result.ErrorSummary = "BuildPipeline returned no BuildReport.";
                result.ErrorCount = 1;
                return;
            }

            var summary = report.summary;
            switch (summary.result)
            {
                case BuildResult.Succeeded:
                    result.Result = BuildFlowResultStatus.Succeeded.ToString();
                    break;
                case BuildResult.Failed:
                    result.Result = BuildFlowResultStatus.Failed.ToString();
                    break;
                case BuildResult.Cancelled:
                    result.Result = BuildFlowResultStatus.Cancelled.ToString();
                    break;
                default:
                    result.Result = BuildFlowResultStatus.Unknown.ToString();
                    break;
            }

            result.WarningCount = summary.totalWarnings;
            result.ErrorCount = summary.totalErrors;

            if (summary.result != BuildResult.Succeeded)
            {
                result.ErrorSummary = $"Unity BuildReport result: {summary.result}. Errors: {summary.totalErrors}, Warnings: {summary.totalWarnings}.";
            }
        }

        private IPlatformBuildConfigurator GetConfigurator(BuildFlowTargetPlatform platform)
        {
            switch (platform)
            {
                case BuildFlowTargetPlatform.WindowsStandaloneX64:
                    return _windows;
                case BuildFlowTargetPlatform.AndroidApk:
                case BuildFlowTargetPlatform.AndroidAab:
                    return _android;
                case BuildFlowTargetPlatform.IosXcodeProject:
                    return _ios;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
        }
    }
}
