using System;
using System.Collections.Generic;
using System.IO;

namespace Games_Launcher.Core.FD
{
    public static class FileDownloaderUtils
    {
        public enum Size
        {
            B,
            KB,
            MB,
            GB
        }
        public enum ResumeSupport
        {
            True,
            False,
            Unknown
        }
        public static int CalculateBufferSize(long fileSize)
        {
            int bufferSize = (int)(fileSize / 100);
            return Clamp(bufferSize, 16 * 1024, 256 * 1024);
        }
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
        public static KeyValuePair<Size, double> CalculateFileSize(long bytes)
        {
            if (bytes >= 1024 * 1024 * 1024)
                return new KeyValuePair<Size, double>(Size.GB, (bytes / (1024.0 * 1024 * 1024)));
            else if (bytes >= 1024 * 1024)
                return new KeyValuePair<Size, double>(Size.MB, (bytes / (1024.0 * 1024)));
            else if (bytes >= 1024)
                return new KeyValuePair<Size, double>(Size.KB, (bytes / (1024.0)));
            else if (bytes > 0 && bytes < 1024)
                return new KeyValuePair<Size, double>(Size.B, bytes);
            else
                return new KeyValuePair<Size, double>(Size.B, 0);
        }
        public static string FormatFileSize(long bytes)
        {
            var sizePair = CalculateFileSize(bytes);
            return $"{sizePair.Value:F2} {sizePair.Key}";
        }
        public static void FormatETAAndSpeed(DownloadState obj, out string etaString, out string speedString)
        {
            double speed = obj.BytesLastSecond / 1024.0; // KB/s
            speedString = speed >= 1024
                ? $"{(speed / 1024):0.00} MB/s"
                : $"{speed:0.00} KB/s";

            double etaSeconds = (obj.FileSize - obj.TotalBytes) / (speed * 1024);
            etaString = TimeSpan.FromSeconds(etaSeconds).ToString("hh\\:mm\\:ss");
        }

        public static string GetAvailableFileName(string fullPath)
        {
            string directory = Path.GetDirectoryName(fullPath) ?? "";
            string filename = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);

            string result = fullPath;
            int counter = 1;

            while (File.Exists(result))
            {
                result = Path.Combine(directory, $"{filename} ({counter}){extension}");
                counter++;
            }

            return result;
        }
    }
}
