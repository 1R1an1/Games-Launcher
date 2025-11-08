using Games_Launcher.Core;
using Games_Launcher.Model;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Games_Launcher.Views
{
    /// <summary>
    /// Lógica de interacción para ConfigGameView.xaml
    /// </summary>
    public partial class ConfigGameView : UserControl
    {
        private Game _thisGame;
        private GameView _thisGameView;
        public ConfigGameView(Game game, GameView gameView)
        {
            _thisGame = game;
            _thisGameView = gameView;
            InitializeComponent();
            Init();
        }

        private void SelectGamePathBTN_Click(object sender, RoutedEventArgs e)
        {
            if (GameFunctions.SelectGamePath(out string path) == true)
                GamePathTBX.Text = path;
        }

        private void AplicarBTN_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(GamePathTBX.Text) || !GamePathTBX.Text.EndsWith(".exe"))
            {
                MessageBox.Show("La ruta del juego no es válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _thisGame.Parameters = GameParametersTBX.Text;
            _thisGame.Name = GameNameTBX.Text;
            _thisGame.Path = GamePathTBX.Text;
            _thisGame.ProcessName = Path.GetFileNameWithoutExtension(GamePathTBX.Text);
            //App.window.CDU_Window..UpdateGames();
            _thisGameView.UpdateInfo();
            Window.GetWindow(this).Close();
        }

        private void CancelarBTN_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void GamePathTBX_TextChanged(object sender, TextChangedEventArgs e)
        {
            GameIconIMG.Source = GameFunctions.GetGameIcon(GamePathTBX.Text);
        }

        private void Init()
        {
            GamePathTBX.Text = _thisGame.Path;
            GameNameTBX.Text = _thisGame.Name;
            GameParametersTBX.Text = _thisGame.Parameters;
        }

        private void OpenGamePathBTN_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_thisGame.Path)))
            {
                MessageBox.Show("La carpeta del juego no fue encontrada", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Process.Start("explorer.exe", Path.GetDirectoryName(_thisGame.Path));
        }
    }
}
