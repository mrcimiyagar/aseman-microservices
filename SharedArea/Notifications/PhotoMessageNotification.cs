using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class PhotoMessageNotification : Notification
    {
        public PhotoMessage Message { get; set; }
    }
}