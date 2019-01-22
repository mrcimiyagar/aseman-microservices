using Microsoft.EntityFrameworkCore;
using SharedArea.Entities;

namespace SharedArea.DbContexts
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Session> Sessions { get; set; }
        public DbSet<BaseUser> BaseUsers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserSecret> UserSecrets { get; set; }
        public DbSet<Bot> Bots { get; set; }
        public DbSet<Complex> Complexes { get; set; }
        public DbSet<ComplexSecret> ComplexSecrets { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<User>().HasBaseType<BaseUser>();
            modelBuilder.Entity<Bot>().HasBaseType<BaseUser>();
            
            modelBuilder.Entity<TextMessage>().HasBaseType<Message>();
            modelBuilder.Entity<PhotoMessage>().HasBaseType<Message>();
            modelBuilder.Entity<AudioMessage>().HasBaseType<Message>();
            modelBuilder.Entity<VideoMessage>().HasBaseType<Message>();
            modelBuilder.Entity<ServiceMessage>().HasBaseType<Message>();

            modelBuilder.Entity<Contact>()
                .HasOne(c => c.User)
                .WithMany(u => u.Contacts)
                .HasForeignKey(c => c.UserId);
            
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.Peer)
                .WithMany(u => u.Peereds)
                .HasForeignKey(c => c.PeerId);
            
            modelBuilder.Entity<Membership>()
                .HasIndex(m => new {m.ComplexId, m.UserId})
                .IsUnique();
            
            modelBuilder.Entity<Contact>()
                .HasIndex(c => new {c.UserId, c.PeerId})
                .IsUnique();

            modelBuilder.Entity<Invite>()
                .HasIndex(i => new {i.ComplexId, i.UserId})
                .IsUnique();
        }
    }
}