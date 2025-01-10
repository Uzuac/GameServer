using GameServer.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain.Commands
{
    public class LoginCommand : IRequest<string>
    {
        public Guid DeviceId { get; set; }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
    {
        private readonly IAppDbContext _dbContext;
        public LoginCommandHandler(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var player = _dbContext.Players.AsNoTracking().Where(p => p.Devices.Any(d => d.Id == request.DeviceId)).FirstOrDefault();

            if (player == null)
            {
                throw new Exception("Player not found");
            }

            var auth = _dbContext.PlayerAuth.AsNoTracking().Where(pa => pa.DeviceId == request.DeviceId).FirstOrDefault();
            if (auth == null)
            {
                auth = new Model.PlayerAuth()
                {
                    Id = Guid.NewGuid(),
                    DeviceId = request.DeviceId,
                    AuthExpiration = DateTime.UtcNow.AddMinutes(30)
                };

                _dbContext.PlayerAuth.Add(auth);
            }
            else
            {
                return "Already logged in";
            }

            await _dbContext.SaveChangesAsync();
            return player.Id.ToString();
        }
    }
}
