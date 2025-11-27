using System.Windows.Media;
using System.Windows.Threading;

namespace Games_Launcher.Core.FD
{
    public interface IFileDownloaderView
    {
        void Log(string message);
        void Log(string message, Color color);
        void RemoveLastLog();
        void DownloadStarter();
        void FinishDownload();
        Dispatcher Dispatcher { get; }
    }
}
