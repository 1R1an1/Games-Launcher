using Games_Launcher.Core;
using System.Windows;
using System.Windows.Input;

namespace Games_Launcher
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            GameMonitor.ReadyToClose += GameMonitor_ReadyToClose;
            borde1.Visibility = Visibility.Visible;
        }

        private void GameMonitor_ReadyToClose()
        {
            Dispatcher.Invoke(()=> Close());
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void b_cerrar_Click(object sender, RoutedEventArgs e)
        {
            GameMonitor.RequestStop();
        }
        private void b_minimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
