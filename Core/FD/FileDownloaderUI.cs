using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static Games_Launcher.Core.FD.FileDownloaderUtils;

namespace Games_Launcher.Core.FD
{
    public class FileDownloaderUI
    {
        private IFileDownloaderView _view;

        public FileDownloaderUI(IFileDownloaderView view, FileDownloader fd)
        {
            _view = view;
            fd.OnStateChanged += Fd_OnStateChanged;
            fd.AskResumeDecision = Fd_AskResumeDecision;
        }

        private void Fd_OnStateChanged(DownloadState obj)
        {
            switch (obj.Status)
            {
                case DownloadStatus.TempFound:
                    _view.Log($"Archivo temporal encontrado. Reanudando desde {FormatFileSize(obj.FileSize)}...");
                    break;
                case DownloadStatus.ResumeSupported:
                    _view.Log("El servidor soporta la reanudación de la descarga.");
                    break;
                case DownloadStatus.ResumeNotSupported:
                    _view.Log(obj.Tick == 0
                              ? "El servidor no soporta reanudación. Iniciando descarga desde cero..."
                              : obj.Tick == 1
                              ? "El servidor no soporta reanudacion. Continuando con la descarga..."
                              : "No se pudo determinar si el servidor soporta la reanudacion de la descarga.. Continuando con la descarga...", Colors.OrangeRed);
                    break;
                case DownloadStatus.Downloading:
                    _view.Log($"Tamaño total del archivo: {FormatFileSize(obj.FileSize)}\nDescargando archivo...");
                    _view.DownloadStarter();
                    break;
                case DownloadStatus.Progress:
                    if (obj.Tick > 0)
                        _view.RemoveLastLog();

                    FormatETAAndSpeed(obj, out string etaString, out string speedString);

                    _view.Log($"[ESTADO DE DESCARGA]\n" +
                        $"  • Progreso     : {FormatFileSize(obj.TotalBytes)} / {FormatFileSize(obj.FileSize)}\n" +
                        $"  • Velocidad    : {speedString}\n" +
                        $"  • T. Estimado  : {etaString}");
                    break;
                case DownloadStatus.Paused:
                    _view.Log("\nLa descarga ha sido pausada.", Colors.Cyan);
                    break;
                case DownloadStatus.Resumed:
                    _view.Log("Reanudando descarga...", Colors.Cyan);
                    _view.RemoveLastLog();
                    _view.RemoveLastLog();
                    _view.RemoveLastLog();
                    break;
                case DownloadStatus.Finished:
                    _view.Log($"\nDescarga completada. Archivo guardado como: {obj.FinalPath}", Colors.LightGreen);
                    _view.FinishDownload();
                    break;
                case DownloadStatus.ErrorGeneral:
                    _view.Log($"\n[Error general] {obj.Error.Message}", Colors.Red);
                    _view.FinishDownload();
                    break;
                case DownloadStatus.ErrorNet:
                    _view.Log($"\n[Error de red] {obj.Error.Message}. La descarga puede reanudarse más tarde.", Colors.Red);
                    _view.FinishDownload();
                    break;
                case DownloadStatus.ErrorIO:
                    _view.Log($"\n[Error de E/S] {obj.Error.Message}.", Colors.Red);
                    _view.FinishDownload();
                    break;
                case DownloadStatus.CanceledUser:
                    _view.Log("\nDescarga cancelada por el usuario.", Colors.Cyan);
                    _view.FinishDownload();
                    break;
                case DownloadStatus.Canceled:
                    _view.Log("Descarga cancelada.", Colors.OrangeRed);
                    _view.FinishDownload();
                    break;
            }
        }
        private async Task<bool> Fd_AskResumeDecision(byte i)
        {
            return await _view.Dispatcher.InvokeAsync(() =>
            {
                var result = MessageBox.Show(
                    i == 0
                    ? "El servidor no soporta la reanudación de la descarga. ¿Deseas continuar desde el principio (S/N)?"
                    : i == 1 ?
                    "El servidor no soporta la reanudación de la descarga. ¿Deseas continuar con la descarga?":
                    "No se pudo determinar si el servidor soporta la reanudacion de la descarga. ¿Deseas continuar igualmente con la descarga?",
                    "Reanudar descarga",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                return result == MessageBoxResult.Yes;
            });
        }
    }
}
