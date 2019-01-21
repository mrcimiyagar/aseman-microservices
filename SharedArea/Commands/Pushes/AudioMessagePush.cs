using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class AudioMessagePush : Push
    {
        public AudioMessageNotification Notif { get; set; }
    }
}