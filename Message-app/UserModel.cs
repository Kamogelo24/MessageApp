using System;

namespace MessageApp.mwm.model
{
    public class UserModel
    {
        public string Username { get; set; } = string.Empty;
        public Guid UID { get; set; } = Guid.NewGuid();
    }
}