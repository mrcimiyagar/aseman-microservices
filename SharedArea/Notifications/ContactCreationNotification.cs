using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class ContactCreationNotification : Notification
    {
        public long? ContactId { get; set; }
        public virtual Contact Contact { get; set; }
    }
}