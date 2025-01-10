using GameServer.Domain.Interfaces;
using GameServer.Domain.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Domain.Commands
{
    public class SendResourcesCommand : AuthRequest, IRequest<string>
    {
        public Guid FriendPlayerId { get; set; }
        public ResourceType ResourceType { get; set; }
        public int ResourceValue { get; set; }
    }

    public class SendResourcesCommandHandler : IRequestHandler<SendResourcesCommand, string>
    {
        private readonly IAppDbContext _dbContext;
        private readonly INotificationService _notificationService;

        public SendResourcesCommandHandler(IAppDbContext dbContext, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }
        public async Task<string> Handle(SendResourcesCommand request, CancellationToken cancellationToken)
        {
            var player = _dbContext.Players.AsNoTracking()
                .Include(p => p.Resources)
                .Where(p => p.Devices.Any(d => d.Id == request.DeviceId)).FirstOrDefault();

            var friendPlayer = _dbContext.Players.AsNoTracking()
                .Include(p => p.Resources)
                .Include(p => p.Devices)
                .Where(p => p.Id == request.FriendPlayerId).FirstOrDefault();

            if (player == null || friendPlayer == null)
            {
                throw new Exception("Player not found");
            }

            var friendResource = _dbContext.Resources.Where(r => r.Type == request.ResourceType && r.PlayerId == friendPlayer.Id).FirstOrDefault();

            if (friendResource == null)
            {
                friendResource = new Resource()
                {
                    Id = Guid.NewGuid(),
                    PlayerId = friendPlayer.Id,
                    Type = request.ResourceType
                };
                _dbContext.Resources.Add(friendResource);
            }

            var friendCurrentAmount = friendResource.UpdateWithAmount(request.ResourceValue);

            var resource = _dbContext.Resources.Where(r => r.Type == request.ResourceType && r.PlayerId == player.Id).FirstOrDefault();
            if (resource == null)
            {
                return "Insufficient funds";
            }

            var response = resource.UpdateWithAmount(-request.ResourceValue);

            await _dbContext.SaveChangesAsync();
            await _notificationService.SendNotification(friendPlayer.Devices?.Select(d => d.Id).ToList(), $"{request.ResourceType.ToString()} {request.ResourceType} have been added to your account. Current balance is {friendCurrentAmount}");

            return $"{response} {request.ResourceType.ToString()} available";
        }
    }
}
