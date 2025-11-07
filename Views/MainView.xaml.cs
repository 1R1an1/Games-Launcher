using Games_Launcher.Core;
using Games_Launcher.Model;
using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.Linq;
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
            foreach (Game game in GamesInfo.Games)
            {
                CreateGame(game);
            }
        }

        private void BTNAgregar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
            dialog.Title = "Selecciona un archivo ejecutable";

            bool? resultado = dialog.ShowDialog();

            if (resultado == true)
            {
                Game newGame = new Game
                {
                    Name = Path.GetFileNameWithoutExtension(dialog.FileName),
                    Path = dialog.FileName,
                    PlayTime = new System.TimeSpan(0)
                };
                GamesInfo.Games.Add(newGame);

                CreateGame(GamesInfo.Games.Last());
            }
        }

        private void CreateGame(Game game) { var gamee = new GameView(game) { Margin = new Thickness(5) }; Juegos.Children.Add(gamee); }

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
