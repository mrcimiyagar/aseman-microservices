namespace SharedArea.Notifications
{
    public class BotSentBotViewNotification : Notification
    {
        public long ComplexId { get; set; }
        public long RoomId { get; set; }
        public long BotId { get; set; }
        public string ViewData { get; set; }
    }
}