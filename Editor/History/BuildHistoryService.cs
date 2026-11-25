using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FurkanGok.BuildFlow.Build;
using FurkanGok.BuildFlow.Utilities;
using UnityEngine;

namespace FurkanGok.BuildFlow.History
{
    [Serializable]
    public sealed class BuildHistoryIndex
    {
        public List<string> ReportFiles = new List<string>();
    }

    public sealed class BuildHistoryService
    {
        private const string IndexFileName = "history_index.json";

        public string HistoryDirectory => PathUtility.GetHistoryDirectory();

        public void SaveResult(BuildFlowBuildResult result)
        {
            if (result == null)
                return;

            Directory.CreateDirectory(HistoryDirectory);
            var reportPath = BuildReportGenerator.WriteJsonReport(result);
            var index = LoadIndex();
            if (!index.ReportFiles.Contains(Path.GetFileName(reportPath)))
                index.ReportFiles.Insert(0, Path.GetFileName(reportPath));
            SaveIndex(index);
        }

        public IReadOnlyList<BuildFlowBuildResult> LoadHistory()
        {
            Directory.CreateDirectory(HistoryDirectory);
            var results = new List<BuildFlowBuildResult>();

            foreach (var file in Directory.GetFiles(HistoryDirectory, "build_*.json")
                         .OrderByDescending(File.GetLastWriteTimeUtc))
            {
                try
                {
                    var result = BuildReportGenerator.ReadJsonReport(file);
                    if (result != null)
                    {
                        if (string.IsNullOrEmpty(result.ReportPath))
                            result.ReportPath = file;
                        results.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[BuildFlow] Failed to read history entry '{file}': {ex.Message}");
                }
            }

            return results;
        }

        public BuildFlowBuildResult GetLatest()
        {
            return LoadHistory().FirstOrDefault();
        }

        public int CountByStatus(BuildFlowResultStatus status)
        {
            return LoadHistory().Count(r => r.Status == status);
        }

        private BuildHistoryIndex LoadIndex()
        {
            var path = Path.Combine(HistoryDirectory, IndexFileName);
            if (!File.Exists(path))
                return new BuildHistoryIndex();

            try
            {
                return JsonUtility.FromJson<BuildHistoryIndex>(File.ReadAllText(path)) ?? new BuildHistoryIndex();
            }
            catch
            {
                return new BuildHistoryIndex();
            }
        }

        private void SaveIndex(BuildHistoryIndex index)
        {
            var path = Path.Combine(HistoryDirectory, IndexFileName);
            File.WriteAllText(path, JsonUtility.ToJson(index, true));
        }
    }
}
