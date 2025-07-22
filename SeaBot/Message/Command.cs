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
        public Echo() 
        {
            this.unique_id = "echo";
        }
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
        public Status()
        {
            this.unique_id = "status";
        }
        protected override MessageBuilder Process(string command, MessageChain chain, MessageBuilder message)
        {
            message.Forward(chain);
            message.Text("SeaBot v0.1.3 running.\nSystem OS:" + Environment.OSVersion);
            message.Text("\n运行时间" + (DateTime.Now - Message.bot.StartTime));
            return message;
        }
    }
    internal class Ping : ModuleBase
    {
        public Ping()
        {
            this.unique_id = "ping";
        }
        protected override MessageBuilder Process(string command, MessageChain chain, MessageBuilder message)
        {
            message.Text($"嗯哼？（{(DateTime.Now - chain.Time)}）");
            return message;
        }
    }
    internal class Help : ModuleBase
    {
        public Help()
        {
            this.unique_id = "help";
        }
        protected override MessageBuilder Process(string command, MessageChain chain, MessageBuilder message)
        {
            message.Forward(chain);
            message.Text("在模块主命令后加入help参数以查看帮助:");
            message.Text("\necho <string:message> -返回message的内容，该模块没有帮助");
            message.Text("\nstatus -返回bot运行信息，该模块没有帮助");
            message.Text("\nycm -有车吗模块，请访问其帮助系统。");
            message.Text("\nguess -猜字母模块，请访问其帮助系统。");
            message.Text("\n更多模块帮助请使用modulelist命令获取主命令已查询其帮助系统。");
            return message;
        }
    }
    internal class ModuleList : ModuleBase
    {
        public ModuleList()
        {
            this.unique_id = "modulelist";
            this.commandAlias = new string[] { "ls" };
        }
        protected override MessageBuilder Process(string command, MessageChain chain, MessageBuilder message)
        {
            var strs = Message.GetModuleList();
            message.Forward(chain);
            foreach (var item in strs)
            {
                message.Text($"-{item}{(item == strs[strs.Length - 1] ? "" : "\n")}");
            }
            return message;
        }
    }
}
