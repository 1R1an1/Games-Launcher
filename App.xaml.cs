using System.Windows;
using FortiCrypts;
using Games_Launcher.Core;

namespace Games_Launcher
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow window;
        protected override void OnStartup(StartupEventArgs e)
        {
            CryptoUtils.iterations = 2500;
            GamesInfo.LoadGamesData();

            window = new MainWindow();
            MainWindow = window;
            MainWindow.Show();
            App.Current.Exit += Current_Exit;


            //reference windoww = new reference();
            ////MainWindow = windoww;
            //windoww.Show();
        }
        private void Current_Exit(object sender, ExitEventArgs e) => GamesInfo.SaveGamesData();
    }
}