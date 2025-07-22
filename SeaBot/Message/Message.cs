using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using SeaBot.Module;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeaBot.Message
{
    internal class Message
    {
        public static MessageResult? LastResult;

        public static Bot? bot;

        public static List<ModuleBase> modules = new List<ModuleBase>();


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
                    for (int i = 0;i < modules.Count-1;i++)
                    {
                        if (commands[0] == modules[i].unique_id)
                        {
                            modules[i].ReceiveCommand(temps, chain);
                            break;
                        }
                        else
                        {
                            foreach (var str in modules[i].commandAlias)
                            {
                                if (commands[0] == str) 
                                {
                                    modules[i].ReceiveCommand(temps, chain);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void Init()
        {
            List<Type?> moduleTypes = GetUniqueId(typeof(ModuleBase));
            foreach (var type in moduleTypes)
            {
                var info = type.GetConstructor(new Type[0]);
                modules.Add(info.Invoke(null) as ModuleBase);
            }
        }
        protected static List<Type?> GetUniqueId(Type parent)
        {
            var temp = AppDomain.CurrentDomain.GetAssemblies();
            List<Type?> types = new List<Type?>();
            foreach (var item in temp)
            {
                if (item.FullName.StartsWith("System.") || item.FullName.StartsWith("Microsoft"))
                    continue;
                types = temp.SelectMany(x =>
                {
                    try
                    {
                        return x.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types.Where(t => t != null);
                    }
                }).Where(x => x.IsClass && x.IsSubclassOf(parent)).ToList();
            }
            return types;
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
                if (Groups.Length == 0)
                    limit = true;
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
                if (Friends.Length == 0)
                    limit = true;
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
