using Games_Launcher.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Games_Launcher.Core
{
    public enum Size
    {
        B,
        KB,
        MB,
        GB
    }

    public class FileDownloader
    {
        private CancellationTokenSource _cts;
        private CancellationToken _cToken;
        private AsyncManualResetEvent _pauseRequest = new AsyncManualResetEvent(true);
        private FileDownloaderView viewLogs;
        private int i = 0;

        public event Action onFinish;
        public event Action onDownloadStarter;

        public FileDownloader(FileDownloaderView _viewLogs)
        {
            viewLogs = _viewLogs;
            _cts = new CancellationTokenSource();
            _cToken = _cts.Token;
            _pauseRequest = new AsyncManualResetEvent(true);
        }

        public void Pause()
        {
            _pauseRequest.Reset();
            viewLogs.Log("La descarga ha sido pausada.", Colors.Cyan);
        }
        public async Task Resume()
        {
            viewLogs.ClearLogs();
            viewLogs.Log("Reanudando descarga...", Colors.Cyan);
            await Task.Delay(1000);
            _pauseRequest.Set();
            i = 0;
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
            else if (bytes > 0 && bytes < 1024)
                return new KeyValuePair<Size, double>(Size.B, bytes);
            else
                return new KeyValuePair<Size, double>(Size.B, 0);
        }

        public async Task DownloadFileWithResume(string url, string finalPath)
        {
            viewLogs.Log($"Iniciando descarga desde: {url}");
            string tempPath = finalPath + ".tmp";
            long existingLength = 0;

            if (File.Exists(tempPath))
            {
                existingLength = new FileInfo(tempPath).Length;
                viewLogs.Log($"\nArchivo temporal encontrado. Reanudando desde {existingLength} bytes...");
                await Task.Delay(1000);
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AddRange(existingLength);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
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
                            onFinish?.Invoke();
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
                        _pauseRequest = new AsyncManualResetEvent(true);

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
                        await Task.Delay(1000);
                        viewLogs.Log("Descargando...");
                        await Task.Delay(200);
                        viewLogs.Log($"Tamaño total del archivo: {fileSizeInfo.Value:0.00} {fileSizeInfo.Key}");
                        await Task.Delay(500);

                        onDownloadStarter?.Invoke();
                        i = 0;
                        try
                        {
                            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                _cToken.ThrowIfCancellationRequested();
                                await _pauseRequest.WaitAsync();

                                await fileStream.WriteAsync(buffer, 0, bytesRead);
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
                            onFinish?.Invoke();
                        }

                        stopwatch.Stop();
                    }

                }
                File.Move(tempPath, finalPath);
                viewLogs.Log($"\nDescarga completada. Archivo guardado como: {finalPath}", Colors.LightGreen);
                onFinish?.Invoke();
            }
            catch (OperationCanceledException)
            {
                viewLogs.Log("\nDescarga cancelada por el usuario.", Colors.Cyan);
                onFinish?.Invoke();
            }
            catch (WebException we)
            {
                viewLogs.Log($"\n[Error de red] {we.Message}. La descarga puede reanudarse más tarde.", Colors.Red);
                onFinish?.Invoke();
            }
            catch (Exception ex)
            {
                viewLogs.Log($"\n[Error general] {ex.Message}", Colors.Red);
                onFinish?.Invoke();
            }
        }
    }
}
