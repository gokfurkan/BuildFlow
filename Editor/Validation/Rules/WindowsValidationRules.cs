using System.IO;
using FurkanGok.BuildFlow.Utilities;
using UnityEditor;

namespace FurkanGok.BuildFlow.Validation.Rules
{
    public sealed class WindowsSupportRule : IValidationRule
    {
        public string RuleId => "windows.support";
        public string RuleName => "Windows Standalone Support";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Windows Standalone build support must be available.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null || profile.TargetPlatform != BuildFlowTargetPlatform.WindowsStandaloneX64)
                return NotApplicable();

            var passed = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? "Windows Standalone support is available."
                    : "Windows Standalone support is not installed.",
                passed);
        }

        private ValidationResult NotApplicable() =>
            new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);
    }

    public sealed class WindowsArchitectureRule : IValidationRule
    {
        public string RuleId => "windows.architecture";
        public string RuleName => "Windows Architecture";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Windows builds must target StandaloneWindows64 (x64).";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null || profile.TargetPlatform != BuildFlowTargetPlatform.WindowsStandaloneX64)
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            var target = PlatformSupportUtility.ToUnityBuildTarget(profile.TargetPlatform);
            var passed = target == BuildTarget.StandaloneWindows64;
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed ? "Target architecture is x64." : "Target architecture is not x64.",
                passed);
        }
    }

    public sealed class WindowsOutputPathRule : IValidationRule
    {
        public string RuleId => "windows.output";
        public string RuleName => "Windows Output Path";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Windows output path must resolve to a valid EXE location.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null || profile.TargetPlatform != BuildFlowTargetPlatform.WindowsStandaloneX64)
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            var root = PathUtility.ToAbsolutePath(profile.OutputDirectory);
            var artifact = ArtifactNaming.Resolve(profile.ArtifactNamingFormat, profile);
            var exeName = string.IsNullOrWhiteSpace(profile.WindowsExecutableName)
                ? artifact
                : ArtifactNaming.Sanitize(profile.WindowsExecutableName);
            var output = Path.Combine(root, artifact, exeName + ".exe");
            var passed = PathUtility.IsValidDirectoryPath(Path.GetDirectoryName(output));
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed ? $"EXE output path: {output}" : "Windows EXE output path is invalid.",
                passed);
        }
    }

    public sealed class WindowsExeNameRule : IValidationRule
    {
        public string RuleId => "windows.exeName";
        public string RuleName => "Windows EXE Name";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Windows executable base name must be valid.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null || profile.TargetPlatform != BuildFlowTargetPlatform.WindowsStandaloneX64)
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            var name = profile.WindowsExecutableName;
            var passed = !string.IsNullOrWhiteSpace(name) &&
                         name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
                         !name.EndsWith(".exe", System.StringComparison.OrdinalIgnoreCase);

            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? $"Executable name '{name}' is valid."
                    : $"Executable name '{name}' is invalid. Provide a base name without extension.",
                passed);
        }
    }
}
