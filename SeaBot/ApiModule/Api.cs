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
using System.ComponentModel.DataAnnotations;

namespace SeaBot.ApiModule
{
    internal class Api
    {
        public HttpServer? Server;

        public void StartListener()
        {
            var logger = new Logger();
            var server = new HttpServer(IPAddress.Loopback, Program.Bot.Config.ApiListenedPort, false);
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

        protected class ApiWebSocketBehavior : WebSocketBehavior
        {
            protected List<WebSocketSharp.WebSocket> sockets = new();

            protected override void OnOpen()
            {
                base.OnOpen();
                sockets.Add(Context.WebSocket);
                Logger logger = new();
                logger.Info("Connection accepted.", "Api");
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                Logger logger = new();
                base.OnMessage(e);
                logger.Info("Receive a message.", "Api");
                ApiText? text = JsonSerializer.Deserialize<ApiText>(e.Data);
                if (text!=null)
                {
                    if (text is ApiText.ApiTextHello)
                    {
                        logger.Info("ss", "ss");
                        Hello(text as ApiText.ApiTextHello);
                    }
                }
                else
                {
                    logger.Warning("Received a null message.", "Api");
                }
            }

            protected override void OnClose(CloseEventArgs e)
            {
                base.OnClose(e);
                Logger logger = new();
                logger.Info($"Connection closed. Reason: {e.Reason}", "Api");
            }

            protected void Hello(ApiText.ApiTextHello text)
            {
                if (text == null)
                {
                    return;
                }
                ApiText.ApiTextHello hello = new()
                {
                    Action = "response",
                    Guid = text.Guid,
                    StatusCode = (int)EStatusCode.Hello
                };
                Send(JsonSerializer.Serialize(hello));
            }
        }
    }
}
