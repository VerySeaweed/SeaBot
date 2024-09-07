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
            logger.Info($"Api started on {server.Address}:{server.Port}", "ApiManager");
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

            bool helloSend = false;

            protected override void OnOpen()
            {
                base.OnOpen();
                sockets.Add(Context.WebSocket);
                Logger logger = new();
                bool close = true;
                logger.Info("Connection accepted.", "Api");
                for (int i = 0; i < 50; i++)
                {
                    if (helloSend)
                    {
                        close = false;
                        break;
                    }
                    Thread.Sleep(100);
                }
                if (!close)
                {
                    logger.Warning("Because not receive hello message yet, connection closed automatically.", "Api");
                }
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                Logger logger = new();
                base.OnMessage(e);
                logger.Info("Receive a message.", "Api");
                var text = JsonSerializer.Deserialize<ApiText>(e.Data);
                if (text != null && text.AccessCode == Program.Bot.Config.AccessCode)
                {
                    switch (text.Type)
                    {
                        case EMessageType.Hello:
                            var hello = JsonSerializer.Deserialize<ApiText.ApiTextHello>(e.Data);
                            if (hello != null)
                                Hello(hello);
                            helloSend = true;
                            break;
                        case EMessageType.Request:
                            var request = JsonSerializer.Deserialize<ApiText.ApiTextRequest>(e.Data);
                            break;
                        case EMessageType.Response:
                            break;
                        case EMessageType.Event:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (text == null)
                    {
                        logger.Warning("Received a null message", "Api");
                    }
                    else if (text.AccessCode != Program.Bot.Config.AccessCode)
                    {
                        logger.Warning("Access code is incorrect", "Api");
                        var response = new ApiText.ApiTextResponse()
                        {
                            Action = "response",
                            StatusCode = EStatusCode.Failed,
                            Type = EMessageType.Response,
                            AccessCode = text.AccessCode,
                            Data = "incorrect access code"
                        };
                    }
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
                    return;
                char[] chars = new char[10];
                Random r = new();
                for (int i = 0; i < 10; i++)
                {
                    chars[i] = (char)r.Next(0, 1000);
                }
                ApiText.ApiTextHello hello = new()
                {
                    Action = "response",
                    Guid = text.Guid,
                    StatusCode = EStatusCode.Hello,
                    Type = EMessageType.Hello,
                    Data = new string(chars)
                };
                Send(JsonSerializer.Serialize(hello));
            }

            protected void Request(ApiText.ApiTextRequest request)
            {
                if (request == null)
                    return;
                if (request.Action=="get_frienf_list")
                {

                }
            }
        }
    }
}
