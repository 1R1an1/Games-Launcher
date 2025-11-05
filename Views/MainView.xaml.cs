using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Games_Launcher.Views
{
    /// <summary>
    /// Lógica de interacción para Window.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Executable Files (*.exe, *.ink)|*.exe, *.ink|All Files (*.*)|*.*";
            dialog.Title = "Selecciona un archivo ejecutable";

            bool? resultado = dialog.ShowDialog();

            if (resultado == true)
            {
                string rutaExe = dialog.FileName;
                // Aquí puedes guardarla en un archivo, en configuración, o devolverla
                AppIcon.Source = ObtenerIconoExe(rutaExe);


                _ = Task.Run(async () =>
                {
                    string nombreProceso = Path.GetFileNameWithoutExtension(rutaExe);
                    DateTime? inicio = null;

                    while (true)
                    {
                        var procesos = Process.GetProcessesByName(nombreProceso);

                        if (procesos.Length > 0 && inicio == null)
                        {
                            inicio = DateTime.Now;
                        }
                        else if (procesos.Length == 0 && inicio != null)
                        {
                            TimeSpan duracion = DateTime.Now - inicio.Value;

                            Console.WriteLine($"⏱ El programa estuvo abierto durante {duracion.TotalSeconds:F0} segundos.");

                            Dispatcher.Invoke(() =>
                            {
                                OpenTimeLBL.Content = duracion.ToString(@"hh\:mm\:ss");
                            });

                            inicio = null;
                        }

                        await Task.Delay(2000); // Revisa cada 2 segundos
                    }
                });
            }
        }

        public BitmapImage ObtenerIconoExe(string rutaExe)
        {
            if (string.IsNullOrEmpty(rutaExe) || !File.Exists(rutaExe))
                return null;

            Icon icon = Icon.ExtractAssociatedIcon(rutaExe);

            using (MemoryStream ms = new MemoryStream())
            {
                icon.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();

                return image;
            }
        }
    }
}
