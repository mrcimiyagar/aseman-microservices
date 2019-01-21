using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class BotRemovationFromRoomPush : Push
    {
        public BotRemovationFromRoomNotification Notif { get; set; }
    }
}