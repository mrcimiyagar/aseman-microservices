using Microsoft.EntityFrameworkCore;
using SharedArea.Notifications;

namespace ApiGateway.DbContexts
{
    public class DatabaseContext : SharedArea.DbContexts.DatabaseContext
    {
        public DbSet<Notification> Notifications { get; set; }
    }
}