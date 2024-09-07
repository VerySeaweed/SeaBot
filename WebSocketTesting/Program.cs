using WebSocketSharp;
using SeaBot.ApiModule;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace WebSocketTesting
{
    internal class Program
    {
        static string? Access;

        static void Main(string[] args)
        {
            Process();
        }

        static void Process()
        {
            using WebSocket ws = new("ws://127.0.0.1:8082/ws");
            ws.OnOpen += (sender, e) => 
            {
                Console.WriteLine("Connected to the server.");
            };
            ws.OnClose += (sender, e) =>
            {
                Console.WriteLine("Connection closed");
            };
            ws.OnMessage += Get;
            ws.Connect();
            ApiText.Hello hello = new()
            {
                AccessCode = "sea",
                Action = "hello",
                StatusCode = EStatusCode.NotResponse,
                Type = EMessageType.Hello
            };
            Console.WriteLine(JsonSerializer.Serialize(hello));
            ws.Send(JsonSerializer.Serialize(hello));
            while (true)
            {
                ws.Ping();
                Thread.Sleep(5000);
                if (!ws.IsAlive)
                {
                    ws.Close();
                    break;
                }
            }
        }

        static void Get(object? sender,MessageEventArgs e)
        {
            Console.WriteLine(e.Data);
            var text = JsonSerializer.Deserialize<ApiText>(e.Data);
            if (text != null && text.Type == EMessageType.Hello)
            {
                var hello = JsonSerializer.Deserialize<ApiText.Hello>(e.Data);
                Access = hello.Data as string;
            }
            else if (text != null && text.AccessCode == Access)
            {

            }
        }
    }
}
