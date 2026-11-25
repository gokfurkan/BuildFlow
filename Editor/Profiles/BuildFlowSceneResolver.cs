using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FurkanGok.BuildFlow.Profiles
{
    public readonly struct BuildFlowSceneEntry
    {
        public BuildFlowSceneEntry(string path, bool enabled, bool exists)
        {
            Path = path;
            Enabled = enabled;
            Exists = exists;
        }

        public string Path { get; }
        public bool Enabled { get; }
        public bool Exists { get; }
    }

    public static class BuildFlowSceneResolver
    {
        public static List<BuildFlowSceneEntry> ResolveEntries(BuildFlowProfile profile)
        {
            var entries = new List<BuildFlowSceneEntry>();
            if (profile == null)
                return entries;

            if (profile.UseEditorBuildSettingsScenes)
            {
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    var exists = !string.IsNullOrEmpty(scene.path) && File.Exists(scene.path);
                    entries.Add(new BuildFlowSceneEntry(scene.path, scene.enabled, exists));
                }

                return entries;
            }

            foreach (var sceneAsset in profile.Scenes)
            {
                if (sceneAsset == null)
                {
                    entries.Add(new BuildFlowSceneEntry(string.Empty, false, false));
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(sceneAsset);
                var exists = !string.IsNullOrEmpty(path) && File.Exists(path);
                entries.Add(new BuildFlowSceneEntry(path, true, exists));
            }

            return entries;
        }

        public static string[] ResolveEnabledScenePaths(BuildFlowProfile profile)
        {
            var paths = new List<string>();
            foreach (var entry in ResolveEntries(profile))
            {
                if (entry.Enabled && entry.Exists && !string.IsNullOrEmpty(entry.Path))
                    paths.Add(entry.Path);
            }

            return paths.ToArray();
        }
    }
}
