using WebSocketSharp;
using SeaBot.ApiModule;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace WebSocketTesting
{
    internal class Program
    {
        static string? Access;

        static uint? HeartInterval;

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
                AccessCode = null,
                Action = "hello",
                StatusCode = EStatusCode.NotResponse,
                Type = EMessageType.Hello
            };
            Console.WriteLine(JsonSerializer.Serialize(hello));
            ws.Send(JsonSerializer.Serialize(hello));
            while (true)
            {
                ApiText.Heart heart = new()
                {
                    AccessCode = Access
                };
                ws.Send(JsonSerializer.Serialize(heart));
                Thread.Sleep(Convert.ToInt32(HeartInterval));
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
                if (hello != null)
                {
                    Access = hello.Data as string;
                    HeartInterval = hello.HeartInterval;
                }
            }
            else if (text != null && text.AccessCode == Access)
            {

            }
        }
    }
}
