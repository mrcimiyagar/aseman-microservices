namespace SharedArea.Notifications
{
    public class BotAnimatedBotViewNotification : Notification
    {
        public long ComplexId { get; set; }
        public long RoomId { get; set; }
        public long BotId { get; set; }
        public string AnimData { get; set; }
    }
}