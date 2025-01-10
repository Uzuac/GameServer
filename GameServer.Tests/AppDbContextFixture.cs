using GameServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;

namespace GameServer.Tests
{
    public class AppDbContextFixture
    {
        public AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique database for each test
                .Options;

            return new AppDbContext(options);
        }
    }
}
