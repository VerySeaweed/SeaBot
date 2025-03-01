using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using SeaBot.Event;
using SeaBot.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBot.Message
{
    internal class Echo : ModuleBase
    {
        protected override MessageBuilder Process(string command, MessageChain chain, MessageBuilder message)
        {
            message.Forward(chain);
            if (command.Length <= 5)
                message.Text("我和你爆了");
            else
                message.Text(command.Remove(0, 5));
            return message;
        }
    }
    internal class Status : ModuleBase
    {
        protected override MessageBuilder Process(string command, MessageChain chain, MessageBuilder message)
        {
            message.Forward(chain);
            message.Text("SeaBot v0.1.1 running.\nSystem OS:" + Environment.OSVersion);
            message.Text("\nRunning time:" + (DateTime.Now - Message.bot.StartTime));
            return message;
        }
    }
    internal class Help : ModuleBase
    {
        protected override MessageBuilder Process(string command, MessageChain chain, MessageBuilder message)
        {
            message.Forward(chain);
            message.Text("在模块主命令后加入help参数以查看帮助:");
            message.Text("\necho <string:message> -返回message的内容，该模块没有帮助");
            message.Text("\nstatus -返回bot运行信息，该模块没有帮助");
            message.Text("\nycm -有车吗模块，帮助请访问其帮助系统。");
            return message;
        }
    }
}
