using GameServer.Domain.Interfaces;

namespace GameServer.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IConnectionManager _connectionManager;
        public NotificationService(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }
        public async Task SendNotification(List<Guid> deviceIds, string message)
        {
            foreach (var deviceId in deviceIds)
            {
                var connection = _connectionManager.GetConnection(deviceId.ToString());
                if (connection != null)
                {
                    await connection.Send(message);
                }
            }
        }
    }
}
