namespace SharedArea.Notifications
{
    public class MessageSeenNotification : Notification
    {
        public long MessageId { get; set; }
        public long MessageSeenCount { get; set; }
    }
}