using System;

namespace Games_Launcher.Core.FD
{
    public enum DownloadStatus
    {
        //Starting,
        //CheckingTemp,
        TempFound,
        //CreatingRequest,
        ResumeSupported,
        ResumeNotSupported,
        Downloading,
        Progress,
        Paused,
        Resumed,
        Finished,
        ErrorGeneral,
        ErrorNet,
        ErrorIO,
        CanceledUser,
        Canceled
    }
    public class DownloadState
    {
        public DownloadStatus Status { get; set; }
        public Exception Error { get; set; }
        public string FinalPath { get; set; }


        public long TotalBytes { get; set; }
        public long BytesLastSecond { get; set; }
        public long FileSize { get; set; }
        public int Tick { get; set; }
    }
}
