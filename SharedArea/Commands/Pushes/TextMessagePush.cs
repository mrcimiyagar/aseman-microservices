using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class TextMessagePush : Push
    {
        public TextMessageNotification Notif { get; set; }
    }
}