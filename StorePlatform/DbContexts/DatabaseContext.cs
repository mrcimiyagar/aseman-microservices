
using SharedArea.Entities;
using Microsoft.EntityFrameworkCore;

namespace StorePlatform.DbContexts
{
    public class DatabaseContext : SharedArea.DbContexts.DatabaseContext
    {
        public DbSet<BotSecret> BotSecrets { get; set; }
        public DbSet<BotStoreHeader> BotStoreHeader { get; set; }
        public DbSet<BotStoreSection> BotStoreSections { get; set; }
        public DbSet<BotCreation> BotCreations { get; set; }
        public DbSet<BotSubscription> BotSubscriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=StorePlatformDb;Trusted_Connection=True;");
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<BotCreation>()
                .HasIndex(bc => new {bc.BotId, bc.CreatorId})
                .IsUnique();

            modelBuilder.Entity<BotSubscription>()
                .HasIndex(bs => new {bs.BotId, bs.SubscriberId})
                .IsUnique();

            modelBuilder.Entity<Invite>()
                .Property(i => i.InviteId)
                .ValueGeneratedNever();

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
                .Property(b => b.BaseUserId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<Contact>()
                .Property(b => b.ContactId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<BotCreation>()
                .Property(u => u.BotCreationId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<BotSubscription>()
                .Property(u => u.BotSubscriptionId)
                .ValueGeneratedNever();

            modelBuilder.Entity<BotSecret>()
                .Property(bs => bs.BotSecretId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Message>()
                .Property(m => m.MessageId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<MemberAccess>()
                .Property(m => m.MemberAccessId)
                .ValueGeneratedNever();
        }
    }
}