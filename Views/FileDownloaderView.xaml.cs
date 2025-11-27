using Games_Launcher.Core.FD;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Games_Launcher.Views
{
    /// <summary>
    /// Lógica de interacción para FileDownloaderView.xaml
    /// </summary>
    public partial class FileDownloaderView : UserControl, IFileDownloaderView
    {
        public FileDownloader _fd;
        public FileDownloaderUI _fdUI;
        private bool _finishDownload = false;
        private bool _pausedDownload = false;

        public FileDownloaderView()
        {
            InitializeComponent();
            FileDownloadTBX.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            
            ConsoleOutput.Document.Blocks.Clear();
            Log("File Downloader iniciado.", Colors.LightGreen);
            
            _fd = new FileDownloader();
            _fdUI = new FileDownloaderUI(this, _fd);
        }
        public void FinishDownload()
        {
            _fd.Dispose();
            Dispatcher.Invoke(() =>
            {
                _finishDownload = true;
                GRIDControlersDownload.Visibility = Visibility.Collapsed;
                BTNDownload.Visibility = Visibility.Visible;
                SetEnabledControl(BTNDownload, true);
                BTNDownload.Content = "Cerrar";
            });
        }
        public void DownloadStarter()
        {
            Dispatcher.Invoke(() =>
            {
                GRIDControlersDownload.Visibility = Visibility.Visible;
                BTNDownload.Visibility = Visibility.Collapsed;
            });
        }

        #region Consola
        public void Log(string message)
        {
            ConsoleOutput.Dispatcher.Invoke(() =>
            {
                var paragraph = new Paragraph(new Run(message))
                {
                    Foreground = Brushes.LightGray,
                    Margin = new Thickness(0)
                };
                ConsoleOutput.Document.Blocks.Add(paragraph);
                ConsoleOutput.ScrollToEnd();
            });
        }
        public void Log(string message, SolidColorBrush color)
        {
            ConsoleOutput.Dispatcher.Invoke(() =>
            {
                var paragraph = new Paragraph(new Run(message))
                {
                    Foreground = color,
                    Margin = new Thickness(0)
                };
                ConsoleOutput.Document.Blocks.Add(paragraph);
                ConsoleOutput.ScrollToEnd();
            });
        }
        public void Log(string message, Color color)
        {
            ConsoleOutput.Dispatcher.Invoke(() =>
            {
                var paragraph = new Paragraph(new Run(message))
                {
                    Foreground = new SolidColorBrush(color),
                    Margin = new Thickness(0)
                };
                ConsoleOutput.Document.Blocks.Add(paragraph);
                ConsoleOutput.ScrollToEnd();
            });
        }
        public void RemoveLastLog()
        {
            ConsoleOutput.Dispatcher.Invoke(() =>
            {
                var blocks = ConsoleOutput.Document.Blocks;
                if (blocks.Count > 0)
                {
                    var last = blocks.LastBlock;
                    blocks.Remove(last);
                }
            });
        }
        public void ClearLogs()
        {
            ConsoleOutput.Dispatcher.Invoke(() =>
            {
                ConsoleOutput.Document.Blocks.Clear();
            });
        }
        #endregion

        private void SelectGamePathBTN_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            //new Form.OpenFileDialog().
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok && Directory.Exists(dialog.FileName))
                FileDownloadTBX.Text = dialog.FileName;
        }

        private void FileURLTBX_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Uri uri = new Uri(FileURLTBX.Text);
                foreach (var item in uri.Segments)
                {
                    var parts = item.Trim('/').Split('.');
                    if (parts.Length > 1 && parts.All(p => !string.IsNullOrEmpty(p)) && string.IsNullOrWhiteSpace(FileNameTBX.Text))
                    {
                        FileNameTBX.Text = item.Trim('/');
                        break;
                    }
                }
            }
            catch { return; }

        }

        private void BTNDownload_Click(object sender, RoutedEventArgs e)
        {
            if (_finishDownload)
            {
                Window.GetWindow(this).Close();
                return;
            }

            if (string.IsNullOrWhiteSpace(FileDownloadTBX.Text) || string.IsNullOrWhiteSpace(FileNameTBX.Text) || string.IsNullOrWhiteSpace(FileURLTBX.Text))
            {
                MessageBox.Show("Por favor, completa todos los campos antes de iniciar la descarga.", "Campos incompletos", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SetEnabledControl(FileDownloadTBX, false);
            SetEnabledControl(FileNameTBX, false);
            SetEnabledControl(FileURLTBX, false);
            SetEnabledControl(BTNDownload, false);
            SetEnabledControl(SelectGamePathBTN, false);

            string url = FileURLTBX.Text;
            string path = Path.Combine(FileDownloadTBX.Text, FileNameTBX.Text);
            
            _ = _fd.DownloadFileWithResume(url, path);
        }

        private void BTNCancel_Click(object sender, RoutedEventArgs e)
        {
            _fd.Cancel();
        }

        private void BTNPause_Click(object sender, RoutedEventArgs e)
        {
            if (_pausedDownload)
            {
                _pausedDownload = false;
                BTNPause.Content = "Pausar";
                SetEnabledControl(BTNCancel, true);
                _fd.Resume();
            }
            else
            {
                _pausedDownload = true;
                BTNPause.Content = "Reanudar";
                SetEnabledControl(BTNCancel, false);
                _fd.Pause();
            }
        }

        private void SetEnabledControl(Control element, bool SetEnabledValue)
        {
            if (!SetEnabledValue)
            {
                element.IsEnabled = false;
                element.Tag = (Brush)FindResource("NormalColorDisabled");
                element.Foreground = (Brush)FindResource("FontColorDisabled");
            }
            else
            {
                element.IsEnabled = true;
                element.Tag = (Brush)FindResource("NormalColorNormal");
                element.Foreground = (Brush)FindResource("FontColor");
            }
        }
    }
}
