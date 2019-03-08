
using SharedArea.Entities;
using Microsoft.EntityFrameworkCore;

namespace EntryPlatform.DbContexts
{
    public class DatabaseContext : SharedArea.DbContexts.DatabaseContext
    {
        public DbSet<Pending> Pendings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=EntryPlatformDb;Trusted_Connection=True;");
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            modelBuilder.Entity<Message>()
                .Property(m => m.MessageId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<MemberAccess>()
                .Property(m => m.MemberAccessId)
                .ValueGeneratedNever();
        }
    }
}