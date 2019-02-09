
using Microsoft.EntityFrameworkCore;

namespace ServiceManager.DbContexts
{
    public class DatabaseContext : DbContext
    {
        private readonly string _dbName;
        
        public DatabaseContext(string dbName)
        {
            this._dbName = dbName;
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=" + _dbName + ";Trusted_Connection=True;");
    }
}