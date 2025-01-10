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
        public async Task SendNotification(string deviceUuid, string message)
        {
            var connection = _connectionManager.GetConnection(deviceUuid);
            if (connection != null)
            {
                await connection.Send(message);
            }
        }
    }
}
