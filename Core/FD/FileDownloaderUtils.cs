using System.Collections.Generic;

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
    }
}
