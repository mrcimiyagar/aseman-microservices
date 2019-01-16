using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class BotAdditionToRoomNotification : Notification
    {
        public long? RoomId { get; set; }
        public virtual Room Room { get; set; }
    }
}