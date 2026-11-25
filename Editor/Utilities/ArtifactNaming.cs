using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FurkanGok.BuildFlow.Utilities
{
    public static class ArtifactNaming
    {
        private static readonly Regex InvalidFileChars = new Regex(
            $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]",
            RegexOptions.Compiled);

        public static string Resolve(string format, BuildFlowProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            var template = string.IsNullOrWhiteSpace(format)
                ? BuildFlowConstants.DefaultArtifactFormat
                : format.Trim();

            var resolved = template
                .Replace("{name}", Sanitize(profile.ProfileName))
                .Replace("{version}", Sanitize(profile.ApplicationVersion))
                .Replace("{build}", Sanitize(profile.BuildNumber))
                .Replace("{platform}", Sanitize(PlatformToToken(profile.TargetPlatform)))
                .Replace("{config}", Sanitize(profile.BuildConfiguration.ToString().ToLowerInvariant()))
                .Replace("{id}", Sanitize(profile.ProfileId));

            return string.IsNullOrWhiteSpace(resolved) ? "build" : resolved;
        }

        public static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unnamed";

            var cleaned = InvalidFileChars.Replace(value.Trim(), "_");
            cleaned = Regex.Replace(cleaned, @"\s+", "_");
            return cleaned;
        }

        public static string PlatformToToken(BuildFlowTargetPlatform platform)
        {
            switch (platform)
            {
                case BuildFlowTargetPlatform.WindowsStandaloneX64:
                    return "windows-x64";
                case BuildFlowTargetPlatform.AndroidApk:
                    return "android-apk";
                case BuildFlowTargetPlatform.AndroidAab:
                    return "android-aab";
                case BuildFlowTargetPlatform.IosXcodeProject:
                    return "ios-xcode";
                default:
                    return "unknown";
            }
        }

        public static bool IsValidExecutableBaseName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var sanitized = Sanitize(name);
            return sanitized.Equals(name.Trim().Replace(' ', '_'), StringComparison.OrdinalIgnoreCase)
                   || !InvalidFileChars.IsMatch(name);
        }
    }
}
