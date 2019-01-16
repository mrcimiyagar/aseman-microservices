using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class ServiceMessageNotification : Notification
    {
        public ServiceMessage Message { get; set; }
    }
}