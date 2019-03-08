using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class MemberAccessUpdatedPush : Push
    {
        public MemberAccessUpdatedNotification Notif { get; set; }
    }
}