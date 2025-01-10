using System.ComponentModel.DataAnnotations;
using System.Net.WebSockets;
using System.Text;

namespace GameServer.Application.Services
{
    public class Connection
    {
        public Guid DeviceId { get; set; }
        public WebSocket Socket { get; set; }

        public async Task Receive(Func<string, Task<string>> handleMessage)
        {
            var buffer = new byte[1024 * 2];
            while (Socket.State == WebSocketState.Open)
            {
                var result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), default);
                if (result.EndOfMessage == false)
                {
                    await Socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Max message size is 2KiB.", default);
                    return;
                }
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, default);
                    return;
                }
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    await Socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Expected text message", default);
                    return;
                }
                var messageString = Encoding.UTF8.GetString(buffer[..result.Count]);

                var response = await handleMessage(messageString);
                if (!string.IsNullOrEmpty(response))
                {
                    var responseBuffer = Encoding.UTF8.GetBytes(response);
                    var segment = new ArraySegment<byte>(responseBuffer);
                    await Socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public async Task Send(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);
            await Socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
