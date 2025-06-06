using System.Windows;
using System.Windows.Input;
using MessageApp.mwm.viewModel;

namespace MessageApp
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow(string username, string serverIp)
        {
            InitializeComponent();
            ViewModel = new MainViewModel(username, serverIp);
            DataContext = ViewModel;
            Title = $"Chat App - {username}";
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Restore_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}