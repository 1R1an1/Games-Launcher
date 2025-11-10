using FortiCrypts;
using Games_Launcher.Core;
using System.Threading.Tasks;
using System.Windows;

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
            base.OnStartup(e);
            CryptoUtils.iterations = 2500;
            GamesInfo.LoadGamesData();

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(60000);
                    GamesInfo.SaveGamesData();
                }
            });
            window = new MainWindow();
            MainWindow = window;
            MainWindow.Show();
            App.Current.Exit += Current_Exit;

            GameMonitor.StartLoop();

            //reference windoww = new reference();
            ////MainWindow = windoww;
            //windoww.Show();
        }
        private void Current_Exit(object sender, ExitEventArgs e) => GamesInfo.SaveGamesData();
    }
}