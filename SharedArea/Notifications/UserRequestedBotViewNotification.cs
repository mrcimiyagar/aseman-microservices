using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class UserRequestedBotViewNotification : Notification
    {
        public long ComplexId { get; set; }
        public long RoomId { get; set; }
        public long BotId { get; set; }
        public User User { get; set; }
    }
}