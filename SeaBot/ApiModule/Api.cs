using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SeaBot.ApiModule
{
    internal class Api
    {
        public HttpListener? Listener;

        public WebSocket? WebSocket;

        public async void StartListener()
        {
            var logger = new Logger();
            using var listener = new HttpListener();
            listener.Prefixes.Add(Program.Bot.Config.WebSocketListenedUri);
            listener.Start();
            logger.Info("ApiListener is running on " + Program.Bot.Config.WebSocketListenedUri, "HttpListener");
            Listener = listener;
            while (true)
            {
                logger.Info("Waiting new websocket connectiong...", "HttpListener");
                var context = await Listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    logger.Info("Received websocket connectiong.", "HttpListener");
                    HttpListenerWebSocketContext socket = await context.AcceptWebSocketAsync(null);
                    WebSocket webSocket = socket.WebSocket;
                    WebSocket = webSocket;
                    Listener.Stop();
                    await Process(WebSocket);
                    Listener.Start();
                }
                else
                {
                    logger.Warning("Received a wrong websocket connection. Connecting failed.", "HttpListener");
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Close();
                }
            }
        }

        public void EndListener()
        {
            Logger logger = new();
            logger.Info("Listener Closed", "HttpListener");
            if (WebSocket != null)
            {
                WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "server closed", new CancellationToken());
            }
            Listener.Close();
        }

        protected async Task Process(WebSocket socket)
        {
            while (socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(new byte[4096]), new CancellationToken());
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, new CancellationToken());
                    return;
                }
            }
        }
    }
}
