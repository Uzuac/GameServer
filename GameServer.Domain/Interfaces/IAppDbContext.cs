using GameServer.Application.Services;
using GameServer.Domain.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Player> Players { get; set; }
        DbSet<Resource> Resources { get; set; }
        DbSet<ResourceTransaction> Transactions { get; set; }
        DbSet<PlayerAuth> PlayerAuth { get; set; }
        DbSet<Device> Devices { get; set; }
        Task<int> SaveChangesAsync();
    }
}
