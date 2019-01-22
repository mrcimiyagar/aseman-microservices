using Microsoft.EntityFrameworkCore;
using SharedArea.Entities;
using SharedArea.Notifications;

namespace ApiGateway.DbContexts
{
    public class DatabaseContext : SharedArea.DbContexts.DatabaseContext
    {
        public DbSet<Notification> Notifications { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=ApiGatewayDb;Trusted_Connection=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
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

            modelBuilder.Entity<Session>()
                .Property(s => s.SessionId)
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
            
            modelBuilder.Entity<Workership>()
                .Property(u => u.WorkershipId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<Contact>()
                .Property(b => b.ContactId)
                .ValueGeneratedNever();
        }
    }
}