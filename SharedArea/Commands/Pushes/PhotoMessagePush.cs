using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class PhotoMessagePush : Push
    {
        public PhotoMessageNotification Notif { get; set; }
    }
}