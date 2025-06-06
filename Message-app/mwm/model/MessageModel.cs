using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MessageApp.mwm.model
{
    public class MessageModel : INotifyPropertyChanged
    {
        private string _username = string.Empty;
        private string _messageText = string.Empty;
        private DateTime _time;
        private string _usernameColor = "#409aff";

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string MessageText
        {
            get => _messageText;
            set { _messageText = value; OnPropertyChanged(); }
        }

        public DateTime Time
        {
            get => _time;
            set { _time = value; OnPropertyChanged(); }
        }

        public string UsernameColor
        {
            get => _usernameColor;
            set { _usernameColor = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}