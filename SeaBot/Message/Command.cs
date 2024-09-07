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
    internal class Echo
    {
        public void Sendback(string commands,MessageChain chain)
        {
            if (chain.GroupUin != null)
                Group(commands, chain);
            else
                Friend(commands, chain);
        }
        protected void Group(string commands,MessageChain chain)
        {
            var messageChain = MessageBuilder.Group(Convert.ToUInt32(chain.GroupUin));
            messageChain.Forward(chain);
            if (commands.Length<=5)
                messageChain.Text("我和你爆了");
            else
                messageChain.Text(commands.Remove(0, 5));
            Message.SendMessage(messageChain, chain);
        }
        protected void Friend(string commands, MessageChain chain)
        {
            var messageChain = MessageBuilder.Friend(chain.FriendUin);
            messageChain.Forward(chain);
            if (commands.Length <= 5)
                messageChain.Text("我和你爆了");
            else
                messageChain.Text(commands.Remove(0, 5));
            Message.SendMessage(messageChain, chain);
        }
    }
    internal class Status : ModuleBase
    {
        protected override MessageBuilder Process(string command, MessageChain chain, MessageBuilder message)
        {
            message.Forward(chain);
            message.Text("SeaBot v0.1.1 running.\nSystem OS:" + Environment.OSVersion);
            message.Text("\nRunning time:" + (DateTime.Now - Program.Bot.StartTime));
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
