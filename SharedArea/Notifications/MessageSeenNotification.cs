using SharedArea.Commands.Message;
using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class MessageSeenNotification : Notification
    {
        public long MessageId { get; set; }
        public long MessageSeenCount { get; set; }
    }
}