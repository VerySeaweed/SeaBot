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

        public async void StartListener()
        {
            var logger = new Logger();
            IPAddress? ip = null;
            HttpServer? server =null;
            if (IPAddress.TryParse(Program.Bot.Config.WebSocketListenedUri.Host,out ip))
            {
                server = new HttpServer(ip, Program.Bot.Config.WebSocketListenedUri.Port, false);
            }
            else
            {
                ip = await Dns.GetHostAddressesAsync(Program.Bot.Config.WebSocketListenedUri.Host);
            }
        }

        public class ApiWebSocketBehavior : WebSocketBehavior
        {
            
        }
    }
}
