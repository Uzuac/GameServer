using GameServer.Domain.Interfaces;
using GameServer.Domain.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Domain.Commands
{
    public class UpdateResourcesCommand : AuthRequest, IRequest<string>
    {
        public ResourceType ResourceType { get; set; }
        public int ResourceValue { get; set; }
    }

    public class UpdateResourcesCommandHandler : IRequestHandler<UpdateResourcesCommand, string>
    {
        private readonly IAppDbContext _dbContext;
        public UpdateResourcesCommandHandler(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<string> Handle(UpdateResourcesCommand request, CancellationToken cancellationToken)
        {
            var player = _dbContext.Players.AsNoTracking()
                .Include(p => p.Resources)
                .Where(p => p.Devices.Any(d => d.Id == request.DeviceId))
                .FirstOrDefault();

            if (player == null)
            {
                throw new Exception("Player not found");
            }
            var resource = _dbContext.Resources
                .FirstOrDefault(r => r.Type == request.ResourceType && r.PlayerId == player.Id);

            if (resource == null)
            {
                resource = new Resource
                {
                    Id = Guid.NewGuid(),
                    PlayerId = player.Id,
                    Type = request.ResourceType,
                };

                _dbContext.Resources.Add(resource);
            }

            var response = resource.UpdateWithAmount(request.ResourceValue);

            await _dbContext.SaveChangesAsync();

            return $"{response} {request.ResourceType.ToString()} available";
        }
    }
}
