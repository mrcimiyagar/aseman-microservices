
using SharedArea.Entities;
using SharedArea.Notifications;
using Microsoft.EntityFrameworkCore;

namespace DesktopPlatform.DbContexts
{
    public class DatabaseContext : SharedArea.DbContexts.DatabaseContext
    {
        public DbSet<Workership> Workerships { get; set; }
        public DbSet<BotSecret> BotSecrets { get; set; }
        public DbSet<BotCreation> BotCreations { get; set; }
        public DbSet<BotSubscription> BotSubscriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=DesktopPlatformDb;Trusted_Connection=True;");
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Workership>()
                .HasIndex(w => new {w.BotId, w.RoomId})
                .IsUnique();
            
            modelBuilder.Entity<BotCreation>()
                .HasIndex(bc => new {bc.BotId, bc.CreatorId})
                .IsUnique();

            modelBuilder.Entity<BotSubscription>()
                .HasIndex(bs => new {bs.BotId, bs.SubscriberId})
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.BaseUserId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<Complex>()
                .Property(u => u.ComplexId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<Room>()
                .Property(u => u.RoomId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<Membership>()
                .Property(u => u.MembershipId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<UserSecret>()
                .Property(u => u.UserSecretId)
                .ValueGeneratedNever();

            modelBuilder.Entity<ComplexSecret>()
                .Property(u => u.ComplexSecretId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<Session>()
                .Property(u => u.SessionId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<Bot>()
                .Property(u => u.BaseUserId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<BotSecret>()
                .Property(u => u.BotSecretId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<BotCreation>()
                .Property(u => u.BotCreationId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<BotSubscription>()
                .Property(u => u.BotSubscriptionId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<Contact>()
                .Property(b => b.ContactId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Message>()
                .Property(m => m.MessageId)
                .ValueGeneratedNever();
        }
    }
}