using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class BotAdditionToRoomNotification : Notification
    {
        public Workership Workership { get; set; }
    }
}