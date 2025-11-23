using System;
using System.Windows;
using System.Windows.Input;

namespace Games_Launcher
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event Action CloseEvent;
        public MainWindow()
        {
            InitializeComponent();
            borde1.Visibility = Visibility.Visible;
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void b_cerrar_Click(object sender, RoutedEventArgs e)
        {
            InvokeEvent();
            Close();
        }
        public void InvokeEvent() => CloseEvent?.Invoke();

        private void b_minimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
