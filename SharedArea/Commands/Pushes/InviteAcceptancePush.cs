using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class InviteAcceptancePush : Push
    {
        public InviteAcceptanceNotification Notif { get; set; }
    }
}