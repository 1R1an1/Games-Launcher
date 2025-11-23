using Games_Launcher.Core;
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
    public partial class FileDownloaderView : UserControl
    {
        public FileDownloaderView()
        {
            InitializeComponent();
            ConsoleOutput.Document.Blocks.Clear();
            FileDownloadTBX.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }

        #region Consola
        public void Log(string message)
        {
            var paragraph = new Paragraph(new Run(message))
            {
                Foreground = Brushes.LightGray,
                Margin = new Thickness(0)
            };
            ConsoleOutput.Document.Blocks.Add(paragraph);
            ConsoleOutput.Dispatcher.InvokeAsync(() =>
            {
                ConsoleOutput.ScrollToEnd();
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
        public void Log(string message, SolidColorBrush color)
        {
            var paragraph = new Paragraph(new Run(message))
            {
                Foreground = color,
                Margin = new Thickness(0)
            };
            ConsoleOutput.Document.Blocks.Add(paragraph);
            ConsoleOutput.Dispatcher.InvokeAsync(() =>
            {
                ConsoleOutput.ScrollToEnd();
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
        public void Log(string message, Color color)
        {
            var paragraph = new Paragraph(new Run(message))
            {
                Foreground = new SolidColorBrush(color),
                Margin = new Thickness(0)
            };
            ConsoleOutput.Document.Blocks.Add(paragraph);
            ConsoleOutput.Dispatcher.InvokeAsync(() =>
            {
                ConsoleOutput.ScrollToEnd();
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
        public void RemoveLastLog()
        {
            var blocks = ConsoleOutput.Document.Blocks;
            if (blocks.Count > 0)
            {
                var last = blocks.LastBlock;
                blocks.Remove(last);
            }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FileDownloadTBX.Text) || string.IsNullOrWhiteSpace(FileNameTBX.Text) || string.IsNullOrWhiteSpace(FileURLTBX.Text))
            {
                MessageBox.Show("Por favor, completa todos los campos antes de iniciar la descarga.", "Campos incompletos", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            FileDownloadTBX.IsEnabled = false;
            FileDownloadTBX.Foreground = (Brush)FindResource("FontColorDisabled");
            FileNameTBX.IsEnabled = false;
            FileNameTBX.Foreground = (Brush)FindResource("FontColorDisabled");
            FileURLTBX.IsEnabled = false;
            FileURLTBX.Foreground = (Brush)FindResource("FontColorDisabled");
        }
    }
}
