using System.Text.RegularExpressions;
using FurkanGok.BuildFlow.Utilities;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace FurkanGok.BuildFlow.Validation.Rules
{
    public sealed class AndroidSupportRule : IValidationRule
    {
        public string RuleId => "android.support";
        public string RuleName => "Android Build Support";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Android Build Support must be installed.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (!IsAndroid(profile))
                return NotApplicable();

            var passed = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android);
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? "Android Build Support is installed."
                    : "Android Build Support is not installed. Add the Android module in Unity Hub.",
                passed);
        }

        private static bool IsAndroid(BuildFlowProfile profile) =>
            profile != null &&
            (profile.TargetPlatform == BuildFlowTargetPlatform.AndroidApk ||
             profile.TargetPlatform == BuildFlowTargetPlatform.AndroidAab);

        private ValidationResult NotApplicable() =>
            new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);
    }

    public sealed class AndroidSdkNdkRule : IValidationRule
    {
        public string RuleId => "android.sdkNdk";
        public string RuleName => "Android SDK / NDK";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Android SDK and NDK configuration should be available.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null ||
                (profile.TargetPlatform != BuildFlowTargetPlatform.AndroidApk &&
                 profile.TargetPlatform != BuildFlowTargetPlatform.AndroidAab))
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            var sdk = PlatformSupportUtility.IsAndroidSdkConfigured();
            var ndk = PlatformSupportUtility.IsAndroidNdkConfigured();

            if (sdk && ndk)
            {
                return new ValidationResult(
                    RuleId, RuleName, Severity, Description,
                    "Android SDK and NDK paths are configured.",
                    true);
            }

            // Unity may embed/manage SDK paths without EditorPrefs; treat missing prefs as warning when module is installed.
            if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                return new ValidationResult(
                    RuleId, RuleName, ValidationSeverity.Warning, Description,
                    "Android module is installed, but SDK/NDK paths were not detected in EditorPrefs or environment variables. Verify External Tools settings before release builds.",
                    true);
            }

            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                $"SDK configured: {sdk}. NDK configured: {ndk}.",
                false);
        }
    }

    public sealed class AndroidBundleIdentifierRule : IValidationRule
    {
        private static readonly Regex BundleIdRegex = new Regex(
            @"^[a-zA-Z][a-zA-Z0-9_]*(\.[a-zA-Z][a-zA-Z0-9_]*)+$",
            RegexOptions.Compiled);

        public string RuleId => "android.bundleId";
        public string RuleName => "Android Bundle Identifier";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Android Bundle Identifier must be valid.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null ||
                (profile.TargetPlatform != BuildFlowTargetPlatform.AndroidApk &&
                 profile.TargetPlatform != BuildFlowTargetPlatform.AndroidAab))
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            var bundleId = profile.OverrideAndroidBundleIdentifier
                ? profile.AndroidBundleIdentifier
                : PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);

            var passed = !string.IsNullOrWhiteSpace(bundleId) && BundleIdRegex.IsMatch(bundleId);
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? $"Bundle Identifier '{bundleId}' is valid."
                    : $"Bundle Identifier '{bundleId}' is invalid.",
                passed);
        }
    }

    public sealed class AndroidVersionCodeRule : IValidationRule
    {
        public string RuleId => "android.versionCode";
        public string RuleName => "Android Version Code";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "Android version code / build number must be a positive integer.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null ||
                (profile.TargetPlatform != BuildFlowTargetPlatform.AndroidApk &&
                 profile.TargetPlatform != BuildFlowTargetPlatform.AndroidAab))
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            var passed = int.TryParse(profile.BuildNumber, out var code) && code > 0;
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                passed
                    ? $"Version code '{profile.BuildNumber}' is valid."
                    : $"Version code '{profile.BuildNumber}' must be a positive integer.",
                passed);
        }
    }

    public sealed class AndroidPackageFormatRule : IValidationRule
    {
        public string RuleId => "android.packageFormat";
        public string RuleName => "Android Package Format";
        public ValidationSeverity Severity => ValidationSeverity.Error;
        public string Description => "APK or AAB selection must be explicit and valid.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null ||
                (profile.TargetPlatform != BuildFlowTargetPlatform.AndroidApk &&
                 profile.TargetPlatform != BuildFlowTargetPlatform.AndroidAab))
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            var format = profile.TargetPlatform == BuildFlowTargetPlatform.AndroidAab ? "AAB" : "APK";
            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                $"Android package format is {format}.",
                true);
        }
    }

    public sealed class AndroidSigningRule : IValidationRule
    {
        public string RuleId => "android.signing";
        public string RuleName => "Android Signing";
        public ValidationSeverity Severity => ValidationSeverity.Warning;
        public string Description => "Release Android builds should use a configured keystore. Passwords are never displayed.";

        public ValidationResult Evaluate(BuildFlowProfile profile)
        {
            if (profile == null ||
                (profile.TargetPlatform != BuildFlowTargetPlatform.AndroidApk &&
                 profile.TargetPlatform != BuildFlowTargetPlatform.AndroidAab))
                return new ValidationResult(RuleId, RuleName, ValidationSeverity.Information, Description, "Not applicable.", true);

            if (profile.BuildConfiguration == BuildFlowBuildConfiguration.Development || profile.DevelopmentBuild)
            {
                return new ValidationResult(
                    RuleId, RuleName, ValidationSeverity.Information, Description,
                    "Development build: debug signing is acceptable.",
                    true);
            }

            var usesCustom = PlayerSettings.Android.useCustomKeystore;
            var hasKeystore = !string.IsNullOrWhiteSpace(PlayerSettings.Android.keystoreName);
            var hasKeyAlias = !string.IsNullOrWhiteSpace(PlayerSettings.Android.keyaliasName);

            if (usesCustom && hasKeystore && hasKeyAlias)
            {
                return new ValidationResult(
                    RuleId, RuleName, Severity, Description,
                    "Custom keystore and key alias are configured for release signing. Keystore passwords are not exposed by BuildFlow.",
                    true);
            }

            return new ValidationResult(
                RuleId, RuleName, Severity, Description,
                "Release Android build does not appear to have a complete custom keystore configuration. Configure signing in Player Settings before publishing. BuildFlow never reads or displays keystore passwords.",
                true);
        }
    }
}
