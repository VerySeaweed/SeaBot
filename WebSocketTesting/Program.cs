using System.Net.WebSockets;

namespace WebSocketTesting
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Process();
        }

        static async void Process()
        {
            var socket = new ClientWebSocket();
            socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(500);
            Console.WriteLine(socket.State);
            Uri uri = new Uri("ws://127.0.0.1:8082/");
            Console.WriteLine(uri.Host + ":" + uri.Port);
            await socket.ConnectAsync(new Uri("ws://127.0.0.1:8082/"), CancellationToken.None);
            await KeepConnectionActive(socket);
            Console.WriteLine(socket.State);
            Console.ReadKey();
            while (true)
            {
                await socket.SendAsync(new byte[] { 1, 1, 1 }, WebSocketMessageType.Text, false, new CancellationToken());
                Console.WriteLine("Sent");
                Thread.Sleep(5000);
                await socket.ReceiveAsync(new ArraySegment<byte>(), new CancellationToken());
            }
        }

        static async Task KeepConnectionActive(ClientWebSocket clientWebSocket)
        {
            if (clientWebSocket.State == WebSocketState.Open)
            {
                // 这里可以发送和接收消息
                Console.WriteLine("Press any key to exit...");
                await Task.Run(() => Console.ReadKey());
            }
        }
    }
}
