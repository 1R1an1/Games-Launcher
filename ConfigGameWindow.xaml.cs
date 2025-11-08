using Games_Launcher.Model;
using Games_Launcher.Views;
using System.Windows;
using System.Windows.Input;

namespace Games_Launcher
{
    /// <summary>
    /// Lógica de interacción para MainConfigGameWindowWindow.xaml
    /// </summary>
    public partial class ConfigGameWindow : Window
    {
        public ConfigGameWindow(Game game)
        {
            InitializeComponent();
            aaaaaaaa.Children.Add(new ConfigGameView(game));
            borde1.Visibility = Visibility.Visible;
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void b_cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void b_minimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
