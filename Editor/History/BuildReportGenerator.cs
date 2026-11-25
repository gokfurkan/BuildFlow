using System.IO;
using FurkanGok.BuildFlow.Build;
using FurkanGok.BuildFlow.Utilities;
using UnityEngine;

namespace FurkanGok.BuildFlow.History
{
    public static class BuildReportGenerator
    {
        public static string WriteJsonReport(BuildFlowBuildResult result)
        {
            if (result == null)
                return null;

            var directory = PathUtility.GetHistoryDirectory();
            Directory.CreateDirectory(directory);

            var fileName = $"build_{result.BuildId}.json";
            var path = Path.Combine(directory, fileName);
            var json = JsonUtility.ToJson(result, true);
            File.WriteAllText(path, json);
            result.ReportPath = path;
            return path;
        }

        public static BuildFlowBuildResult ReadJsonReport(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<BuildFlowBuildResult>(json);
        }

        public static string Serialize(BuildFlowBuildResult result)
        {
            return JsonUtility.ToJson(result, true);
        }
    }
}
