using System.Text.RegularExpressions;
using FurkanGok.BuildFlow.Utilities;
using UnityEditor;
using UnityEditor.Build;

namespace FurkanGok.BuildFlow.Validation.Rules
{
    public sealed class IosSupportRule : IValidationRule
    {
        public string RuleId => "ios.support";
        public string RuleName => "iOS Build Support";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "iOS Build Support must be installed.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null || profile.TargetPlatform != BuildFlowTargetPlatform.IosXcodeProject)
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            var passed = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS);
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? "iOS Build Support is installed."
                    : "iOS Build Support is not installed. Add the iOS module in Unity Hub.",
                passed);
        }
    }

    public sealed class IosBundleIdentifierRule : IValidationRule
    {
        private static readonly Regex BundleIdRegex = new Regex(
            @"^[a-zA-Z][a-zA-Z0-9_]*(\.[a-zA-Z][a-zA-Z0-9_]*)+$",
            RegexOptions.Compiled);

        public string RuleId => "ios.bundleId";
        public string RuleName => "iOS Bundle Identifier";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "iOS Bundle Identifier must be valid.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null || profile.TargetPlatform != BuildFlowTargetPlatform.IosXcodeProject)
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            var bundleId = profile.OverrideIosBundleIdentifier
                ? profile.IosBundleIdentifier
                : PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS);

            var passed = !string.IsNullOrWhiteSpace(bundleId) && BundleIdRegex.IsMatch(bundleId);
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? $"Bundle Identifier '{bundleId}' is valid."
                    : $"Bundle Identifier '{bundleId}' is invalid.",
                passed);
        }
    }

    public sealed class IosExportDirectoryRule : IValidationRule
    {
        public string RuleId => "ios.exportDirectory";
        public string RuleName => "iOS Xcode Export Directory";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Xcode export directory must be valid.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null || profile.TargetPlatform != BuildFlowTargetPlatform.IosXcodeProject)
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            var passed = PathUtility.IsValidDirectoryPath(profile.OutputDirectory);
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? "Xcode export directory path is valid."
                    : "Xcode export directory path is invalid.",
                passed);
        }
    }

    public sealed class IosVersionRule : IValidationRule
    {
        public string RuleId => "ios.version";
        public string RuleName => "iOS Version / Build Number";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "iOS version and build number must be valid.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null || profile.TargetPlatform != BuildFlowTargetPlatform.IosXcodeProject)
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            var versionOk = !string.IsNullOrWhiteSpace(profile.ApplicationVersion);
            var buildOk = !string.IsNullOrWhiteSpace(profile.BuildNumber);
            var passed = versionOk && buildOk;
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? $"iOS version '{profile.ApplicationVersion}' / build '{profile.BuildNumber}'."
                    : "iOS version and build number must both be set.",
                passed);
        }
    }

    public sealed class IosMacLimitationRule : IValidationRule
    {
        public string RuleId => "ios.macosLimitation";
        public string RuleName => "iOS Compilation Requirements";
        public ValidationSeverity Severity => ValidationSeverity.Information;
        public string Description => "IPA compilation requires macOS and Xcode.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null || profile.TargetPlatform != BuildFlowTargetPlatform.IosXcodeProject)
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            var onMac = PlatformSupportUtility.IsRunningOnMacOs();
            var message = onMac
                ? "Running on macOS. BuildFlow will export an Xcode project. Compiling, signing, and exporting an IPA still require Xcode and are outside this Unity export step."
                : "This Editor is not running on macOS. BuildFlow can still generate an Xcode project folder from Unity when iOS support is installed, but compiling, signing, and exporting an IPA require macOS and Xcode. A successful Xcode project export is not an IPA build.";

            return new ValidationResult(RuleId, RuleName, Severity, Description, message, true);
        }
    }
}
