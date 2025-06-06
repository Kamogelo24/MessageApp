using System.Collections.ObjectModel;

namespace MessageApp.mwm.model
{
    public class ContactModel
    {
        public string Username { get; set; } = string.Empty;
        public string ImageSource { get; set; } = string.Empty;
        public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();
    }
}