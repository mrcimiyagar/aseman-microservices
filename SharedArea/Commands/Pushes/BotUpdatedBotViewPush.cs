using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class BotUpdatedBotViewPush : Push
    {
        public BotUpdatedBotViewNotification Notif { get; set; }
    }
}