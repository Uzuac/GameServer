using GameServer.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain.Interfaces
{
    public interface IConnectionManager
    {
        Task<Connection> HandleConnection(Guid deviceId, WebSocket webSocket);
        void RemoveConnection(Guid deviceId);
        Connection GetConnection(string deviceId);
    }
}
