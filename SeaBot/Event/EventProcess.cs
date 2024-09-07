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
        public static void BotOnlineCheck(object sender, EventArgs e)
        {
            var logger = new Logger();
            logger.Info("Bot log in Sucessfully.", "OnBotOnlineEvent");
        }

        public static void BotOfflineCheck(object sender, EventArgs e)
        {
            var logger = new Logger();
            logger.Warning("Bot log out by server.", "OnBotOfflineEvent");
            FileStream stream = new(@"keystore.json", FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            stream.Close();
            File.Delete(@"keystore.json");
            Thread.Sleep(1000);
            if (Program.Bot.Config.UseApi)
            {
                Program.Bot._api.EndListener();
            }
            Program.Bot._bot.Dispose();
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
                if (!Program.Bot.Config.LimitUinList.IsWhiteList(eventEntity.Chain))
                {
                    logger.Info("GroupUin/FriendUin can not pass whitelist check.", "MessageCheck");
                    return;
                }
                logger.Info("Message passed whitelist check.", "MessageCheck");
                foreach (var item in eventEntity.Chain)
                {
                    if (item is TextEntity text)
                        logger.Info("Message is \"" + text.Text + "\"", "MessageEvent");
                    else if (item is ImageEntity)
                        logger.Info("Receive a image.", "MessageEvent");
                }
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
                if (!Program.Bot.Config.LimitUinList.IsWhiteList(eventEntity.Chain))
                {
                    logger.Info("GroupUin/FriendUin can not pass whitelist check.", "MessageCheck");
                    return;
                }
                logger.Info("Message passed whitelist check.", "MessageCheck");
                foreach (var item in eventEntity.Chain)
                {
                    if (item is TextEntity text)
                        logger.Info("Message is \"" + text.Text + "\" by " + eventEntity.Chain.FriendUin, "MessageEvent");
                    else if (item is ImageEntity)
                        logger.Info("Receive a image by " + eventEntity.Chain.FriendUin, "MessageEvent");
                }
                Message.Message.CommandParse(eventEntity.Chain);
            }
        }
    }
}
