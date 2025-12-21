using Games_Launcher.Core;
using Games_Launcher.Model;
using Games_Launcher.Windows;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Games_Launcher.Views
{
    /// <summary>
    /// Lógica de interacción para GameView.xaml
    /// </summary>
    public partial class GameView : UserControl
    {
        public Game thisGame;
        public Process[] _gameProcess;
        public bool IsRunning = false;
        public DateTime starterTime = DateTime.Now;

        public GameView(Game Game)
        {
            thisGame = Game;
            InitializeComponent();
            UpdateInfo();
        }

        public GameView()
        {
            InitializeComponent();
        }

        private void BTNJugar_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRunning)
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = thisGame.Path,
                        Arguments = thisGame.Parameters,
                        UseShellExecute = false,
                        WorkingDirectory = Path.GetDirectoryName(thisGame.Path)
                    });
                    BTNJugar.IsEnabled = false;
                    BTNJugar.Tag = (Brush)FindResource("NormalColorNormal2");
                    BTNJugar.Foreground = (Brush)FindResource("FontColorDisabled2");
                } catch
                {
                    MessageBox.Show("No se pudo iniciar el juego. Verifica que la ruta y los parámetros sean correctos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                foreach (var process in _gameProcess)
                    process.Kill();
                
            
        }
        private void BTNEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"¿Estás seguro de que deseas eliminar {thisGame.Name}?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            GamesInfo.Games.Remove(thisGame);
            GameMonitor.Unregister(this);
            if (Parent is Panel panel)
            {
                panel.Children.Remove(this);
            }
            else if (Parent is ContentControl content)
            {
                content.Content = null;
            }
        }


        public void UpdateInfo()
        {
            GameIconIMG.Source = GameFunctions.GetGameIcon(thisGame.Path);
            GameTitleTB.Text = thisGame.Name;
            LBLTimeOppend.Content = GameFunctions.ConvertTime(thisGame.PlayTime);
            LBLLastOppend.Content = GameFunctions.UltimaVezJugado(thisGame.LastPlayed);
        }

        private void GameIconIMG_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfigGameWindow window = new ConfigGameWindow(thisGame, this);
            window.ShowDialog();
        }

        private void BTNUp_Click(object sender, RoutedEventArgs e)
        {
            GamesInfo.Games.Mover(GamesInfo.Games.IndexOf(thisGame), -1);
        }

        private void BTNDown_Click(object sender, RoutedEventArgs e)
        {
            GamesInfo.Games.Mover(GamesInfo.Games.IndexOf(thisGame), +1);
        }

        
    }

    public static class ObservableCollectionExtensions
    {
        public static void Mover<T>(this ObservableCollection<T> list, int index, int direccion)
        {
            // direccion: -1 = subir, +1 = bajar
            if (list == null || index < 0 || index >= list.Count)
                return; // índice fuera de rango

            int nuevoIndice = index + direccion;

            if (nuevoIndice < 0 || nuevoIndice >= list.Count)
                return; // no hacemos nada si se sale del rango

            // Intercambiar elementos
            list.Move(index, nuevoIndice);
        }
    }
}
