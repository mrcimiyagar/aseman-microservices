using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class BotAdditionToRoomPush : Push
    {
        public BotAdditionToRoomNotification Notif { get; set; }
    }
}