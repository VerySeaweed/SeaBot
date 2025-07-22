using System.Text.Json;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Message;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Interface.Api;
using System.Diagnostics;
using SeaBot.Event;
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
            Files.Create(@"log.txt");
            if (!File.Exists(@"device.json"))
            {
                BotDeviceInfo device = BotDeviceInfo.GenerateInfo();
                device.DeviceName = "SeaBotAlpha";
                Files.Create(@"device.json");
                Files.WriteInFiles(JsonSerializer.Serialize(device), @"device.json");
                configExisted = true;
                this._deviceInfo = device;
                log.Warning("device.json缺失", _initName);
                log.Info("已生成device.json", _initName);
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
                };
                config.SaveSelf();//save config
                this.Config = config;
                log.Warning("config.json缺失", _initName);
                log.Info("已生成config.json", _initName);
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
                    logger.Info("config.json文件已生成，请修改其中的设置后重新启动", _name);
                    return;
                }
                logger.Info("初始化……", _name);
                var bot = BotFactory.Create(new BotConfig(), _deviceInfo, _keyStore);//create bot
                this._bot = bot;
                logger.Info($"-----登录基本信息-----\n协议版本：{bot.AppInfo.PtVersion}\n客户端版本：{bot.AppInfo.AppClientVersion}\n客户端系统：{bot.AppInfo.Os}", _name);
                logger.Info($"-----宿主机基本信息-----\n系统版本：{Environment.OSVersion}\n.Net版本：{Environment.Version}\n内存：{Environment.WorkingSet}", _name);
                bot.Invoker.OnBotOnlineEvent += EventProcess.BotOnlineCheck;
                bot.Invoker.OnBotOfflineEvent += EventProcess.BotOfflineCheck;
                bot.Invoker.OnBotCaptchaEvent += EventProcess.BotCaptchaCheck;
                if (FirstLogin || Config.UseQrCodeInsteadOfPassword)
                {
                    logger.Info($"由于{(FirstLogin ? "这是第一次启动" : "")}{(Config.UseQrCodeInsteadOfPassword ? "你的偏好设置" : "")}，将使用扫码登录", _name);
                    var qc = await bot.FetchQrCode();//get qrcode
                    logger.Info("请使用手机QQ扫描即将弹出的二维码（可能需要一点时间），如果没能成功打开请在SeaBot根目录下找到qrcode.png并打开进行扫描", _name);
                    if (qc != null)
                    {
                        var (uri, qrCodeByte) = qc.Value;
                        using FileStream fs = File.Create(@"qrCode.png");
                        await fs.WriteAsync(qrCodeByte, new CancellationToken());
                        Process.Start(new ProcessStartInfo("qrCode.png")
                        {
                            UseShellExecute = true
                        });//use app to open qrcode
                        logger.Info("等待登录完成……", _name);
                    }
                    await bot.LoginByQrCode();//wait qrcode login
                    
                }
                else if (!Config.UseQrCodeInsteadOfPassword)
                {
                    logger.Info("由于你的偏好设置，将使用已保存的Token登录，如不能成功登录，请删除SeaBot根目录下的keystore.json文件再试", _name);
                    bool loginSuccess = await bot.LoginByPassword();//wait password/keystore login
                    if (!loginSuccess)
                    {
                        logger.Info("Token登录不成功。", _name);
                    }
                }
                _keyStore = bot.UpdateKeystore();
                this.Config.QQUin = _keyStore.Uin;
                this.Config.SaveSelf();
                logger.Info("Bot Uin: " + Config.QQUin, _name);
                Files.Create(@"keystore.json");
                Files.WriteInFiles(JsonSerializer.Serialize(_keyStore), @"keystore.json");//save keystore
                logger.Info("登录Token已保存", _name);
                Message.Message.Init();
                bot.Invoker.OnFriendMessageReceived += EventProcess.BotReceiveMessage;
                bot.Invoker.OnGroupMessageReceived += EventProcess.BotReceiveMessage;
                logger.Info("如果你未看到“登录成功”，请参照上述步骤进行；如果你是扫码登录，那么大概率是Code45，请联系开发者", _name);
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
                logger.Info("消息已申请发送", "Message.Send");
                Message.Message.LastResult = await _bot.SendMessage(message);
                if (message.GroupUin != null)
                {
                    logger.Info($"已向群{old.GroupUin}发送消息", "Message.Send");
                }
                else if (message.GroupUin == null)
                {
                    logger.Info($"已向好友{old.FriendUin}发送消息", "Message.Send");
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
                logger.Info("消息已申请发送", "Message.Send");
                Message.Message.LastResult = await _bot.SendMessage(message);
                if (message.GroupUin != null)
                {
                    logger.Info($"已向群{message.GroupUin}发送消息", "Message.Send");
                }
                else if (message.GroupUin == null)
                {
                    logger.Info($"已向群{message.FriendUin}发送消息", "Message.Send");
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
