
using SharedArea.Entities;
using SharedArea.Notifications;
using Microsoft.EntityFrameworkCore;

namespace CityPlatform.DbContexts
{
    public class DatabaseContext : SharedArea.DbContexts.DatabaseContext
    {
        public DbSet<BotSecret> BotSecrets { get; set; }
        public DbSet<BotCreation> BotCreations { get; set; }
        public DbSet<BotSubscription> BotSubscriptions { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=CityPlatformDb;Trusted_Connection=True;");
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<BotCreation>()
                .HasIndex(bc => new {bc.BotId, bc.CreatorId})
                .IsUnique();

            modelBuilder.Entity<BotSubscription>()
                .HasIndex(bs => new {bs.BotId, bs.SubscriberId})
                .IsUnique();

            modelBuilder.Entity<Message>()
                .Property(m => m.MessageId)
                .ValueGeneratedNever();
        }
    }
}