using WebSocketSharp;
using SeaBot.ApiModule;

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
            ws.OnMessage += (sender, e) => 
            {
                Console.WriteLine("Received: " + e.Data);
            };
            ws.Connect();
            while (true)
            {
                ApiText.ApiTextHello hello = new();
                hello.Action = "hello";
                hello.StatusCode = (int)EStatusCode.NotResponse;
            }
        }
    }
}
