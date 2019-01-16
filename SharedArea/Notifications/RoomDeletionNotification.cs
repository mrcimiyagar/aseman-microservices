

namespace SharedArea.Notifications
{
    public class RoomDeletionNotification : Notification
    {
        public long ComplexId { get; set; }
        public long RoomId { get; set; }
    }
}