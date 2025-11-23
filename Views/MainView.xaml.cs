using Games_Launcher.Core;
using Games_Launcher.Model;
using Games_Launcher.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            if (GameFunctions.SelectGamePath(out string path) == true)
            {
                Game newGame = new Game
                {
                    Name = Path.GetFileNameWithoutExtension(path),
                    ProcessName = Path.GetFileNameWithoutExtension(path),
                    Path = path,
                    Parameters = "",
                    PlayTime = new System.TimeSpan(0)
                };
                GamesInfo.Games.Add(newGame);

                CreateGame(GamesInfo.Games.Last());
            }
        }
        //public void UpdateGame()
        //{
        //    //Juegos.Children.Clear();
        //    //foreach (Game game in GamesInfo.Games)
        //    //{
        //    //    CreateGame(game);
        //    //}
        //}

        private void CreateGame(Game game) { var gamee = new GameView(game) { Margin = new Thickness(5) }; Juegos.Children.Add(gamee); GameMonitor.Register(gamee); }

        private void BTNOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Path.GetFullPath("./"));
        }

        private void BTNFileDownloader_Click(object sender, RoutedEventArgs e)
        {
            new FileDownloaderWindow().Show();
        }
    }
}
