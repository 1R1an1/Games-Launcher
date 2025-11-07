using Games_Launcher.Core;
using Games_Launcher.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
                    _gameProcess = Process.GetProcessesByName(_thisGame.Name);
                    bool currentlyRunning = _gameProcess.Length > 0;

                    if (currentlyRunning && !_isRunning)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            BTNJugar.Margin = new Thickness(0, 0, 12, 0);
                            BTNJugar.Content = "DETENER";
                            BTNJugar.Tag = App.Current.FindResource("DownloadColorNormal");
                            BTNJugar.BorderBrush = (Brush)App.Current.FindResource("DownloadColorMouseOver");
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
                            BTNJugar.Tag = App.Current.FindResource("JugarColorNormal");
                            BTNJugar.BorderBrush = (Brush)App.Current.FindResource("JugarColorMouseOver");
                        });

                        _isRunning = false;
                        TimeSpan duration = DateTime.Now - starterTime;
                        _thisGame.PlayTime += duration;

                        Dispatcher.Invoke(() => LBLTimeOppend.Content = ConvertTime(_thisGame.PlayTime));
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
                Process.Start(_thisGame.Path);
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


        private BitmapImage GetGameIcon()
        {
            if (string.IsNullOrEmpty(_thisGame.Path) || !File.Exists(_thisGame.Path))
                return null;

            System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(_thisGame.Path);

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
        private string ConvertTime(TimeSpan time)
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
        private void Init()
        {
            GameIconIMG.Source = GetGameIcon();
            GameTitleTB.Text = _thisGame.Name;
            LBLTimeOppend.Content = ConvertTime(_thisGame.PlayTime);
        }
    }
}
