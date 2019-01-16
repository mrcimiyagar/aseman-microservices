using System.ComponentModel.DataAnnotations;
using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class Notification
    {
        [Key]
        public long NotificationId { get; set; }
        public Session Session { get; set; }
    }
}