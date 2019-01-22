using Microsoft.EntityFrameworkCore;

namespace SharedArea.Utils
{
    public static class DatabaseConfig
    {
        public static void ConfigDatabase(DbContext context)
        {
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}