using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class InviteIgnoredPush : Push
    {
        public InviteIgnoranceNotification Notif { get; set; }
    }
}