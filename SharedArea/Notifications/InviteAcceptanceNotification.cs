using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class InviteAcceptanceNotification : Notification
    {
        public long? InviteId { get; set; }
        public virtual Invite Invite { get; set; }
    }
}