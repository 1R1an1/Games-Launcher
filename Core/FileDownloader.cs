using Games_Launcher.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
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

        // void Main()
        //{
        //    Console.Write("Ingresa la URL del archivo: ");
        //    string url = Console.ReadLine();

        //    Console.Write("Nombre del archivo destino (ej. archivo.zip): ");
        //    string finalPath = Console.ReadLine();

        //    Downloader.DownloadFileWithResume(url, finalPath);

        //    Console.WriteLine("Pulse una tecla para cerrar el programa . . .");
        //    Console.ReadKey();
        //}
    }


    public static class Downloader
    {
        public static int CalculateBufferSize(long fileSize)
        {
            int bufferSize = (int)(fileSize / 100);
            return bufferSize;
        }

        public static KeyValuePair<Size, double> CalculateFileSize(long bytes)
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

        public static void DownloadFileWithResume(FileDownloaderView viewLogs, string url, string finalPath)
        {
            viewLogs.Log($"Iniciando descarga desde: {url}");
            string tempPath = finalPath + ".tmp";
            long existingLength = 0;

            if (File.Exists(tempPath))
            {
                existingLength = new FileInfo(tempPath).Length;
                viewLogs.Log($"Archivo temporal encontrado. Reanudando desde {existingLength} bytes...");
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
                        if (MessageBox.Show("El servidor no soporta la reanudación de la descarga. \n¿Deseas continuar desde el principio (S/N)?", "", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
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
                        long totalBytes = existingLength;
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        long bytesLastSecond = 0;
                        Stopwatch secondTimer = Stopwatch.StartNew();

                        long fileSize = response.ContentLength + existingLength;
                        var fileSizeInfo = CalculateFileSize(fileSize);
                        viewLogs.Log("");
                        viewLogs.Log($"Tamaño total del archivo: {fileSizeInfo.Value:0.00} {fileSizeInfo.Key}");

                        byte[] buffer = new byte[CalculateBufferSize(fileSize)];
                        int bytesRead;

                        viewLogs.Log("Descargando...");

                        int i = 0;
                        try
                        {
                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
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

                                    viewLogs.Log($"Progreso: {progress.Value:0.00} {progress.Key} / {totalSize.Value:0.00} {totalSize.Key} | Velocidad: {speedDisplay}       ");
                                    bytesLastSecond = 0;
                                    secondTimer.Restart();
                                    i++;
                                }
                            }
                        }
                        catch (IOException io)
                        {
                            viewLogs.Log($"[Error de E/S] {io.Message}. Descarga pausada por fallo de conexión.");
                        }

                        stopwatch.Stop();
                    }

                }
                File.Move(tempPath, finalPath);
                viewLogs.Log($"\nDescarga completada. Archivo guardado como: {finalPath}", Colors.LightGreen);
            }
            catch (OperationCanceledException)
            {
                viewLogs.Log("\nDescarga cancelada por el usuario.");
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
