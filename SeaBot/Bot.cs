using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Lagrange;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Internal;
using Lagrange.Core.Event;
using Lagrange.Core.Message;
using Lagrange.Core.Utility;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Interface.Api;
using System.Diagnostics;
using Lagrange.Core.Utility.Extension;
using SeaBot.Event;
using SeaBot.Message;
using Lagrange.Core.Common.Entity;

namespace SeaBot
{
    internal class Bot
    {
        public BotDeviceInfo? _deviceInfo;

        public BotKeystore? _keyStore;

        public Config? Config;

        protected BotContext? _bot;

        private bool FirstLogin = false;

        public DataBase DataBase = new();

        public DateTime StartTime;


        public Bot()
        {
            StartTime = DateTime.Now;
            EventProcess.bot = this;
            Message.Message.bot = this;
        }


        protected bool Init()
        {
            var log = new Logger();
            const string _initName = "InitManager";
            bool configExisted = false;
            if (!File.Exists(@"device.json"))
            {
                BotDeviceInfo device = BotDeviceInfo.GenerateInfo();
                device.DeviceName = "SeaBotAlpha";
                Files.Create(@"device.json");
                Files.WriteInFiles(JsonSerializer.Serialize(device), @"device.json");
                configExisted = true;
                this._deviceInfo = device;
                log.Warning("Missing device.json", _initName);
                log.Info("Generated a device.json", _initName);
            }
            else
            {
                this._deviceInfo = JsonSerializer.Deserialize<BotDeviceInfo>(Files.ReadInFiles(@"device.json"));
            }
            if (!File.Exists(@"config.json"))
            {
                var config = new Config
                {
                    QQUin = 0,
                    CommandPrefix = new char[] { '.', '/', '_' },
                    UseQrCodeInsteadOfPassword = true,
                    LimitUinList = new Message.LimitUin(),
                    UseApi = false,
                    ApiListenedPort = 8082,
                    AccessCode = "please edit this to create you own access code."
                };
                config.SaveSelf();//save config
                this.Config = config;
                log.Warning("Missing config.json", _initName);
                log.Info("Generated a config.json", _initName);
                configExisted = true;
            }
            else
            {
                this.Config = JsonSerializer.Deserialize<Config>(Files.ReadInFiles(@"config.json"));
            }
            if (File.Exists(@"keystore.json"))
            {
                _keyStore = JsonSerializer.Deserialize<BotKeystore>(Files.ReadInFiles(@"keystore.json"));
            }
            else
            {
                _keyStore = new BotKeystore
                {
                    Uin = Config.QQUin
                };
                this.FirstLogin = true;
            }
            Files.Create(@"log.txt");
            return configExisted;
        }

        public async void Start()
        {
            try
            {
                var logger = new Logger();//init Logger
                const string _name = "BotStartManager";
                if (Init())//check files(config.json/device.json)
                {
                    logger.Info("Please edit config.json and restart the program.", _name);
                    return;
                }
                logger.Info("Initing...", _name);
                var bot = BotFactory.Create(new BotConfig(), _deviceInfo, _keyStore);//create bot
                this._bot = bot;
                bot.Invoker.OnBotOnlineEvent += EventProcess.BotOnlineCheck;
                bot.Invoker.OnBotOfflineEvent += EventProcess.BotOfflineCheck;
                if (FirstLogin || Config.UseQrCodeInsteadOfPassword)
                {
                    logger.Info($"Because {(FirstLogin ? "you are trying logining on the first time" : "")}{(Config.UseQrCodeInsteadOfPassword ? "of your preference" : "")}. Use QrCode login now.", _name);
                    var qc = await bot.FetchQrCode();//get qrcode
                    logger.Info("Please use your QQ app on your smartphone to scan the QrCode which should be open in your default photo app. If it doesn't work, open it from qrCode.png in currert path.", _name);
                    if (qc != null)
                    {
                        var (uri, qrCodeByte) = qc.Value;
                        using FileStream fs = File.Create(@"qrCode.png");
                        await fs.WriteAsync(qrCodeByte, new CancellationToken());
                        Process.Start(new ProcessStartInfo
                        {
                            Arguments = ".\\qrCode.png",
                            CreateNoWindow = true,
                            FileName = "powershell"
                        });//use app to open qrcode
                        logger.Info("Waiting for logining.", _name);
                    }
                    await bot.LoginByQrCode();//wait qrcode login
                }
                else if (!Config.UseQrCodeInsteadOfPassword)
                {
                    logger.Info("Because of your preference. Use keystore login now.", _name);
                    await bot.LoginByPassword();//wait password/keystore login
                }
                _keyStore = bot.UpdateKeystore();
                this.Config.QQUin = _keyStore.Uin;
                this.Config.SaveSelf();
                logger.Info("Bot Uin: " + Config.QQUin, _name);
                Files.Create(@"keystore.json");
                Files.WriteInFiles(JsonSerializer.Serialize(_keyStore), @"keystore.json");//save keystore
                logger.Info("Key Stored.", _name);
                bot.Invoker.OnFriendMessageReceived += EventProcess.BotReceiveMessage;
                bot.Invoker.OnGroupMessageReceived += EventProcess.BotReceiveMessage;
                logger.Info("If bot's log do not show message \"Bot login successfully.\", please restart bot.", _name);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void Stop()
        {
            _bot.Dispose();
        }

        public async void SendMessage(MessageBuilder chain, MessageChain old)
        {
            try
            {
                Random r = new Random();
                char[] randomCode = new char[8];
                for (int i = 0; i < 8; i++)
                {
                    randomCode[i] = (char)r.Next(0, 1000);
                }
                chain.Text("\n随机码：" + new string(randomCode));
                var message = chain.Build();
                Thread.Sleep(r.Next(1000, 3000));
                var logger = new Logger();
                logger.Info("Message.Seng request sent", "Message.Send");
                Message.Message.LastResult = await _bot.SendMessage(message);
                if (message.GroupUin != null)
                {
                    logger.Info("Send a message to group: " + old.GroupUin, "Message.Send");
                }
                else if (message.GroupUin == null)
                {
                    logger.Info("Send a message to friend: " + old.FriendUin, "Message.Send");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async void SendMessage(MessageBuilder chain)
        {
            try
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
                Message.Message.LastResult = await _bot.SendMessage(message);
                if (message.GroupUin != null)
                {
                    logger.Info("Send a message to group: " + message.GroupUin, "Message.Send");
                }
                else if (message.GroupUin == null)
                {
                    logger.Info("Send a message to friend: " + message.FriendUin, "Message.Send");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BotGroupMember>> FetchGroupMembers(uint groupUin)
        {
            return await _bot.FetchMembers(groupUin);
        }
    }
}
