using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class UserJointComplexNotification : Notification
    {
        public long? MembershipId { get; set; }
        public Membership Membership { get; set; }
    }
}