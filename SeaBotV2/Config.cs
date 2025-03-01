using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace SeaBotV2
{
    internal class Config
    {
        public string? AccessCode { get; set; }

        public char[]? CommandPrefix { get; set; }

        public bool UseApi { get; }

        public int ApiListenedPort { get; }


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
