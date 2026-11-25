using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FurkanGok.BuildFlow.Profiles;
using FurkanGok.BuildFlow.Utilities;
using UnityEditor;

namespace FurkanGok.BuildFlow.Validation.Rules
{
    public sealed class ProfileExistsRule : IValidationRule
    {
        public string RuleId => "general.profile.exists";
        public string RuleName => "Profile Exists";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "A BuildFlow profile must be selected.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            var passed = profile != null;
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed ? "Profile is available." : "No BuildFlow profile is selected.",
                passed);
        }
    }

    public sealed class ProfileConfigurationRule : IValidationRule
    {
        public string RuleId => "general.profile.configuration";
        public string RuleName => "Profile Configuration";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Profile identity and naming must be configured.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null)
                return SkipMissing();

            var issues = new System.Collections.Generic.List<string>();
            if (string.IsNullOrWhiteSpace(profile.ProfileName))
                issues.Add("Profile name is empty.");
            if (string.IsNullOrWhiteSpace(profile.ProfileId))
                issues.Add("Profile ID is missing.");
            if (string.IsNullOrWhiteSpace(profile.ArtifactNamingFormat))
                issues.Add("Artifact naming format is empty.");

            var passed = issues.Count == 0;
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed ? "Profile configuration is valid." : string.Join(" ", issues),
                passed);
        }

        private ValidationResult SkipMissing() =>
            new ValidationResult(RuleId, RuleName, Severity, Description, "Skipped: profile missing.", false);
    }

    public sealed class ApplicationVersionRule : IValidationRule
    {
        private static readonly Regex SemVerLike = new Regex(
            @"^\d+(\.\d+){0,3}([-+][0-9A-Za-z\.-]+)?$",
            RegexOptions.Compiled);

        public string RuleId => "general.version";
        public string RuleName => "Application Version";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Application version must be a valid version string.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null)
                return new ValidationResult(RuleId, RuleName, Severity, Description, "Skipped: profile missing.", false);

            var version = profile.ApplicationVersion?.Trim();
            var passed = !string.IsNullOrWhiteSpace(version) && SemVerLike.IsMatch(version);
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? $"Version '{version}' is valid."
                    : $"Application version '{version}' is invalid. Use a numeric version such as 1.0.0.",
                passed);
        }
    }

    public sealed class BuildNumberRule : IValidationRule
    {
        public string RuleId => "general.buildNumber";
        public string RuleName => "Build Number";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Build number must be a positive integer.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null)
                return new ValidationResult(RuleId, RuleName, Severity, Description, "Skipped: profile missing.", false);

            var passed = int.TryParse(profile.BuildNumber, out var number) && number > 0;
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? $"Build number '{profile.BuildNumber}' is valid."
                    : $"Build number '{profile.BuildNumber}' must be a positive integer.",
                passed);
        }
    }

    public sealed class SceneListRule : IValidationRule
    {
        public string RuleId => "general.scenes.enabled";
        public string RuleName => "Enabled Scenes";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "At least one enabled scene must be included.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null)
                return new ValidationResult(RuleId, RuleName, Severity, Description, "Skipped: profile missing.", false);

            var enabled = BuildFlowSceneResolver.ResolveEnabledScenePaths(profile);
            var passed = enabled.Length > 0;
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? $"Found {enabled.Length} enabled scene(s)."
                    : "No enabled scenes are configured for this profile.",
                passed);
        }
    }

    public sealed class SceneExistsRule : IValidationRule
    {
        public string RuleId => "general.scenes.exists";
        public string RuleName => "Scene Assets Exist";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Every referenced scene must exist on disk.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null)
                return new ValidationResult(RuleId, RuleName, Severity, Description, "Skipped: profile missing.", false);

            var missing = BuildFlowSceneResolver.ResolveEntries(profile)
                .Where(e => !string.IsNullOrEmpty(e.Path) && e.Enabled && !e.Exists)
                .Select(e => e.Path)
                .ToList();

            var nullEntries = BuildFlowSceneResolver.ResolveEntries(profile)
                .Count(e => string.IsNullOrEmpty(e.Path));

            var passed = missing.Count == 0 && nullEntries == 0;
            var message = passed
                ? "All referenced scenes exist."
                : $"Missing or invalid scenes: {string.Join(", ", missing)}" +
                  (nullEntries > 0 ? $" ({nullEntries} null scene reference(s))." : ".");

            return new ValidationResult(RuleId, RuleName, Severity, Description, message, passed);
        }
    }

    public sealed class DisabledScenesInformationRule : IValidationRule
    {
        public string RuleId => "general.scenes.disabled";
        public string RuleName => "Disabled Scenes";
        public ValidationSeverity Severity => ValidationSeverity.Information;
        public string Description => "Reports disabled scenes that will be excluded.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null)
                return new ValidationResult(RuleId, RuleName, Severity, Description, "Skipped: profile missing.", true);

            var disabled = BuildFlowSceneResolver.ResolveEntries(profile).Count(e => !e.Enabled && !string.IsNullOrEmpty(e.Path));
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                disabled > 0
                    ? $"{disabled} disabled scene(s) will be excluded from the build."
                    : "No disabled scenes detected.",
                true);
        }
    }

    public sealed class OutputDirectoryRule : IValidationRule
    {
        public string RuleId => "general.output.valid";
        public string RuleName => "Output Directory";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Output directory path must be valid.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null)
                return new ValidationResult(RuleId, RuleName, Severity, Description, "Skipped: profile missing.", false);

            var passed = PathUtility.IsValidDirectoryPath(profile.OutputDirectory);
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? $"Output directory '{profile.OutputDirectory}' is valid."
                    : $"Output directory '{profile.OutputDirectory}' is invalid.",
                passed);
        }
    }

    public sealed class OutputDirectoryWritableRule : IValidationRule
    {
        public string RuleId => "general.output.writable";
        public string RuleName => "Output Directory Writable";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Output directory must be writable.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null)
                return new ValidationResult(RuleId, RuleName, Severity, Description, "Skipped: profile missing.", false);

            if (!PathUtility.IsValidDirectoryPath(profile.OutputDirectory))
                return new ValidationResult(RuleId, RuleName, Severity, Description, "Output path is invalid.", false);

            var passed = PathUtility.IsDirectoryWritable(profile.OutputDirectory);
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed ? "Output directory is writable." : "Output directory is not writable.",
                passed);
        }
    }

    public sealed class TargetSupportInstalledRule : IValidationRule
    {
        public string RuleId => "general.target.support";
        public string RuleName => "Target Build Support";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Unity build support for the selected target must be installed.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null)
                return new ValidationResult(RuleId, RuleName, Severity, Description, "Skipped: profile missing.", false);

            var supported = PlatformSupportUtility.IsBuildTargetSupported(profile.TargetPlatform);
            var display = PlatformSupportUtility.GetDisplayName(profile.TargetPlatform);
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                supported
                    ? $"{display} build support is installed."
                    : $"{display} build support is not installed. Install it via Unity Hub > Installs > Add Modules.",
                supported);
        }
    }

    public sealed class OutputOverwriteRule : IValidationRule
    {
        public string RuleId => "general.output.overwrite";
        public string RuleName => "Output Overwrite Safety";
        public ValidationSeverity Severity => ValidationSeverity.Warning;
        public string Description => "Warns when existing output may be overwritten.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null)
                return new ValidationResult(RuleId, RuleName, Severity, Description, "Skipped: profile missing.", true);

            var artifact = ArtifactNaming.Resolve(profile.ArtifactNamingFormat, profile);
            var outputRoot = PathUtility.ToAbsolutePath(profile.OutputDirectory);
            var candidate = Path.Combine(outputRoot, artifact);
            var exists = Directory.Exists(candidate) || File.Exists(candidate) ||
                         File.Exists(candidate + ".exe") ||
                         File.Exists(candidate + ".apk") ||
                         File.Exists(candidate + ".aab");

            if (!exists)
            {
                return new ValidationResult(
                    RuleId, RuleName, Severity, Description,
                    "No existing output conflict detected.",
                    true);
            }

            if (profile.AllowOverwriteExistingOutput || profile.CleanOutputBeforeBuild)
            {
                return new ValidationResult(
                    RuleId, RuleName, Severity, Description,
                    "Existing output will be cleaned or overwritten by explicit profile settings.",
                    true);
            }

            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                $"Output already exists at '{candidate}'. BuildFlow will use a unique path unless overwrite is enabled.",
                true);
        }
    }

    public sealed class NativeBuildProfileMismatchRule : IValidationRule
    {
        public string RuleId => "general.nativeProfile.mismatch";
        public string RuleName => "Native Build Profile Platform";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Optional Unity Build Profile must match the selected BuildFlow platform.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null)
                return new ValidationResult(RuleId, RuleName, Severity, Description, "Skipped: profile missing.", false);

            if (profile.NativeBuildProfile == null)
            {
                return new ValidationResult(
                    RuleId, RuleName, ValidationSeverity.Information, Description,
                    "No native Unity Build Profile referenced.",
                    true);
            }

            if (!PlatformSupportUtility.TryGetNativeBuildProfileTarget(profile.NativeBuildProfile, out var nativeTarget))
            {
                return new ValidationResult(
                    RuleId, RuleName, ValidationSeverity.Warning, Description,
                    "Could not read platform from the referenced Unity Build Profile.",
                    true);
            }

            var expected = PlatformSupportUtility.ToUnityBuildTarget(profile.TargetPlatform);
            var passed = nativeTarget == expected;
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? $"Native Build Profile target ({nativeTarget}) matches BuildFlow platform."
                    : $"Native Build Profile targets {nativeTarget}, but BuildFlow profile targets {expected}.",
                passed);
        }
    }
}
