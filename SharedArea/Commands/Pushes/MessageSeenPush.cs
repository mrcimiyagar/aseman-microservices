using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class MessageSeenPush : Push
    {
        public MessageSeenNotification Notif { get; set; }
    }
}