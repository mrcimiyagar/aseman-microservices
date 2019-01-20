using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class ServiceMessagePush : Push
    {
        public ServiceMessageNotification Notif { get; set; }
    }
}