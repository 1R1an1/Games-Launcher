using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Games_Launcher.Core.FD.FileDownloaderUtils;

namespace Games_Launcher.Core.FD
{
    public class FileDownloader : IDisposable
    {
        public event Action<DownloadState> OnStateChanged;
        public Func<bool, Task<bool>> AskResumeDecision;

        private CancellationTokenSource _cts;
        private CancellationToken _cToken;
        
        private AsyncManualResetEvent _pauseRequest = new AsyncManualResetEvent(true);
        private int i = 0;

        private readonly HttpClient _httpClient = new HttpClient
        (new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        public FileDownloader()
        {
            _cts = new CancellationTokenSource();
            _cToken = _cts.Token;
            _pauseRequest = new AsyncManualResetEvent(true);
        }

        public void Pause()
        {
            _pauseRequest.Reset();
            OnStateChanged?.Invoke(new DownloadState { Status = DownloadStatus.Paused });
        }
        public void Resume()
        {
            OnStateChanged?.Invoke(new DownloadState { Status = DownloadStatus.Resumed });
            _pauseRequest.Set();
            i = 0;
        }
        public void Cancel() => _cts.Cancel();

        public async Task DownloadFileWithResume(string url, string finalPath)
        {
            string tempPath = finalPath + ".tmp";
            long existingLength = GetExistingLenght(tempPath);

            HttpRequestMessage request = CreateHttpRequest(url, existingLength);

            HttpResponseMessage response = null;
            try { response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead); }
            catch (TaskCanceledException) { OnStateChanged?.Invoke(new DownloadState { Status = DownloadStatus.Canceled }); return; }

            try { existingLength = await HandleResumeSupportHttp(response, existingLength); }
            catch (OperationCanceledException) { OnStateChanged?.Invoke(new DownloadState { Status = DownloadStatus.Canceled }); return; }

            try
            {
                await DownloadCoreHttp(response, tempPath, existingLength);
                string final = GetAvailableFileName(finalPath);
                File.Move(tempPath, GetAvailableFileName(final));
                OnStateChanged?.Invoke(new DownloadState() { Status = DownloadStatus.Finished, FinalPath = GetAvailableFileName(final) });
            }
            catch (OperationCanceledException)
            {
                OnStateChanged?.Invoke(new DownloadState() { Status = DownloadStatus.CanceledUser });
            }
            catch (WebException we)
            {
                OnStateChanged?.Invoke(new DownloadState() { Status = DownloadStatus.ErrorNet, Error = we });
            }
            catch (IOException io)
            {
                OnStateChanged?.Invoke(new DownloadState() { Status = DownloadStatus.ErrorIO, Error = io });
            }
            catch (Exception ex)
            {
                OnStateChanged?.Invoke(new DownloadState() { Status = DownloadStatus.ErrorGeneral, Error = ex });
            }
        }

        private long GetExistingLenght(string tempPath)
        {
            if (!File.Exists(tempPath))
                return 0;

            long len = new FileInfo(tempPath).Length;
            OnStateChanged?.Invoke(new DownloadState { Status = DownloadStatus.TempFound, FileSize = len });
            return len;
        }
        private HttpRequestMessage CreateHttpRequest(string url, long existingLength)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (existingLength > 0)
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingLength, null);

            return request;
        }
        private async Task<long> HandleResumeSupportHttp(HttpResponseMessage response, long existingLength)
        {
            if (response.StatusCode == HttpStatusCode.PartialContent)
            {
                OnStateChanged?.Invoke(new DownloadState { Status = DownloadStatus.ResumeSupported });
                return existingLength;
            }
            else if (response.StatusCode != HttpStatusCode.PartialContent && existingLength == 0)
            {
                if (await AskResumeDecision?.Invoke(true) != true)
                    throw new OperationCanceledException();

                OnStateChanged?.Invoke(new DownloadState { Status = DownloadStatus.ResumeSupported, Tick = 1 });  
            }

            if (existingLength > 0)
            {
                if (await AskResumeDecision?.Invoke(false) != true)
                    throw new OperationCanceledException();


                OnStateChanged?.Invoke(new DownloadState { Status = DownloadStatus.ResumeNotSupported, Tick = 0 });
                return 0;
            }

            return existingLength;
        }
        private async Task DownloadCoreHttp(HttpResponseMessage response, string tempPath, long existingLenght)
        {
            using Stream responseStream = await response.Content.ReadAsStreamAsync();
            using FileStream fileStream = new FileStream(tempPath, FileMode.Append, FileAccess.Write, FileShare.None);

            long totalBytes = existingLenght;
            long fileSize = (response.Content.Headers.ContentLength ?? 0) + existingLenght;

            byte[] buffer = new byte[CalculateBufferSize(fileSize)];
            int bytesRead;

            long bytesLastSecond = 0;
            Stopwatch timer = Stopwatch.StartNew();

            OnStateChanged?.Invoke(new DownloadState() { Status = DownloadStatus.Downloading, FileSize = fileSize });

            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                _cToken.ThrowIfCancellationRequested();
                await _pauseRequest.WaitAsync();

                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalBytes += bytesRead;
                bytesLastSecond += bytesRead;

                if (timer.ElapsedMilliseconds >= 1000)
                {
                    OnStateChanged?.Invoke(new DownloadState
                    {
                        Status = DownloadStatus.Progress,
                        TotalBytes = totalBytes,
                        BytesLastSecond = bytesLastSecond,
                        FileSize = fileSize,
                        Tick = i
                    });

                    bytesLastSecond = 0;
                    timer.Restart();
                    i++;
                }
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

                OnStateChanged = null;
                AskResumeDecision = null;
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
