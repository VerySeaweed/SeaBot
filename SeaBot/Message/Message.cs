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

        public static Bot? bot;

        public static void CommandParse(MessageChain chain)
        {
            try
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
                    logger.Info($"接收到指令: {message}", _name);
                    char[] tempc = message.ToCharArray();
                    tempc[0] = ' ';
                    string temps = new string(tempc).Trim();
                    string[] commands = temps.Split(' ');
                    switch (commands[0])
                    {
                        case "ycm":
                            var item_ycm = bot.DataBase.Data.Find(x => x is YouCarMa);
                            var ycm = new YouCarMa();
                            if (item_ycm != null)
                            {
                                ycm = item_ycm as YouCarMa;
                            }
                            else
                            {
                                ycm.bot = bot;
                                bot.DataBase.AddData(ycm);
                            }
                            ycm.ReceiveCommand(temps, chain);
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
                            echo.ReceiveCommand(temps, chain);
                            break;
                        case "g":
                        case "guess":
                            var item_guess = bot.DataBase.Data.Find(x => x is Guess);
                            var guess = new Guess();
                            if (item_guess != null)
                            {
                                guess = item_guess as Guess;
                            }
                            else
                            {
                                guess.bot = bot;
                                bot.DataBase.AddData(guess);
                            }
                            guess.ReceiveCommand(temps, chain);
                            break;
                        case "ping":
                            var ping = new Ping();
                            ping.ReceiveCommand(temps, chain);
                            break;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void SendMessage(MessageBuilder chain, MessageChain old)
        {
            bot.SendMessage(chain, old);
        }
        public static void SendMessage(MessageBuilder chain)
        {
            bot.SendMessage(chain);
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
