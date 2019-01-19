using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class RoomDeletionPush : Push
    {
        public RoomDeletionNotification Notif { get; set; }
    }
}