
using SharedArea.Entities;
using SharedArea.Notifications;
using Microsoft.EntityFrameworkCore;

namespace MessengerPlatform.DbContexts
{
    public class DatabaseContext : SharedArea.DbContexts.DatabaseContext
    {
        public DbSet<File> Files { get; set; }
        public DbSet<FileUsage> FileUsages { get; set; }
        public DbSet<Workership> Workerships { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=MessengerPlatformDb;Trusted_Connection=True;");
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Photo>().HasBaseType<File>();
            modelBuilder.Entity<Audio>().HasBaseType<File>();
            modelBuilder.Entity<Video>().HasBaseType<File>();
            
            modelBuilder.Entity<TextMessage>().HasBaseType<Message>();
            modelBuilder.Entity<PhotoMessage>().HasBaseType<Message>();
            modelBuilder.Entity<AudioMessage>().HasBaseType<Message>();
            modelBuilder.Entity<VideoMessage>().HasBaseType<Message>();
            modelBuilder.Entity<ServiceMessage>().HasBaseType<Message>();

            modelBuilder.Entity<FileUsage>()
                .HasIndex(fu => new {fu.FileId, fu.RoomId})
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
                .Property(b => b.BaseUserId)
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

            modelBuilder.Entity<Photo>()
                .Property(b => b.FileId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Audio>()
                .Property(b => b.FileId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Video>()
                .Property(b => b.FileId)
                .ValueGeneratedNever();

            modelBuilder.Entity<FileUsage>()
                .Property(fu => fu.FileUsageId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<Contact>()
                .Property(b => b.ContactId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Workership>()
                .Property(w => w.WorkershipId)
                .ValueGeneratedNever();
        }
    }
}