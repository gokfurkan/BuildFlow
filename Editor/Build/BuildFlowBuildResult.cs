using System;
using FurkanGok.BuildFlow.Utilities;

namespace FurkanGok.BuildFlow.Build
{
    [Serializable]
    public sealed class BuildFlowBuildResult
    {
        public string BuildId;
        public string ProfileId;
        public string ProfileName;
        public string Target;
        public string BuildConfiguration;
        public string ApplicationVersion;
        public string BuildNumber;
        public string StartTimeUtc;
        public string EndTimeUtc;
        public double DurationSeconds;
        public string Result;
        public string OutputPath;
        public long OutputSizeBytes;
        public string OutputSizeDisplay;
        public int WarningCount;
        public int ErrorCount;
        public string UnityVersion;
        public string EditorPlatform;
        public string ErrorSummary;
        public string ReportPath;
        public bool IsXcodeExportOnly;
        public string Notes;

        public BuildFlowResultStatus Status
        {
            get
            {
                if (Enum.TryParse(Result, true, out BuildFlowResultStatus status))
                    return status;
                return BuildFlowResultStatus.Unknown;
            }
        }

        public static BuildFlowBuildResult CreateSkeleton(BuildFlowProfile profile)
        {
            return new BuildFlowBuildResult
            {
                BuildId = Guid.NewGuid().ToString("N"),
                ProfileId = profile != null ? profile.ProfileId : string.Empty,
                ProfileName = profile != null ? profile.ProfileName : string.Empty,
                Target = profile != null
                    ? PlatformSupportUtility.GetDisplayName(profile.TargetPlatform)
                    : string.Empty,
                BuildConfiguration = profile != null ? profile.BuildConfiguration.ToString() : string.Empty,
                ApplicationVersion = profile != null ? profile.ApplicationVersion : string.Empty,
                BuildNumber = profile != null ? profile.BuildNumber : string.Empty,
                StartTimeUtc = DateTime.UtcNow.ToString("o"),
                UnityVersion = UnityEngine.Application.unityVersion,
                EditorPlatform = UnityEngine.Application.platform.ToString(),
                Result = BuildFlowResultStatus.Unknown.ToString(),
                IsXcodeExportOnly = profile != null &&
                                    profile.TargetPlatform == BuildFlowTargetPlatform.IosXcodeProject
            };
        }

        public void CompleteTiming()
        {
            var end = DateTime.UtcNow;
            EndTimeUtc = end.ToString("o");
            if (DateTime.TryParse(StartTimeUtc, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var start))
            {
                DurationSeconds = Math.Max(0, (end - start).TotalSeconds);
            }
        }

        public void SetOutput(string path, long sizeBytes)
        {
            OutputPath = path;
            OutputSizeBytes = sizeBytes;
            OutputSizeDisplay = DirectorySizeUtility.FormatBytes(sizeBytes);
        }
    }
}
