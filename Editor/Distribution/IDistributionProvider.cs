using FurkanGok.BuildFlow.Build;

namespace FurkanGok.BuildFlow.Distribution
{
    /// <summary>
    /// Extension point for distribution destinations such as SteamPipe.
    /// Steam consumes a successful Windows Standalone build; it is not a Unity BuildTarget.
    /// </summary>
    public interface IDistributionProvider
    {
        string Id { get; }
        string DisplayName { get; }
        BuildFlowTargetPlatform[] SupportedSourcePlatforms { get; }
        bool CanDistribute(BuildFlowBuildResult buildResult);
        DistributionResult Distribute(BuildFlowBuildResult buildResult);
    }

    public sealed class DistributionResult
    {
        public bool Succeeded;
        public string Message;
    }
}
