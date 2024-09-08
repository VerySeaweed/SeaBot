﻿using System;
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
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;

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
                var text = JsonSerializer.Deserialize<ApiText>(e.Data);
                if (text != null && text.AccessCode == Program.Bot.Config.AccessCode)
                {
                    switch (text.Type)
                    {
                        case EMessageType.Hello:
                            var hello = JsonSerializer.Deserialize<ApiText.Hello>(e.Data);
                            if (hello != null)
                                Hello(hello);
                            break;
                        case EMessageType.Request:
                            var request = JsonSerializer.Deserialize<ApiText.Request>(e.Data);
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
                    }
                }
            }

            protected override void OnClose(CloseEventArgs e)
            {
                base.OnClose(e);
                Logger logger = new();
                logger.Info($"Connection closed. Reason: {e.Reason}", "Api");
            }

            protected void Hello(ApiText.Hello text)
            {
                if (text == null)
                    return;
                ApiText.Hello hello = new()
                {
                    Action = "response",
                    Guid = text.Guid,
                    StatusCode = EStatusCode.Hello,
                    Type = EMessageType.Hello
                };
                Send(JsonSerializer.Serialize(hello));
            }

            protected void Request(ApiText.Request request)
            {
                if (request == null)
                    return;
                if (request.Action == "send_private_msg")
                {
                    ApiText.Message? origin = request.Data as ApiText.Message;
                    if (origin != null)
                    {
                        var message = MessageBuilder.Friend(origin.FriendUin);
                        foreach (var item in origin.messageEntity)
                        {
                            if (item is ApiText.Message.TextEntity)
                            {
                                var temp = item as ApiText.Message.TextEntity;
                                message.Text(temp.Text);
                            }
                            else if (item is ApiText.Message.ImageEntity)
                            {
                                var temp = item as ApiText.Message.ImageEntity;
                                message.Image(temp.ImagePath);
                            }
                            else if (item is ApiText.Message.ForwardEntity)
                            {
                                var temp = item as ApiText.Message.ForwardEntity;
                                message.Add(new ForwardEntity() { Sequence = temp.Sequence });
                            }
                            else if (item is ApiText.Message.MentionEntity)
                            {
                                var temp = item as ApiText.Message.MentionEntity;
                                message.Mention(temp.TargetUin);
                            }
                        }
                        Message.Message.SendMessage(message);
                    }
                }
                else if (request.Action == "send_group_msg")
                {
                    ApiText.Message? origin = request.Data as ApiText.Message;
                    if (origin != null)
                    {
                        var message = MessageBuilder.Group(Convert.ToUInt32(origin.GroupUin));
                        foreach (var item in origin.messageEntity)
                        {
                            if (item is ApiText.Message.TextEntity)
                            {
                                var temp = item as ApiText.Message.TextEntity;
                                message.Text(temp.Text);
                            }
                            else if (item is ApiText.Message.ImageEntity)
                            {
                                var temp = item as ApiText.Message.ImageEntity;
                                message.Image(temp.ImagePath);
                            }
                            else if (item is ApiText.Message.ForwardEntity)
                            {
                                var temp = item as ApiText.Message.ForwardEntity;
                                message.Add(new ForwardEntity() { Sequence = temp.Sequence });
                            }
                            else if (item is ApiText.Message.MentionEntity)
                            {
                                var temp = item as ApiText.Message.MentionEntity;
                                message.Mention(temp.TargetUin);
                            }
                        }
                        Message.Message.SendMessage(message);
                    }
                }
            }
        }
    }
}
