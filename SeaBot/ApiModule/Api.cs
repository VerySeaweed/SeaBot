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
        public HttpServer? HttpServer;

        public void StartListener()
        {
            var logger = new Logger();
            var server = new HttpServer();

        }
    }
}
