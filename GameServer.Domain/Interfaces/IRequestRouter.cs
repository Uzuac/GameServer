using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain.Interfaces
{
    public interface IRequestRouter
    {
        Task RunCommandHandler(string deviceId, WebSocket socket);
    }
}
