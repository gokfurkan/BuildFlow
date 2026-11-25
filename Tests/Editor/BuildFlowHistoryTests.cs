using System.IO;
using FurkanGok.BuildFlow.Build;
using FurkanGok.BuildFlow.History;
using FurkanGok.BuildFlow.Utilities;
using NUnit.Framework;
using UnityEngine;

namespace FurkanGok.BuildFlow.Tests
{
    public sealed class BuildFlowHistoryTests
    {
        [Test]
        public void BuildReportGenerator_RoundTripsJson()
        {
            var result = new BuildFlowBuildResult
            {
                BuildId = "abc123",
                ProfileId = "profile1",
                ProfileName = "Windows Release",
                Target = "Windows Standalone x64",
                BuildConfiguration = "Release",
                ApplicationVersion = "0.1.0",
                BuildNumber = "1",
                Result = BuildFlowResultStatus.Succeeded.ToString(),
                OutputPath = "Builds/Test",
                WarningCount = 2,
                ErrorCount = 0,
                UnityVersion = Application.unityVersion,
                EditorPlatform = Application.platform.ToString()
            };
            result.SetOutput("Builds/Test", 2048);

            var json = BuildReportGenerator.Serialize(result);
            Assert.IsTrue(json.Contains("abc123"));
            Assert.IsTrue(json.Contains("Succeeded"));

            var restored = JsonUtility.FromJson<BuildFlowBuildResult>(json);
            Assert.AreEqual(result.BuildId, restored.BuildId);
            Assert.AreEqual(result.OutputSizeBytes, restored.OutputSizeBytes);
            Assert.AreEqual(BuildFlowResultStatus.Succeeded, restored.Status);
        }

        [Test]
        public void BuildHistoryService_PersistsAndLoads()
        {
            var service = new BuildHistoryService();
            var result = new BuildFlowBuildResult
            {
                BuildId = System.Guid.NewGuid().ToString("N"),
                ProfileName = "HistoryTest",
                Result = BuildFlowResultStatus.Failed.ToString(),
                ErrorSummary = "unit-test",
                ApplicationVersion = "0.1.0",
                BuildNumber = "9"
            };

            service.SaveResult(result);
            Assert.IsTrue(File.Exists(result.ReportPath));

            var loaded = BuildReportGenerator.ReadJsonReport(result.ReportPath);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(result.BuildId, loaded.BuildId);
            Assert.AreEqual(BuildFlowResultStatus.Failed, loaded.Status);
        }

        [Test]
        public void ResultStatus_DefaultsToUnknownForInvalidValue()
        {
            var result = new BuildFlowBuildResult { Result = "not-a-status" };
            Assert.AreEqual(BuildFlowResultStatus.Unknown, result.Status);
        }

        [Test]
        public void PathUtility_HistoryDirectory_IsUnderLibrary()
        {
            var history = PathUtility.GetHistoryDirectory().Replace('\\', '/');
            Assert.IsTrue(history.Contains("Library/BuildFlow/History"));
        }
    }
}
