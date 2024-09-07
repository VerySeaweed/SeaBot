using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using WebSocketSharp;

namespace SeaBot.ApiModule
{
    internal class Api
    {
        public HttpServer? Server;

        public void StartListener()
        {
            var logger = new Logger();
            var server = new HttpServer(Program.Bot.Config.ApiListenedPort, false);
            server.AddWebSocketService<ApiWebSocketBehavior>("/ws");
            Server = server;
            server.Start();
            logger.Info("Api started.", "ApiManager");
        }

        public void StopListener()
        {
            var logger = new Logger();
            logger.Info("Api stopped.", "ApiManager");
            Server.Stop();
        }

        public class ApiWebSocketBehavior : WebSocketBehavior
        {

            public List<WebSocketSharp.WebSocket> Sockets = new();

            protected override void OnOpen()
            {
                base.OnOpen();
                
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                Logger logger = new();
                base.OnMessage(e);
                ApiText? text = JsonSerializer.Deserialize<ApiText>(e.Data);
                if (text!=null)
                {

                }
                else
                {
                    logger.Warning("Received a null message.", "ApiReceive");
                }
            }
        }
    }
}
