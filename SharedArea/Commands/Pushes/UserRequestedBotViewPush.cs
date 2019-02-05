using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class UserRequestedBotViewPush : Push
    {
        public UserRequestedBotViewNotification Notif { get; set; }
    }
}