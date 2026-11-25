using System.IO;

namespace FurkanGok.BuildFlow.Utilities
{
    public static class DirectorySizeUtility
    {
        public static long Calculate(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return 0;

            if (File.Exists(path))
                return new FileInfo(path).Length;

            if (!Directory.Exists(path))
                return 0;

            long total = 0;
            try
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        total += new FileInfo(file).Length;
                    }
                    catch
                    {
                        // Skip locked/unreadable files.
                    }
                }
            }
            catch
            {
                return total;
            }

            return total;
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            double kb = bytes / 1024.0;
            if (kb < 1024)
                return $"{kb:0.##} KB";
            double mb = kb / 1024.0;
            if (mb < 1024)
                return $"{mb:0.##} MB";
            double gb = mb / 1024.0;
            return $"{gb:0.##} GB";
        }
    }
}
