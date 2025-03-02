using Lagrange.Core.Message;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBot.Module
{
    internal class ModuleBase
    {
        public Bot bot = Message.Message.bot;

        public ModuleBase() 
        {

        }

        public virtual void ReceiveCommand(string command, MessageChain chain)
        {
            if (chain.GroupUin != null)
                Group(command, chain);
            else
                Friend(command, chain);
        }

        protected virtual void Friend(string command, MessageChain chain)
        {
            var messageChain = MessageBuilder.Friend(chain.FriendUin);
            var message = Process(command, chain, messageChain);
            bot.SendMessage(message, chain);
        }

        protected virtual void Group(string command, MessageChain chain)
        {
            var messageChain = MessageBuilder.Group(Convert.ToUInt32(chain.GroupUin));
            var message = Process(command, chain, messageChain);
            bot.SendMessage(message, chain);
        }

        protected virtual MessageBuilder Process(string command, MessageChain chain, MessageBuilder message)
        {
            return message;
        }

        protected virtual MessageBuilder Help(MessageChain chain,MessageBuilder message)
        {
            return message;
        }
    }
}
