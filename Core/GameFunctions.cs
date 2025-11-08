using Games_Launcher.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Games_Launcher.Core
{
    public static class GameFunctions
    {
        public static BitmapImage GetGameIcon(string gamePath)
        {
            if (string.IsNullOrEmpty(gamePath) || !File.Exists(gamePath))
                return new BitmapImage(new Uri("pack://application:,,,/ErrorImg.png"));

            System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(gamePath);

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
        public static string ConvertTime(TimeSpan time)
        {
            if (time.TotalSeconds > 100)
            {
                if (time.TotalMinutes > 100)
                {
                    return $"{time.TotalHours:F1} horas";
                }
                else
                {
                    return $"{time.TotalMinutes:F1} minutos";
                }
            }
            else
            {
                return $"{time.TotalSeconds:F1} segundos";
            }
        }

        public static bool? SelectGamePath(out string path)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Executable Files (*.exe)|*.exe";
            dialog.Title = "Selecciona un archivo ejecutable";

            bool? a = dialog.ShowDialog();
            path = dialog.FileName;

            return a;
        }
    }
}
