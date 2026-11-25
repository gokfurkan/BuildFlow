using UnityEditor;

namespace FurkanGok.BuildFlow.Build.Platform
{
    public interface IPlatformBuildConfigurator
    {
        BuildFlowTargetPlatform Platform { get; }
        string ResolveLocationPathName(BuildFlowProfile profile, string outputFolder);
        void ApplyPreBuildSettings(BuildFlowProfile profile, out IDisposableBuildState restoreState);
        void ValidateGeneratedOutput(string locationPathName, BuildFlowBuildResult result);
    }

    public interface IDisposableBuildState : System.IDisposable
    {
    }

    public sealed class NoopBuildState : IDisposableBuildState
    {
        public void Dispose()
        {
        }
    }
}
