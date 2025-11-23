using Games_Launcher.Core;
using Games_Launcher.Model;
using System;
using System.Collections.Specialized;
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
            GamesInfo.Games.CollectionChanged += Games_CollectionChanged;
        }

        private void Games_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move && e.OldItems.Count == 1)
            {
                Game game = (Game)e.OldItems[0];

                // Buscar el GameView correspondiente en el panel usando DataContext
                GameView element = Juegos.Children
                                         .OfType<GameView>()
                                         .FirstOrDefault(x => x.DataContext == game);

                if (element != null)
                {
                    // Remover de la posición antigua y agregar en la nueva
                    Juegos.Children.Remove(element);
                    Juegos.Children.Insert(e.NewStartingIndex, element);
                }
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
                    PlayTime = new TimeSpan(0)
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

        private void CreateGame(Game game) { var gamee = new GameView(game) { Margin = new Thickness(5), DataContext = game }; Juegos.Children.Add(gamee); GameMonitor.Register(gamee); }

        private void BTNOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Path.GetFullPath("./"));
        }
    }
}
