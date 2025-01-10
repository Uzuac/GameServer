using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var websocketUri = ("wss://localhost:7200/ws");
        using var clientPlayer1 = new ClientWebSocket();
        using var clientPlayer2 = new ClientWebSocket();

        try
        {
            // Player device IDs
            var player1DeviceId = "62d6b0d7-c885-4d2c-8b82-4b3fbc518e3f";
            var player2DeviceId = "803efc45-43fc-4266-823e-085a1350a230";

            Console.WriteLine($"Connecting to WebSocket server at {websocketUri}...");
            await clientPlayer1.ConnectAsync(new Uri(websocketUri + $"?deviceId={player1DeviceId}"), CancellationToken.None);
            await clientPlayer2.ConnectAsync(new Uri(websocketUri + $"?deviceId={player2DeviceId}"), CancellationToken.None);
            Console.WriteLine("Connected!");



            var player1Id = "0287c93e-b236-4c99-befa-22cdfba3e976";
            var player2Id = "8c64bc82-9db8-43ec-b310-09614dfe2395";

            while (true)
            {
                Console.WriteLine("\nChoose an action:");
                Console.WriteLine("1. Login as Player 1");
                Console.WriteLine("2. Login as Player 2");
                Console.WriteLine("3. Add 10 coins for Player 1");
                Console.WriteLine("4. Add 10 coins for Player 2");
                Console.WriteLine("5. Send 10 coins from Player 1 to Player 2");
                Console.WriteLine("6. Send 10 coins from Player 2 to Player 1");
                Console.WriteLine("7. Exit");
                Console.Write("Enter your choice: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await SendMessage(clientPlayer1, CreateLoginMessage(player1DeviceId));
                        await ReceiveMessage(clientPlayer1);
                        break;
                    case "2":
                        await SendMessage(clientPlayer2, CreateLoginMessage(player2DeviceId));
                        await ReceiveMessage(clientPlayer2);
                        break;
                    case "3":
                        await SendMessage(clientPlayer1, CreateUpdateResourceMessage(player1DeviceId, 0, 10));
                        await ReceiveMessage(clientPlayer1);
                        break;
                    case "4":
                        await SendMessage(clientPlayer2, CreateUpdateResourceMessage(player2DeviceId, 0, 10));
                        await ReceiveMessage(clientPlayer2);
                        break;
                    case "5":
                        await SendMessage(clientPlayer1, CreateSendResourceMessage(player1DeviceId, player2Id, 0, 10));
                        await ReceiveMessage(clientPlayer1);
                        break;
                    case "6":
                        await SendMessage(clientPlayer2, CreateSendResourceMessage(player2DeviceId, player1Id, 0, 10));
                        await ReceiveMessage(clientPlayer2);
                        break;
                    case "7":
                        Console.WriteLine("Exiting...");
                        await clientPlayer1.CloseAsync(WebSocketCloseStatus.NormalClosure, "Exiting", CancellationToken.None);
                        await clientPlayer2.CloseAsync(WebSocketCloseStatus.NormalClosure, "Exiting", CancellationToken.None);
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static string CreateLoginMessage(string deviceId)
    {
        return $@"
        {{
            ""Path"": ""/login"",
            ""MessageContent"": {{
                ""DeviceId"": ""{deviceId}""
            }}
        }}";
    }

    static string CreateUpdateResourceMessage(string deviceId, int resourceType, int resourceValue)
    {
        return $@"
        {{
            ""Path"": ""/resources/update"",
            ""MessageContent"": {{
                ""DeviceId"": ""{deviceId}"",
                ""ResourceType"": {resourceType},
                ""ResourceValue"": {resourceValue}
            }}
        }}";
    }

    static string CreateSendResourceMessage(string deviceId, string friendPlayerId, int resourceType, int resourceValue)
    {
        return $@"
        {{
            ""Path"": ""/resources/send"",
            ""MessageContent"": {{
                ""DeviceId"": ""{deviceId}"",
                ""FriendPlayerId"": ""{friendPlayerId}"",
                ""ResourceType"": {resourceType},
                ""ResourceValue"": {resourceValue}
            }}
        }}";
    }

    static async Task SendMessage(ClientWebSocket client, string message)
    {
        Console.WriteLine($"\nSending message:\n{message}");
        var bytes = Encoding.UTF8.GetBytes(message);
        var buffer = new ArraySegment<byte>(bytes);

        await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        Console.WriteLine("Message sent.");
    }

    static async Task ReceiveMessage(ClientWebSocket client)
    {
        var buffer = new ArraySegment<byte>(new byte[4096]);
        WebSocketReceiveResult result;

        do
        {
            result = await client.ReceiveAsync(buffer, CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("Server closed the connection.");
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                return;
            }

            var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
            Console.WriteLine($"Received message:\n{message}");
        }
        while (!result.EndOfMessage);
    }
}
