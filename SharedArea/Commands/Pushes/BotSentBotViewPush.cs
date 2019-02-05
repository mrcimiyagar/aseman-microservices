using SharedArea.Notifications;

namespace SharedArea.Commands.Pushes
{
    public class BotSentBotViewPush : Push
    {
        public BotSentBotViewNotification Notif { get; set; }
    }
}