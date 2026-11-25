using System;
using System.IO;
using UnityEngine;

namespace FurkanGok.BuildFlow.Utilities
{
    public static class PathUtility
    {
        public static string GetProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }

        public static string GetHistoryDirectory()
        {
            return Path.Combine(GetProjectRoot(), BuildFlowConstants.HistoryFolderRelative);
        }

        public static string ToAbsolutePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            if (Path.IsPathRooted(path))
                return Path.GetFullPath(path);

            return Path.GetFullPath(Path.Combine(GetProjectRoot(), path));
        }

        public static string ToProjectRelativePath(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
                return string.Empty;

            var full = Path.GetFullPath(absolutePath);
            var root = GetProjectRoot();
            if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                var relative = full.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return relative.Replace('\\', '/');
            }

            return full;
        }

        public static bool IsValidDirectoryPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                var full = ToAbsolutePath(path);
                var root = Path.GetPathRoot(full);
                if (string.IsNullOrEmpty(root))
                    return false;

                var invalid = Path.GetInvalidPathChars();
                return full.IndexOfAny(invalid) < 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsDirectoryWritable(string path)
        {
            try
            {
                var full = ToAbsolutePath(path);
                Directory.CreateDirectory(full);

                var probe = Path.Combine(full, $".buildflow_write_probe_{Guid.NewGuid():N}.tmp");
                File.WriteAllText(probe, "probe");
                File.Delete(probe);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string EnsureUniqueDirectory(string desiredPath)
        {
            var full = ToAbsolutePath(desiredPath);
            if (!Directory.Exists(full) && !File.Exists(full))
                return full;

            var parent = Path.GetDirectoryName(full) ?? GetProjectRoot();
            var name = Path.GetFileName(full);
            var index = 1;
            string candidate;
            do
            {
                candidate = Path.Combine(parent, $"{name}_{index}");
                index++;
            } while (Directory.Exists(candidate) || File.Exists(candidate));

            return candidate;
        }

        public static void OpenInFileBrowser(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            var full = ToAbsolutePath(path);
            if (File.Exists(full))
            {
                EditorUtilitySafeReveal(full);
                return;
            }

            if (!Directory.Exists(full))
                Directory.CreateDirectory(full);

            EditorUtilitySafeReveal(full);
        }

        private static void EditorUtilitySafeReveal(string fullPath)
        {
            UnityEditor.EditorUtility.RevealInFinder(fullPath);
        }
    }
}
