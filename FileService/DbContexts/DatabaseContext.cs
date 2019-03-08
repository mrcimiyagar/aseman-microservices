
using SharedArea.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileService.DbContexts
{
    public class DatabaseContext : SharedArea.DbContexts.DatabaseContext
    {
        public DbSet<File> Files { get; set; }
        public DbSet<FileUsage> FileUsages { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=FileServiceDb;Trusted_Connection=True;");
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Photo>().HasBaseType<File>();
            modelBuilder.Entity<Audio>().HasBaseType<File>();
            modelBuilder.Entity<Video>().HasBaseType<File>();

            modelBuilder.Entity<FileUsage>()
                .HasIndex(fu => new {fu.FileId, fu.RoomId})
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

            modelBuilder.Entity<Message>()
                .Property(m => m.MessageId)
                .ValueGeneratedNever();
            
            modelBuilder.Entity<MemberAccess>()
                .Property(m => m.MemberAccessId)
                .ValueGeneratedNever();
        }
    }
}