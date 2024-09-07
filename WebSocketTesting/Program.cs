using WebSocketSharp;
using SeaBot.ApiModule;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace WebSocketTesting
{
    enum EStatusCode
    {
        Success = 0,
        Hello,
        Processing,
        Failed,
        NotResponse = -1,
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Process();
        }

        static async void Process()
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
            ws.OnMessage += (sender, e) => 
            {
                Console.WriteLine("Received: " + e.Data);
            };
            ws.Connect();
            ApiText.ApiTextHello hello = new()
            {
                Action = "hello",
                StatusCode = (int)EStatusCode.NotResponse
            };
            ws.Send(JsonSerializer.Serialize(hello));
            Console.ReadLine();
            ws.Close();
        }
    }
}
