using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using SharedArea.Notifications;

namespace SharedArea.Utils
{
    public static class DatabaseConfig
    {
        public static void ConfigDatabase(DbContext context)
        {
            context.Database.SetCommandTimeout(60000);
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        public static void ConfigMongoNotifDb(IMongoCollection<Notification> notifColl)
        {
            //notifColl.DeleteMany(notification => true);
        }
    }
}