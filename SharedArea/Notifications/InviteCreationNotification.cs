
using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class InviteCreationNotification : Notification
    {
        public long? InviteId { get; set; }
        public virtual Invite Invite { get; set; }
    }
}