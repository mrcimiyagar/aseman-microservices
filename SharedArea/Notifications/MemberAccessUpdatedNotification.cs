using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class MemberAccessUpdatedNotification : Notification
    {
        public MemberAccess MemberAccess { get; set; }
    }
}