using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SeaBotV2
{
    internal class Bot
    {
        private WebSocketServer? server;

        internal void Start()
        {
            server = new WebSocketServer(IPAddress.Loopback, 8080);
            server.AddWebSocketService<SeaBotWSBehavir>("/onebot/v11/ws");
        }


        protected class SeaBotWSBehavir : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                base.OnOpen();
            }
            protected override void OnClose(CloseEventArgs e)
            {
                base.OnClose(e);
            }
            protected override void OnError(WebSocketSharp.ErrorEventArgs e)
            {
                base.OnError(e);
            }
            protected override void OnMessage(MessageEventArgs e)
            {
                base.OnMessage(e);
            }
        }
    }
}
