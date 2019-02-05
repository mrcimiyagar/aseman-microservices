namespace SharedArea.Notifications
{
    public class BotRanCommandsOnBotViewNotification : Notification
    {
        public long ComplexId { get; set; }
        public long RoomId { get; set; }
        public long BotId { get; set; }
        public string CommandsData { get; set; }
    }
}