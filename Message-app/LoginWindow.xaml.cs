using System;
using System.Windows;

namespace MessageApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtUsername.Text))
            {
                var ipDialog = new InputDialog("Enter Server IP", "192.168.1.100");
                if (ipDialog.ShowDialog() == true)
                {
                    var mainWindow = new MainWindow(txtUsername.Text, ipDialog.Answer);
                    mainWindow.Show();
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Please enter a username", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}