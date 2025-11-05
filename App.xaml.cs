using System.Windows;
using FortiCrypts;

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

            window = new MainWindow();
            MainWindow = window;
            MainWindow.Show();


            //reference windoww = new reference();
            ////MainWindow = windoww;
            //windoww.Show();
        }
    }
}
