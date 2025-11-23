using Games_Launcher.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Games_Launcher.Core
{
    public enum Size
    {
        KB,
        MB,
        GB
    }

    public class FileDownloader
    {
        private CancellationTokenSource _cts;
        private CancellationToken _cToken;
        private ManualResetEvent _pauseRequest = new ManualResetEvent(true);
        private FileDownloaderView viewLogs;
        private int i = 0;

        public FileDownloader(FileDownloaderView _viewLogs)
        {
            viewLogs = _viewLogs;
            _cts = new CancellationTokenSource();
            _cToken = _cts.Token;
            _pauseRequest = new ManualResetEvent(true);
        }

        public void Pause()
        {
            _pauseRequest.Reset();
            viewLogs.ClearLogs();
            viewLogs.Log("La descarga ha sido pausada.", Colors.Cyan);
        }
        public void Resume()
        {
            _pauseRequest.Set();
            viewLogs.Log("Reanudando descarga...", Colors.Cyan);
        }
        public void Cancel() => _cts.Cancel();

        private int CalculateBufferSize(long fileSize)
        {
            int bufferSize = (int)(fileSize / 100);
            return Clamp(bufferSize, 16 * 1024, 256 * 1024);
        }
        private int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
        private KeyValuePair<Size, double> CalculateFileSize(long bytes)
        {
            if (bytes >= 1024 * 1024 * 1024)
                return new KeyValuePair<Size, double>(Size.GB, (bytes / (1024.0 * 1024 * 1024)));
            else if (bytes >= 1024 * 1024)
                return new KeyValuePair<Size, double>(Size.MB, (bytes / (1024.0 * 1024)));
            else if (bytes >= 1024)
                return new KeyValuePair<Size, double>(Size.KB, (bytes / (1024.0)));
            else
                return new KeyValuePair<Size, double>(Size.KB, 1);
        }

        public void DownloadFileWithResume(string url, string finalPath)
        {
            viewLogs.Log($"Iniciando descarga desde: {url}");
            string tempPath = finalPath + ".tmp";
            long existingLength = 0;

            if (File.Exists(tempPath))
            {
                existingLength = new FileInfo(tempPath).Length;
                viewLogs.Log($"\nArchivo temporal encontrado. Reanudando desde {existingLength} bytes...");
                Thread.Sleep(1000);
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AddRange(existingLength);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    // Comprobar si el servidor soporta la reanudación
                    if (response.StatusCode == HttpStatusCode.PartialContent && existingLength > 0)
                    {
                        viewLogs.Log("El servidor soporta la reanudación de la descarga.");
                    }
                    else if (existingLength > 0)
                    {
                        if (MessageBox.Show("El servidor no soporta la reanudación de la descarga. ¿Deseas continuar desde el principio (S/N)?", "", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                        {
                            viewLogs.Log("Descarga cancelada.");
                            return;
                        }
                        else
                        {
                            existingLength = 0;  // Reiniciar la descarga
                        }
                    }

                    using (Stream responseStream = response.GetResponseStream())
                    using (FileStream fileStream = new FileStream(tempPath, FileMode.Append, FileAccess.Write, FileShare.None))
                    {
                        _cts = new CancellationTokenSource();
                        _cToken = _cts.Token;
                        _pauseRequest = new ManualResetEvent(true);

                        long totalBytes = existingLength;
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        long bytesLastSecond = 0;
                        Stopwatch secondTimer = Stopwatch.StartNew();

                        long fileSize = response.ContentLength + existingLength;
                        var fileSizeInfo = CalculateFileSize(fileSize);

                        byte[] buffer = new byte[CalculateBufferSize(fileSize)];
                        int bytesRead;

                        viewLogs.ClearLogs();
                        Thread.Sleep(1000);
                        viewLogs.Log("Descargando...");
                        Thread.Sleep(200);
                        viewLogs.Log($"Tamaño total del archivo: {fileSizeInfo.Value:0.00} {fileSizeInfo.Key}");
                        Thread.Sleep(500);

                        i = 0;
                        try
                        {
                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                _cToken.ThrowIfCancellationRequested();
                                _pauseRequest.WaitOne();

                                fileStream.Write(buffer, 0, bytesRead);
                                totalBytes += bytesRead;
                                bytesLastSecond += bytesRead;

                                // Mostrar progreso cada segundo
                                if (secondTimer.ElapsedMilliseconds >= 1000)
                                {
                                    if (i > 0)
                                        viewLogs.RemoveLastLog();
                                    double speed = bytesLastSecond / 1024.0; // KB/s
                                    string speedDisplay = speed >= 1024
                                        ? $"{(speed / 1024):0.00} MB/s"
                                        : $"{speed:0.00} KB/s";

                                    var progress = CalculateFileSize(totalBytes);
                                    var totalSize = CalculateFileSize(fileSize);

                                    viewLogs.Log($"Progreso: {progress.Value:0.00} {progress.Key} / {totalSize.Value:0.00} {totalSize.Key} | Velocidad: {speedDisplay}");
                                    bytesLastSecond = 0;
                                    secondTimer.Restart();
                                    i++;
                                }
                            }
                        }
                        catch (IOException io)
                        {
                            viewLogs.Log($"[Error de E/S] {io.Message}. Descarga cancelada por fallo de conexión.", Colors.Red);
                        }

                        stopwatch.Stop();
                    }

                }
                File.Move(tempPath, finalPath);
                viewLogs.Log($"\nDescarga completada. Archivo guardado como: {finalPath}", Colors.LightGreen);
            }
            catch (OperationCanceledException)
            {
                viewLogs.Log("\nDescarga cancelada por el usuario.", Colors.Cyan);
            }
            catch (WebException we)
            {
                viewLogs.Log($"\n[Error de red] {we.Message}. La descarga puede reanudarse más tarde.", Colors.Red);
            }
            catch (Exception ex)
            {
                viewLogs.Log($"\n[Error general] {ex.Message}", Colors.Red);
            }
        }
    }
}
