using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class InviteCreationPush : Push
    {
        public InviteCreationNotification Notif { get; set; }
    }
}