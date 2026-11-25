using FurkanGok.BuildFlow.Build;

namespace FurkanGok.BuildFlow.Packaging
{
    /// <summary>
    /// Extension point for post-build artifact packaging (ZIP, SHA-256, BuildInfo.json, etc.).
    /// Packaging failures must remain distinguishable from Unity build failures.
    /// </summary>
    public interface IArtifactPackager
    {
        string Id { get; }
        string DisplayName { get; }
        bool CanPack(BuildFlowBuildResult buildResult);
        ArtifactPackagingResult Pack(BuildFlowBuildResult buildResult);
    }

    public sealed class ArtifactPackagingResult
    {
        public bool Succeeded;
        public string Message;
        public string OutputPath;
    }
}
