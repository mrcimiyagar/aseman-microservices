using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class ContactCreationPush : Push
    {
        public ContactCreationNotification Notif { get; set; }
    }
}