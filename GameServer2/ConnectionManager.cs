using GameServer.Domain.Interfaces;
using Microsoft.AspNetCore.Components.Routing;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace GameServer.Application.Services
{
    public class ConnectionManager : IConnectionManager
    {
        public ConcurrentDictionary<Guid, Connection> _connections = new ConcurrentDictionary<Guid, Connection>();

        public Connection GetConnection(string deviceId)
        {
            Connection connection;

            var deviceGuid = Guid.Parse(deviceId);
            var connected = _connections.TryGetValue(deviceGuid, out connection);

            return connection;
        }
        public async Task<Connection> HandleConnection(Guid deviceId, WebSocket webSocket)
        {
            Connection connection;

            var connected = _connections.TryGetValue(deviceId, out connection);
            if (!connected)
            {
                connection = new Connection() { DeviceId = deviceId, Socket = webSocket };
                _connections.TryAdd(deviceId, connection);
            }
            else
            {
                connection.Socket = webSocket;
            }

            return connection;
        }

        public void RemoveConnection(Guid deviceId)
        {
            Connection connection;
            _connections.Remove(deviceId, out connection);
            if (connection != null)
            {

            }
        }
    }
}
