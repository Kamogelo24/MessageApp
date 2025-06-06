using System.Windows;

namespace MessageApp
{
    public partial class InputDialog : Window
    {
        public string Answer { get; set; }

        public InputDialog(string title, string defaultAnswer = "")
        {
            InitializeComponent();
            this.Title = title; // Using base Window.Title property
            Answer = defaultAnswer;
            DataContext = this;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}