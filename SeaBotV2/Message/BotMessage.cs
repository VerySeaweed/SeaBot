using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBotV2.Message
{
    internal class BotMessage
    {
        public string? status { get; set; }
        public int retcode { get; set; }
        public object? data { get; set; }
    }
}
