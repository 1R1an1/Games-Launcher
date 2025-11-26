using Games_Launcher.Views;
using System;
using System.Windows;
using System.Windows.Media;
using static Games_Launcher.Core.FD.FileDownloaderUtils;

namespace Games_Launcher.Core.FD
{
    public class FileDownloaderUI
    {
        private FileDownloaderView _view;

        public FileDownloaderUI(FileDownloaderView view, FileDownloader fd)
        {
            _view = view;
            fd.OnStateChanged += Fd_OnStateChanged;
            fd.AskResumeDecision = async () =>
            {
                return await _view.Dispatcher.InvokeAsync(() =>
                {
                    var result = MessageBox.Show(
                        "El servidor no soporta la reanudación de la descarga. ¿Deseas continuar desde el principio (S/N)?",
                        "Reanudar descarga",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    return result == MessageBoxResult.Yes;
                });
            };
        }

        private void Fd_OnStateChanged(DownloadState obj)
        {
            switch (obj.Status)
            {
                case DownloadStatus.TempFound:
                    _view.Log($"Archivo temporal encontrado. Reanudando desde {CalculateFileSize(obj.FileSize).Value:0.00} {CalculateFileSize(obj.FileSize).Key}...");
                    break;
                case DownloadStatus.ResumeSupported:
                    _view.Log("El servidor soporta la reanudación de la descarga.");
                    break;
                case DownloadStatus.ResumeNotSupported:
                    _view.Log("El servidor no soporta reanudación. Iniciando descarga desde cero...", Colors.OrangeRed);
                    break;
                case DownloadStatus.Downloading:
                    _view.Log($"Tamaño total del archivo: {CalculateFileSize(obj.FileSize).Value:0.00} {CalculateFileSize(obj.FileSize).Key}\nDescargando archivo...");
                    _view.DownloadStarter();
                    break;
                case DownloadStatus.Progress:
                    if (obj.Tick > 0)
                        _view.RemoveLastLog();

                    double speed = obj.BytesLastSecond / 1024.0; // KB/s
                    string speedDisplay = speed >= 1024
                        ? $"{(speed / 1024):0.00} MB/s"
                        : $"{speed:0.00} KB/s";

                    double remaining = obj.FileSize - obj.TotalBytes;
                    double etaSeconds = remaining / (speed * 1024);

                    TimeSpan etaTimeSpan = TimeSpan.FromSeconds(etaSeconds);
                    var progress = CalculateFileSize(obj.TotalBytes);
                    var totalSize = CalculateFileSize(obj.FileSize);

                    _view.Log($"[ESTADO DE DESCARGA]\n" +
                        $"  • Progreso     : {progress.Value:0.00} / {totalSize.Value:0.00} {progress.Key}\n" +
                        $"  • Velocidad    : {speedDisplay}\n" +
                        $"  • T. Estimado  : {etaTimeSpan:hh\\:mm\\:ss}");
                    break;
                case DownloadStatus.Paused:
                    _view.Log("La descarga ha sido pausada.", Colors.Cyan);
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
    }
}
