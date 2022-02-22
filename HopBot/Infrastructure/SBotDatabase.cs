using Microsoft.EntityFrameworkCore;
using HopBot.Models;

namespace HopBot.Infrastructure
{
    public class SBotDatabase : DbContext
    {
        public DbSet<BhopMap> Maps => Set<BhopMap>();
        public SBotDatabase() : base() { }
        public SBotDatabase(DbContextOptions options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=SBotDatabase.db");
                optionsBuilder.EnableSensitiveDataLogging(false);
            }
        }
    }
}
