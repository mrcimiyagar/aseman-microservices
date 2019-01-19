using Microsoft.EntityFrameworkCore;
using SharedArea.Entities;

namespace SharedArea.DbContexts
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Session> Sessions { get; set; }
        public DbSet<BaseUser> BaseUsers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Bot> Bots { get; set; }
        public DbSet<Complex> Complexes { get; set; }
        public DbSet<ComplexSecret> ComplexSecrets { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Membership> Memberships { get; set; }
    }
}