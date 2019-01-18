
using System.ComponentModel.DataAnnotations.Schema;
using SharedArea.Entities;
using SharedArea.Notifications;
using Microsoft.EntityFrameworkCore;

namespace BotPlatform.DbContexts
{
    public class DatabaseContext : SharedArea.DbContexts.DatabaseContext
    {
        public DbSet<BaseUser> BaseUsers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserSecret> UserSecrets { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Pending> Pendings { get; set; }
        public DbSet<Complex> Complexes { get; set; }
        public DbSet<ComplexSecret> ComplexSecrets { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Workership> Workerships { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Bot> Bots { get; set; }
        public DbSet<BotSecret> BotSecrets { get; set; }
        public DbSet<BotStoreHeader> BotStoreHeader { get; set; }
        public DbSet<BotStoreSection> BotStoreSections { get; set; }
        public DbSet<BotCreation> BotCreations { get; set; }
        public DbSet<BotSubscription> BotSubscriptions { get; set; }
        public DbSet<FileUsage> FileUsages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=BotPlatformDb;Trusted_Connection=True;");
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Photo>().HasBaseType<File>();
            modelBuilder.Entity<Audio>().HasBaseType<File>();
            modelBuilder.Entity<Video>().HasBaseType<File>();
            
            modelBuilder.Entity<TextMessage>().HasBaseType<Message>();
            modelBuilder.Entity<PhotoMessage>().HasBaseType<Message>();
            modelBuilder.Entity<AudioMessage>().HasBaseType<Message>();
            modelBuilder.Entity<VideoMessage>().HasBaseType<Message>();
            modelBuilder.Entity<ServiceMessage>().HasBaseType<Message>();

            modelBuilder.Entity<ComplexDeletionNotification>().HasBaseType<Notification>();
            modelBuilder.Entity<RoomDeletionNotification>().HasBaseType<Notification>();
            modelBuilder.Entity<InviteCreationNotification>().HasBaseType<Notification>();
            modelBuilder.Entity<InviteCancellationNotification>().HasBaseType<Notification>();
            modelBuilder.Entity<InviteAcceptanceNotification>().HasBaseType<Notification>();
            modelBuilder.Entity<InviteIgnoranceNotification>().HasBaseType<Notification>();
            modelBuilder.Entity<ServiceMessageNotification>().HasBaseType<Notification>();
            modelBuilder.Entity<UserJointComplexNotification>().HasBaseType<Notification>();
            modelBuilder.Entity<TextMessageNotification>().HasBaseType<Notification>();
            modelBuilder.Entity<BotAdditionToRoomNotification>().HasBaseType<Notification>();

            modelBuilder.Entity<User>().HasBaseType<BaseUser>();
            modelBuilder.Entity<Bot>().HasBaseType<BaseUser>();

            modelBuilder.Entity<Contact>()
                .HasOne(c => c.User)
                .WithMany(u => u.Contacts)
                .HasForeignKey(c => c.UserId);
            
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.Peer)
                .WithMany(u => u.Peereds)
                .HasForeignKey(c => c.PeerId);

            modelBuilder.Entity<Workership>()
                .HasIndex(w => new {w.BotId, w.RoomId})
                .IsUnique();

            modelBuilder.Entity<Membership>()
                .HasIndex(m => new {m.ComplexId, m.UserId})
                .IsUnique();

            modelBuilder.Entity<BotCreation>()
                .HasIndex(bc => new {bc.BotId, bc.CreatorId})
                .IsUnique();

            modelBuilder.Entity<BotSubscription>()
                .HasIndex(bs => new {bs.BotId, bs.SubscriberId})
                .IsUnique();

            modelBuilder.Entity<Contact>()
                .HasIndex(c => new {c.UserId, c.PeerId})
                .IsUnique();

            modelBuilder.Entity<Invite>()
                .HasIndex(i => new {i.ComplexId, i.UserId})
                .IsUnique();

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
        }
    }
}