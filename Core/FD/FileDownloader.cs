using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static Games_Launcher.Core.FD.FileDownloaderUtils;

namespace Games_Launcher.Core.FD
{
    public class FileDownloader : IDisposable
    {
        private CancellationTokenSource _cts;
        private CancellationToken _cToken;
        private AsyncManualResetEvent _pauseRequest = new AsyncManualResetEvent(true);
        private int i = 0;

        // Eventos para comunicar a la UI
        public event Action OnRemoveAllLogs;
        public event Action OnRemoveLastLog;
        public event Action<string> OnLog;
        public event Action<string, Color> OnLogC;
        public event Action<long, long, long, int> OnProgress;
        public event Action onFinish;
        public event Action onDownloadStarter;

        public FileDownloader()
        {
            _cts = new CancellationTokenSource();
            _cToken = _cts.Token;
            _pauseRequest = new AsyncManualResetEvent(true);
        }

        public void Pause()
        {
            _pauseRequest.Reset();
            OnLogC?.Invoke("La descarga ha sido pausada.", Colors.Cyan);
        }
        public async Task Resume()
        {
            OnLogC?.Invoke("Reanudando descarga...", Colors.Cyan);
            await Task.Delay(1000);
            OnRemoveLastLog?.Invoke();
            OnRemoveLastLog?.Invoke();
            OnRemoveLastLog?.Invoke();
            _pauseRequest.Set();
            i = 0;
        }
        public void Cancel() => _cts.Cancel();

        public async Task DownloadFileWithResume(string url, string finalPath)
        {
            //OnLog?.Invoke($"Iniciando descarga desde: {url}");
            string tempPath = finalPath + ".tmp";
            long existingLength = 0;

            if (File.Exists(tempPath))
            {
                existingLength = new FileInfo(tempPath).Length;
                OnLog?.Invoke($"Archivo temporal encontrado. Reanudando desde {CalculateFileSize(existingLength).Value:0.00} {CalculateFileSize(existingLength).Key}...");
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
                        OnLog?.Invoke("El servidor soporta la reanudación de la descarga.");
                    }
                    else if (existingLength > 0)
                    {
                        if (MessageBox.Show("El servidor no soporta la reanudación de la descarga. ¿Deseas continuar desde el principio (S/N)?", "", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                        {
                            OnLog?.Invoke("Descarga cancelada.");
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
                        long totalBytes = existingLength;
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        long bytesLastSecond = 0;
                        Stopwatch secondTimer = Stopwatch.StartNew();

                        long fileSize = response.ContentLength + existingLength;
                        var fileSizeInfo = CalculateFileSize(fileSize);

                        byte[] buffer = new byte[CalculateBufferSize(fileSize)];
                        int bytesRead;

                        //OnRemoveAllLogs?.Invoke();
                        await Task.Delay(1000);
                        OnLog?.Invoke("Descargando...");
                        await Task.Delay(200);
                        OnLog?.Invoke($"Tamaño total del archivo: {fileSizeInfo.Value:0.00} {fileSizeInfo.Key}");
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
                                    OnProgress?.Invoke(totalBytes, bytesLastSecond, fileSize, i);
                                    bytesLastSecond = 0;
                                    secondTimer.Restart();
                                    i++;
                                }
                            }
                        }
                        catch (IOException io)
                        {
                            OnLogC?.Invoke($"[Error de E/S] {io.Message}. Descarga cancelada por fallo de conexión.", Colors.Red);
                            onFinish?.Invoke();
                        }

                        stopwatch.Stop();
                    }

                }
                File.Move(tempPath, finalPath);
                OnLogC?.Invoke($"\nDescarga completada. Archivo guardado como: {finalPath}", Colors.LightGreen);
                onFinish?.Invoke();
            }
            catch (OperationCanceledException)
            {
                OnLogC?.Invoke("\nDescarga cancelada por el usuario.", Colors.Cyan);
                onFinish?.Invoke();
            }
            catch (WebException we)
            {
                OnLogC?.Invoke($"\n[Error de red] {we.Message}. La descarga puede reanudarse más tarde.", Colors.Red);
                onFinish?.Invoke();
            }
            catch (Exception ex)
            {
                OnLogC?.Invoke($"\n[Error general] {ex.Message}", Colors.Red);
                onFinish?.Invoke();
            }
        }

        #region Disposable implementation
        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _cts?.Cancel();

                _cts?.Dispose();
                _cts = null;
                _pauseRequest = null;

                OnRemoveAllLogs = null;
                OnLog = null;
                OnLogC = null;
                OnProgress = null;
                onFinish = null;
                onDownloadStarter = null;
            }

            _disposed = true;
        }

        ~FileDownloader()
        {
            Dispose(false);
        }
        #endregion
    }
}
