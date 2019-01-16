using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class InviteIgnoranceNotification : Notification
    {
        public long? InviteId { get; set; }
        public virtual Invite Invite { get; set; }
    }
}