namespace FurkanGok.BuildFlow.Distribution
{
    /// <summary>
    /// Documents the planned Steam distribution workflow for a future BuildFlow release.
    /// No upload controls are exposed in v0.1.0.
    /// </summary>
    public static class SteamDistributionRoadmap
    {
        public const string ProviderId = "steam.steampipe";
        public const string DisplayName = "Steam (SteamPipe)";

        public const string Summary =
            "Steam is a distribution destination for successful Windows Standalone builds. " +
            "It is not a Unity BuildTarget.";

        public static readonly string[] PlannedCapabilities =
        {
            "Steam AppID configuration",
            "Depot ID mapping",
            "Content staging from Windows build output",
            "VDF configuration generation",
            "SteamCMD upload orchestration",
            "Beta branch selection",
            "Explicit release confirmation before live publish"
        };
    }
}
