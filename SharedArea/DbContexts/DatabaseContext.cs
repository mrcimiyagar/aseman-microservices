using Microsoft.EntityFrameworkCore;
using SharedArea.Entities;

namespace SharedArea.DbContexts
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Session> Sessions { get; set; }
    }
}