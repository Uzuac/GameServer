using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var websocketUri = new Uri("wss://localhost:7110/ws");
        using var client = new ClientWebSocket();

        try
        {
            Console.WriteLine($"Connecting to WebSocket server at {websocketUri}...");
            await client.ConnectAsync(websocketUri, CancellationToken.None);
            Console.WriteLine("Connected!");

            // Player device IDs
            var player1DeviceId = "cdef583a-f5d1-49f0-aae9-40fefbb2ed52";
            var player2DeviceId = "62d6b0d7-c885-4d2c-8b82-4b3fbc518e3f";

            while (true)
            {
                Console.WriteLine("\nChoose an action:");
                Console.WriteLine("1. Login as Player 1");
                Console.WriteLine("2. Login as Player 2");
                Console.WriteLine("3. Update resources for Player 1");
                Console.WriteLine("4. Update resources for Player 2");
                Console.WriteLine("5. Send resources from Player 1 to Player 2");
                Console.WriteLine("6. Send resources from Player 2 to Player 1");
                Console.WriteLine("7. Exit");
                Console.Write("Enter your choice: ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await SendMessage(client, CreateLoginMessage(player1DeviceId));
                        await ReceiveMessage(client);
                        break;
                    case "2":
                        await SendMessage(client, CreateLoginMessage(player2DeviceId));
                        await ReceiveMessage(client);
                        break;
                    case "3":
                        await SendMessage(client, CreateUpdateResourceMessage(player1DeviceId, 1, 10));
                        await ReceiveMessage(client);
                        break;
                    case "4":
                        await SendMessage(client, CreateUpdateResourceMessage(player2DeviceId, 1, 10));
                        await ReceiveMessage(client);
                        break;
                    case "5":
                        await SendMessage(client, CreateSendResourceMessage(player1DeviceId, player2DeviceId, 1, 10));
                        await ReceiveMessage(client);
                        break;
                    case "6":
                        await SendMessage(client, CreateSendResourceMessage(player2DeviceId, player1DeviceId, 1, 10));
                        await ReceiveMessage(client);
                        break;
                    case "7":
                        Console.WriteLine("Exiting...");
                        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Exiting", CancellationToken.None);
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
