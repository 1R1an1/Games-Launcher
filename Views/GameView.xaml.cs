using Games_Launcher.Core;
using Games_Launcher.Model;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
        private Game _thisGame;
        private Process[] _gameProcess;
        private bool _isRunning = false;

        public GameView(Game Game)
        {
            _thisGame = Game;
            InitializeComponent();
            Init();

            _ = Task.Run(async () =>
            {
                DateTime starterTime = DateTime.Now;

                while (true)
                {
                    _gameProcess = Process.GetProcessesByName(_thisGame.ProcessName);
                    bool currentlyRunning = _gameProcess.Length > 0;

                    if (currentlyRunning && !_isRunning)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            BTNJugar.Margin = new Thickness(0, 0, 12, 0);
                            BTNJugar.Content = "DETENER";
                            BTNJugar.Tag = FindResource("DownloadColorNormal");
                            BTNJugar.BorderBrush = (Brush)FindResource("DownloadColorMouseOver");
                        });

                        _isRunning = true;
                        starterTime = DateTime.Now;
                    }
                    else if (!currentlyRunning && _isRunning)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            BTNJugar.Margin = new Thickness(0, 0, 23, 0);
                            BTNJugar.Content = "JUGAR";
                            BTNJugar.Tag = FindResource("JugarColorNormal");
                            BTNJugar.BorderBrush = (Brush)FindResource("JugarColorMouseOver");
                        });

                        _isRunning = false;
                        TimeSpan duration = DateTime.Now - starterTime;
                        _thisGame.PlayTime += duration;

                        Dispatcher.Invoke(() => LBLTimeOppend.Content = GameFunctions.ConvertTime(_thisGame.PlayTime));
                    }

                    await Task.Delay(3000);
                }
            });
        }

        public GameView()
        {
            InitializeComponent();
        }

        private void BTNJugar_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRunning)
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = _thisGame.Path,
                        Arguments = _thisGame.Parameters,
                        UseShellExecute = false
                    });
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
            if (MessageBox.Show($"¿Estás seguro de que deseas eliminar {_thisGame.Name}?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            GamesInfo.Games.Remove(_thisGame);
            if (Parent is Panel panel)
            {
                panel.Children.Remove(this);
            }
            else if (Parent is ContentControl content)
            {
                content.Content = null;
            }
        }


        private void Init()
        {
            GameIconIMG.Source = GameFunctions.GetGameIcon(_thisGame.Path);
            GameTitleTB.Text = _thisGame.Name;
            LBLTimeOppend.Content = GameFunctions.ConvertTime(_thisGame.PlayTime);
        }

        private void GameIconIMG_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfigGameWindow window = new ConfigGameWindow(_thisGame);
            window.ShowDialog();
        }
    }
}
