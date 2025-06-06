using MessageApp.Net;
using MessageApp.mwm.model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using MessageApp.core;
using System.Runtime.CompilerServices;
using System.Linq;

namespace MessageApp.mwm.viewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Server _server;
        private string _message = string.Empty;
        private ContactModel? _selectedContact;
        private UserModel? _selectedUser;
        private string _username;
        private string _serverIp;

        public ObservableCollection<ContactModel> Contacts { get; } = new();
        public ObservableCollection<UserModel> OnlineUsers { get; } = new();

        public ContactModel? SelectedContact
        {
            get => _selectedContact;
            set
            {
                if (_selectedContact != value)
                {
                    _selectedContact = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Messages));
                }
            }
        }

        public UserModel? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<MessageModel> Messages =>
            SelectedContact?.Messages ?? new ObservableCollection<MessageModel>();

        public RelayCommand SendCommand { get; }
        public RelayCommand SendToUserCommand { get; }
        public RelayCommand ReconnectCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel(string username, string serverIp)
        {
            _username = username;
            _serverIp = serverIp;
            _server = new Server();
            InitializeContacts();
            ConnectToServer();

            SendCommand = new RelayCommand(_ =>
            {
                if (!string.IsNullOrWhiteSpace(Message) && SelectedContact != null)
                {
                    _server.SendMessageToServer(Message);
                    SelectedContact.Messages.Add(new MessageModel
                    {
                        Username = "You",
                        MessageText = Message,
                        UsernameColor = "LightGreen",
                        Time = DateTime.Now
                    });
                    Message = string.Empty;
                    OnPropertyChanged(nameof(Messages));
                }
            }, _ => !string.IsNullOrWhiteSpace(Message));

            SendToUserCommand = new RelayCommand(_ =>
            {
                if (SelectedUser != null && !string.IsNullOrWhiteSpace(Message))
                {
                    _server.SendPrivateMessage(SelectedUser.UID, Message);
                    SelectedContact?.Messages.Add(new MessageModel
                    {
                        Username = $"You to {SelectedUser.Username}",
                        MessageText = Message,
                        UsernameColor = "LightGreen",
                        Time = DateTime.Now
                    });
                    Message = string.Empty;
                    OnPropertyChanged(nameof(Messages));
                }
            }, _ => SelectedUser != null && !string.IsNullOrWhiteSpace(Message));

            ReconnectCommand = new RelayCommand(_ => Reconnect());
        }

        private void ConnectToServer()
        {
            _server.ConnectToServer(_username, _serverIp);

            _server.UserConnectedEvent += user =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (user != null && !OnlineUsers.Any(u => u.UID == user.UID))
                    {
                        OnlineUsers.Add(user);
                    }
                });
            };

            _server.UserDisconnectedEvent += userId =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var userToRemove = OnlineUsers.FirstOrDefault(u => u.UID == userId);
                    if (userToRemove != null)
                    {
                        OnlineUsers.Remove(userToRemove);
                    }
                });
            };

            _server.MessageReceivedEvent += (senderId, message, timestamp) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var contact = Contacts.FirstOrDefault();
                    if (contact != null && message != null)
                    {
                        var senderName = senderId == _server.UID
                            ? "You"
                            : OnlineUsers.FirstOrDefault(u => u.UID == senderId)?.Username ?? "Unknown";

                        contact.Messages.Add(new MessageModel
                        {
                            Username = senderName,
                            MessageText = message,
                            UsernameColor = senderId == _server.UID ? "LightGreen" : "#409aff",
                            Time = timestamp != null ? DateTime.Parse(timestamp) : DateTime.Now
                        });
                        OnPropertyChanged(nameof(Messages));
                    }
                });
            };
        }

        public void Reconnect()
        {
            if (!_server.IsConnected)
            {
                OnlineUsers.Clear();
                _server.ConnectToServer(_username, _serverIp);
            }
        }

        private void InitializeContacts()
        {
            Contacts.Add(new ContactModel
            {
                Username = "General Chat",
                ImageSource = "/Icons/general_chat.png",
                Messages = new ObservableCollection<MessageModel>()
            });
            SelectedContact = Contacts[0];
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}