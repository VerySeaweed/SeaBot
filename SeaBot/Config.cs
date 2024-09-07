using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using SeaBot.Message;

namespace SeaBot
{
    internal class Config
    {
        public uint QQUin { get; set; }

        public char[]? CommandPrefix { get; set; }

        public bool UseQrCodeInsteadOfPassword { get; set; }

        public LimitUin? LimitUinList { get; set; }

        public bool UseApi { get; set; }

        public Uri? WebSocketListenedUri { get; set; }

        public void SaveSelf()
        {
            const string configPath = @"config.json";
            Files.Create(configPath);
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
            };
            Files.WriteInFiles(JsonSerializer.Serialize(this, options), configPath);
        }
    }
}
