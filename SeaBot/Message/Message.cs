using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using SeaBot.Module;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeaBot.Message
{
    internal class Message
    {
        public static MessageResult? LastResult;

        public static void CommandParse(MessageChain chain)
        {
            const string _name = "Message";
            var config = JsonSerializer.Deserialize<Config>(Files.ReadInFiles(@"config.json"));
            bool isCommand = false;
            string? message = null;
            var logger = new Logger();
            foreach (var item in chain)
            {
                if (item is TextEntity text)
                    message = text.Text;
            }
            if (message == null)
                return;
            for (int i = 0; i < config.CommandPrefix.Length; i++)
            {
                if (config.CommandPrefix[i] == message[0])
                {
                    isCommand = true;
                    break;
                }
            }
            if (isCommand)
            {
                logger.Info($"Call command: {message}", _name);
                char[] tempc = message.ToCharArray();
                tempc[0]=' ';
                string temps = new string(tempc).Trim();
                string[] commands = temps.Split(' ');
                switch (commands[0])
                {
                    case "ycm":
                        var item_ycm = Program.Bot.TempData.Find(x => x is YouCarMa);
                        var ycm = new YouCarMa();
                        if (item_ycm != null)
                        {
                            ycm = item_ycm as YouCarMa;
                            Program.Bot.RemoveData(item_ycm);
                        }
                        ycm.ReceiveCommand(temps, chain);
                        Program.Bot.AddData(ycm);
                        break;
                    case "status":
                        var status = new Status();
                        status.ReceiveCommand(temps, chain);
                        break;
                    case "help":
                        var help = new Help();
                        help.ReceiveCommand(temps, chain);
                        break;
                    case "echo":
                        var echo = new Echo();
                        echo.Sendback(temps, chain);
                        break;
                    case "g":
                    case "guess":
                        var item_guess = Program.Bot.TempData.Find(x => x is Guess);
                        var guess = new Guess();
                        if (item_guess != null)
                        {
                            guess = item_guess as Guess;
                            Program.Bot.RemoveData(item_guess);
                        }
                        guess.ReceiveCommand(temps, chain);
                        Program.Bot.AddData(guess);
                        break;
                }
            }
        }

        public async static void SendMessage(MessageBuilder chain, MessageChain old)
        {
            Random r = new Random();
            char[] randomCode = new char[8];
            for (int i = 0; i < 8; i++ )
            {
                randomCode[i] = (char)r.Next(0, 1000);
            }
            chain.Text("\n随机码：" + new string(randomCode));
            var message = chain.Build();
            Thread.Sleep(r.Next(1000, 3000));
            var logger = new Logger();
            logger.Info("Message.Seng request sent", "Message.Send");
            LastResult = await Program.Bot._bot.SendMessage(message);
            if (message.GroupUin != null)
            {
                logger.Info("Send a message to group: " + old.GroupUin, "Message.Send");
            }
            else if (message.GroupUin == null)
            {
                logger.Info("Send a message to friend: " + old.FriendUin, "Message.Send");
            }
        }

        public async static void SendMessage(MessageBuilder chain)
        {
            Random r = new Random();
            char[] randomCode = new char[8];
            for (int i = 0; i < 8; i++)
            {
                randomCode[i] = (char)r.Next(0, 1000);
            }
            chain.Text("\n随机码：" + new string(randomCode));
            var message = chain.Build();
            Thread.Sleep(r.Next(1000, 5000));
            var logger = new Logger();
            logger.Info("Message.Seng request sent", "Message.Send");
            LastResult = await Program.Bot._bot.SendMessage(message);
            if (message.GroupUin != null)
            {
                logger.Info("Send a message to group: " + message.GroupUin, "Message.Send");
            }
            else if (message.GroupUin == null)
            {
                logger.Info("Send a message to friend: " + message.FriendUin, "Message.Send");
            }
        }
    }

    internal class LimitUin
    {
        public uint[]? Groups { get; set; }
        public uint[]? Friends { get; set; }

        public bool IsWhiteList(MessageChain chain)
        {
            bool limit = false;
            if (chain.GroupUin!=null)
            {
                foreach (var item in Groups)
                {
                    if (chain.GroupUin == item)
                    {
                        limit = true;
                        break;
                    }
                }
            }
            else if (chain.GroupUin==null)
            {
                foreach (var item in Friends)
                {
                    if (chain.FriendUin == item)
                    {
                        limit = true;
                        break;
                    }
                }
            }
            return limit;
        }
    }
}
