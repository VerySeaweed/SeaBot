using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Lagrange.Core.Common;
using Lagrange.Core.Event.EventArg;
using SeaBot.Message;
using Lagrange.Core.Message.Entity;
using System.Security.Claims;
using System.Xml.Linq;
using System.Diagnostics.Tracing;

namespace SeaBot.Event
{
    internal class EventProcess
    {
        public static Bot? bot;

        public static void BotOnlineCheck(object sender, EventArgs e)
        {
            var logger = new Logger();
            logger.Info("Bot log in Sucessfully.", "BotOnlineEvent");
        }

        public static void BotOfflineCheck(object sender, EventArgs e)
        {
            var logger = new Logger();
            logger.Warning("Bot log out by server.", "BotOfflineEvent");
            FileStream stream = new(@"keystore.json", FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            stream.Close();
            File.Delete(Path.Combine(Environment.CurrentDirectory, @"keystore.json"));
            Thread.Sleep(1000);
            //if (bot.Config.UseApi && bot._api != null)
            //{
            //    bot._api.StopListener();
            //}
            bot.Stop();
        }

        public static void BotReceiveMessage(object sender, EventArgs e)
        {
            var logger = new Logger();
            if (e is FriendMessageEvent)
            {
                var eventEntity = e as FriendMessageEvent;
                if (Message.Message.LastResult != null)
                {
                    if (eventEntity.Chain.Sequence == Message.Message.LastResult.Sequence)
                    {
                        return;
                    }
                }
                logger.Info("Received friend message from friend: " + eventEntity.Chain.FriendUin, "MessageEvent");
                if (!bot.Config.LimitUinList.IsWhiteList(eventEntity.Chain))
                {
                    logger.Info("GroupUin/FriendUin can not pass whitelist check.", "MessageCheck");
                    return;
                }
                logger.Info("Message passed whitelist check.", "MessageCheck");
                Message.Message.CommandParse(eventEntity.Chain);
            }
            else if (e is GroupMessageEvent)
            {
                var eventEntity = e as GroupMessageEvent;
                if (Message.Message.LastResult != null)
                {
                    if (eventEntity.Chain.Sequence == Message.Message.LastResult.Sequence)
                    {
                        return;
                    }
                }
                logger.Info("Received group message from group: " + eventEntity.Chain.GroupUin, "MessageEvent");
                if (!bot.Config.LimitUinList.IsWhiteList(eventEntity.Chain))
                {
                    logger.Info("GroupUin/FriendUin can not pass whitelist check.", "MessageCheck");
                    return;
                }
                logger.Info("Message passed whitelist check.", "MessageCheck");
                Message.Message.CommandParse(eventEntity.Chain);
            }
        }
    }
}
