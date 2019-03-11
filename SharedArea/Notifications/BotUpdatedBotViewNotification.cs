namespace SharedArea.Notifications
{
    public class BotUpdatedBotViewNotification : Notification
    {
        public long ComplexId { get; set; }
        public long RoomId { get; set; }
        public long BotId { get; set; }
        public string UpdateData { get; set; }
        public bool BatchData { get; set; }
    }
}