using GameServer.Application.Routing;
using GameServer.Domain.Commands;
using GameServer.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GameServer.Application.Routing
{
    public class RequestRouter : IRequestRouter
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RequestRouter> _logger;
        private readonly IConnectionManager _connectionManager;
        public RequestRouter(IMediator mediator, ILogger<RequestRouter> logger, IConnectionManager connectionManager)
        {
            _mediator = mediator;
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public async Task RunCommandHandler(string deviceId, WebSocket socket)
        {
            var deviceUuid = Guid.Parse(deviceId);
            var connection = await _connectionManager.HandleConnection(deviceUuid, socket);

            if (connection != null)
            {
                try
                {
                    await connection.Receive(async (message) => await HandleCommand(deviceUuid, message));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Application error: {ex.Message}");
                    _connectionManager.RemoveConnection(deviceUuid);
                }
            }
        }
        private Message ParseRouteFromMessage(string message)
        {
            var routedMessage = JsonSerializer.Deserialize<Message>(message);

            return routedMessage;
        }

        public async Task<string> HandleCommand(Guid deviceId, string message)
        {
            var routedMessage = ParseRouteFromMessage(message);
            IRequest<string> request;
            switch (routedMessage.Path)
            {
                case "/login":
                    request = JsonSerializer.Deserialize<LoginCommand>(routedMessage.MessageContent);
                    break;
                case "/resources/update":
                    request = JsonSerializer.Deserialize<UpdateResourcesCommand>(routedMessage.MessageContent);
                    break;
                case "/resources/send":
                    request = JsonSerializer.Deserialize<SendResourcesCommand>(routedMessage.MessageContent);
                    break;
                default:
                    _logger.LogError("Unknown route.");
                    return "Unknown route.";
            }

            return await _mediator.Send(request);
        }
    }

    public class Message
    {
        public string Path { get; set; }

        [JsonConverter(typeof(JsonRawStringConverter))]
        public string MessageContent { get; set; }
    }
}
