using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class UserJointComplexNotification : Notification
    {
        public long UserId { get; set; }
        public long ComplexId { get; set; }
    }
}