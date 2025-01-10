using GameServer.Domain.Interfaces;
using GameServer.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<ResourceTransaction> Transactions { get; set; }
        public DbSet<PlayerAuth> PlayerAuth { get; set; }
        public DbSet<Device> Devices { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }
    }
}
