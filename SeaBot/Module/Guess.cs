using Lagrange.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Lagrange.Core.Common.Interface.Api;

namespace SeaBot.Module
{
    internal class Guess : ModuleBase
    {
        public static string _songBasePath = @"guess_song_base";

        public static List<string> _songBasesPath = new();

        public static List<Game> _games = new();


        protected void ExistFile()
        {
            if (!Directory.Exists(_songBasePath))
            {
                Directory.CreateDirectory(_songBasePath);
            }
            else
            {
                string[] paths = Directory.GetFiles(_songBasePath, "*.json");
                foreach (var item in paths)
                {
                    _songBasesPath.Add(item);
                }
            }
        }

        protected override void Friend(string command, MessageChain chain)
        {
            var message = MessageBuilder.Friend(chain.FriendUin);
            message.Text("不支持在私聊中调用Guess模块");
            Message.Message.SendMessage(message, chain);
        }

        protected override MessageBuilder Process(string command, MessageChain chain, MessageBuilder message)
        {
            try
            {
                Logger logger = new();
                string[] sub = command.Split(' ');
                ExistFile();
                message.Forward(chain);
                if (sub.Length < 2 || sub[1] == "help")
                {
                    Help(chain, message);
                }
                else if (sub[1] == "create")
                {
                    _games.Add(new Game(Convert.ToUInt32(chain.GroupUin)));
                    message.Text($"已经创建房间{chain.GroupUin}");
                }
                else if (sub[1] == "reload")
                {
                    _games.Clear();
                    message.Text("成功重载了Guess模块");
                }
                else
                {
                    var game = _games.Find(x => x.RoomNumber == Convert.ToUInt32(chain.GroupUin));
                    if (game != null)
                    {
                        switch (sub[1])
                        {
                            case "start":
                                game.GameStart(message);
                                break;
                            case "stop":
                                game.GameStop(message);
                                _games.Remove(game);
                                break;
                            case "range":
                                game.SetRange(sub[2], message);
                                break;
                            case "count":
                                game.SetCount(Convert.ToUInt32(sub[2]), message);
                                break;
                            case "join":
                                game.PlayerJoin(chain.FriendUin, message);
                                break;
                            case "leave":
                                game.PlayerLeave(chain.FriendUin, message);
                                break;
                            default:
                                message.Text("命令无效");
                                break;
                        }
                    }
                    else
                    {
                        message.Text("无法找到房间");
                    }
                }
            }
            catch (Exception e)
            {
                Logger logger = new();
                logger.Error($"Error: {e.Message}", "Guess");
                message.Text("发生了一个错误");
            }
            return message;
        }

        protected override MessageBuilder Help(MessageChain chain, MessageBuilder message)
        {
            message.Text(".g .guess均可调用本模块\n.g create -字面意思，每个群有且只能有1个活动状态的房间\n.g range [命名空间，用;分隔] -设置开字母曲库范围，请确认命名空间是否正确");
            message.Text("\n.g start -字面意思，仅在范围设置后可用\n.g stop -字面意思，同时关闭房间\n.g o/open [要开的字母] -开字母\n.g g/guess [序号] [曲目全名/部分别名] -别名仍在搜集，不正确是正常情况");
            message.Text("\n.g join -加入本群开字母房间\n.g leave -退出房间\n.g count [曲目数，应为纯数字] -设置开字母曲目数");
            return message;
        }


        public class SongBase
        {
            public string Name { get; set; }
            public List<Song> Songs { get; set; } = new();
            public SongBase()
            {
                this.Name = string.Empty;
            }
        }

        public class Song
        {
            public string Name { get; set; }
            public string Composer { get; set; }
            public string[]? Alias { get; set; }
            public Song()
            {
                this.Name = string.Empty;
                this.Composer = string.Empty;
            }
        }

        public class Game
        {
            public uint RoomNumber;

            private readonly List<uint> _membersUin = new();

            private readonly List<string> _membersNick = new();

            private readonly List<SongBase> _songBases = new();

            private readonly List<char> _opened_characters = new();

            private readonly List<Song> _chosen = new();

            private readonly List<string> a_group = new();

            //private readonly List<string> b_group = new();

            private readonly List<string> a_origin = new();

            //private readonly List<string> b_origin = new();

            private int[] scores;

            private uint turns;

            public uint GuessCount = 5;

            public bool StartStatus = false;

            public Game(uint roomNumber)
            {
                this.RoomNumber = roomNumber;
            }

            public void GameStart(MessageBuilder message)
            {
                if (_songBases.Count < 1)
                {
                    message.Text("无法启动游戏，因为范围未设置");
                    return;
                }
                else if (StartStatus)
                {
                    message.Text("游戏已在运行中，不能重复开始");
                    return;
                }
                else if (_membersUin.Count < 1)
                {
                    message.Text("报名人数小于1，无法启动");
                    return;
                }
                StartStatus = true;
                scores = new int[_membersUin.Count];
                for (int i = 0; i < scores.Length; i++)
                {
                    scores[i] = 0;
                }
                turns = 0;
                PrintList(message);
            }

            public void GameStop(MessageBuilder message)
            {
                if(!StartStatus)
                {
                    message.Text("游戏未在运行，不能停止");
                    return;
                }
                StartStatus = false;
            }

            public void PrintList(MessageBuilder message)
            {
                string open = "\n";
                foreach (var item in _opened_characters)
                {
                    open += $"\'{item}\',";
                }
                string a = "\n";
                for (int i = 0; i < a_group.Count; i++)
                {
                    a += $"a{i + 1} {a_group[i]}\n";
                }
                string s = "\n";
                for (int i = 0; i < scores.Length; i++)
                {
                    s += $"{_membersNick[i]}：{scores[i]}\n";
                }
                message.Mention(_membersUin[(int)turns]);
                message.Text($" 该你了\n已开字符：{open}\nA组：{a}\n\n玩家得分：{s}");
            }

            public void SetRange(string range, MessageBuilder message)
            {
                string[] strings = range.Split(';');
                foreach (var item in strings)
                {
                    if (item != null && File.Exists(Path.Combine(_songBasePath, item + ".json")))
                    {
                        message.Text($"已加载曲库：{item}\n");
                        _songBases.Add(JsonSerializer.Deserialize<SongBase>(Files.ReadInFiles(Path.Combine(_songBasePath, item + ".json"))));
                    }
                    else
                    {
                        message.Text($"无法加载{item}对应的命名空间\n");
                    }
                }
            }

            public void SetCount(uint count, MessageBuilder message)
            {
                int mount_songbase = 0;
                foreach (var item in _songBases)
                {
                    mount_songbase += item.Songs.Count;
                }
                if (count >= 12)
                {
                    message.Text("数量过大，不能大于12");
                    return;
                }
                else if (count < 2)
                {
                    message.Text("数量过小，不能小于2");
                    return;
                }
                else if (count > mount_songbase)
                {
                    message.Text("数量超出了已加载的曲库的曲目总量");
                    SetCount((uint)mount_songbase, message);
                    return;
                }
                else if (mount_songbase < 1)
                {
                    message.Text("曲目过少，设置失败");
                }
                GuessCount = count;
                message.Text("已经设置曲目数量");
                RandomSongs();
            }

            protected void RandomSongs()
            {
                Random r = new();
                List<List<int>> chosen = new();
                for (int i = 0; i < _songBases.Count; i++)
                {
                    chosen.Add(new List<int>());
                }
                for (int i = 0; i < GuessCount; i++)
                {
                    int index_temp = r.Next(_songBases.Count);
                    int index_song;
                    while (true)
                    {
                        index_song = r.Next(_songBases[index_temp].Songs.Count);
                        if (!chosen[index_temp].Contains(index_song))
                        {
                            break;
                        }
                    }
                    _chosen.Add(_songBases[index_temp].Songs[index_song]);
                    chosen[index_temp].Add(index_song);
                }
                foreach (var item in _chosen)
                {
                    a_origin.Add(item.Name);
                    a_group.Add(item.Name);
                }
                for (int i = 0; i < a_group.Count; i++)
                {
                    char[] temps = a_group[i].ToCharArray();
                    for (int j = 0; j < temps.Length; j++)
                    {
                        if (temps[j] != ' ')
                        {
                            temps[j] = '*';
                        }
                    }
                    a_group[i] = new string(temps);
                }
            }

            public async void PlayerJoin(uint FriendUin,MessageBuilder message)
            {
                if (StartStatus)
                {
                    message.Text("游戏开始后不能加入游戏");
                    return;
                }
                else if (_membersUin.Contains(FriendUin))
                {
                    message.Text("不能重复加入游戏");
                    return;
                }
                _membersUin.Add(FriendUin);
                var members = await Program.Bot._bot.FetchMembers(this.RoomNumber);
                foreach (var member in members)
                {
                    if (member.Uin == FriendUin)
                    {
                        _membersNick.Add(member.MemberName);
                    }
                }
                message.Text("加入游戏~");
            }

            public async void PlayerLeave(uint FriendUin,MessageBuilder message)
            {
                if (!_membersUin.Contains(FriendUin))
                {
                    message.Text("不能重复退出游戏");
                    return;
                }
                _membersUin.Remove(FriendUin);
                var members = await Program.Bot._bot.FetchMembers(this.RoomNumber);
                foreach (var member in members)
                {
                    if (member.Uin == FriendUin)
                    {
                        _membersNick.Remove(member.MemberName);
                    }
                }
                message.Text("再见……");
            }
        }
    }
}
