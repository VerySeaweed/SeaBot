﻿using System;
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
            logger.Info("登录成功", "BotOnlineEvent");
        }

        public static void BotCaptchaCheck(object sender, EventArgs e)
        {
            var logger = new Logger();
            logger.Info("收到验证码", "BotCaptchaEvent");
        }

        public static void BotOfflineCheck(object sender, EventArgs e)
        {
            var logger = new Logger();
            logger.Warning("服务器强制下线", "BotOfflineEvent");
            FileStream stream = new(@"keystore.json", FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            stream.Close();
            File.Delete(Path.Combine(Environment.CurrentDirectory, @"keystore.json"));
            Thread.Sleep(1000);
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
                logger.Info($"接收到来自好友{eventEntity.Chain.FriendUin}的消息", "MessageEvent");
                if (!bot.Config.LimitUinList.IsWhiteList(eventEntity.Chain))
                {
                    logger.Info($"来自好友{eventEntity.Chain.FriendUin}的消息未通过白名单验证", "MessageCheck");
                    return;
                }
                logger.Info($"来自好友{eventEntity.Chain.FriendUin}的消息通过白名单验证", "MessageCheck");
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
                logger.Info($"接收到来自群聊{eventEntity.Chain.GroupUin}的消息", "MessageEvent");
                if (!bot.Config.LimitUinList.IsWhiteList(eventEntity.Chain))
                {
                    logger.Info($"来自群聊{eventEntity.Chain.GroupUin}的消息未通过白名单验证", "MessageCheck");
                    return;
                }
                logger.Info($"来自群聊{eventEntity.Chain.GroupUin}的消息通过白名单验证", "MessageCheck");
                Message.Message.CommandParse(eventEntity.Chain);
            }
        }
    }
}
