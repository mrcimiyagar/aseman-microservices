using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class TextMessageNotification : Notification
    {
        public TextMessage Message { get; set; }
    }
}