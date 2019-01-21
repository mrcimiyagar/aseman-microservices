using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class VideoMessagePush : Push
    {
        public VideoMessageNotification Notif { get; set; }
    }
}