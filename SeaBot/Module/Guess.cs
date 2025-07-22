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

        private string _name = "Guess";


        public Guess()
        {
            this.unique_id = "guess";
        }


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
            bot.SendMessage(message, chain);
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
                else if (sub[1] == "create" || sub[1] == "c")
                {
                    logger.Info($"房间创建请求{chain.GroupUin}通过", _name);
                    _games.Add(new Game(Convert.ToUInt32(chain.GroupUin)));
                    message.Text($"已经创建房间{chain.GroupUin}");
                }
                else if (sub[1] == "reload")
                {
                    logger.Info("调用重载命令", _name);
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
                                logger.Info($"房间{game.RoomNumber}启动游戏", _name);
                                game.GameStart(message);
                                break;
                            case "stop":
                                logger.Info($"房间{game.RoomNumber}结束游戏", _name);
                                game.GameStop(message);
                                _games.Remove(game);
                                break;
                            case "setrange":
                                logger.Info($"房间{game.RoomNumber}设置范围", _name);
                                game.SetRange(sub[2], message);
                                break;
                            case "setcount":
                                logger.Info($"房间{game.RoomNumber}设置数目", _name);
                                game.SetCount(Convert.ToUInt32(sub[2]), message);
                                break;
                            case "join":
                                logger.Info($"玩家{chain.FriendUin}加入{game.RoomNumber}", _name);
                                game.PlayerJoin(chain.FriendUin, message);
                                break;
                            case "leave":
                                logger.Info($"玩家{chain.FriendUin}退出{game.RoomNumber}", _name);
                                game.PlayerLeave(chain.FriendUin, message);
                                break;
                            case "open":
                            case "o":
                                char[] chars = sub[2].ToCharArray();
                                game.Open(chars[0], chain, message);
                                break;
                            case "guess":
                            case "g":
                                string temp_g = "";
                                for (int i = 2; i < sub.Length; i++)
                                {
                                    temp_g += $"{sub[i]} ";
                                    if (i == sub.Length - 1)
                                    {
                                        temp_g = temp_g.Trim();
                                    }
                                }
                                game.Guess(temp_g, chain, message);
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
            message.Text(".g .guess均可调用本模块\n.g create -字面意思，每个群有且只能有1个活动状态的房间\n.g setrange [命名空间，用;分隔] -设置开字母曲库范围，请确认命名空间是否正确");
            message.Text("\n.g start -字面意思，仅在范围设置后可用\n.g stop -字面意思，同时关闭房间\n.g o/open [要开的字母] -开字母\n.g g/guess [曲目全名/部分别名] -别名仍在搜集，不正确是正常情况");
            message.Text("\n.g join -加入本群开字母房间\n.g leave -退出房间\n.g setcount [曲目数，应为纯数字] -设置开字母曲目数");
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

            private bool[] opened;

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
                RandomSongs();
                scores = new int[_membersUin.Count];
                opened = new bool[_chosen.Count];
                for (int i = 0; i < scores.Length; i++)
                {
                    scores[i] = 0;
                    opened[i] = false;
                }
                for (int i = 0; i < opened.Length; i++)
                {
                    opened[i] = false;
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
                message.Text("游戏结束");
            }

            public void Open(char c, MessageChain chain, MessageBuilder message)
            {
                if (!StartStatus)
                {
                    message.Text("游戏未启动");
                    return;
                }
                else if (chain.FriendUin != _membersUin[(int)turns])
                {
                    message.Text("没到你的回合");
                    message.Mention(chain.FriendUin);
                    return;
                }
                _opened_characters.Add(c);
                bool scr = false;
                uint sc = 0;
                for (int i = 0; i < a_origin.Count; i++)
                {
                    char[] chars_o = a_origin[i].ToCharArray();
                    for (int j = 0; j < chars_o.Length; j++)
                    {
                        if (chars_o[j] == c)
                        {
                            char[] chars_g = a_group[i].ToCharArray();
                            chars_g[j] = chars_o[j];
                            a_group[i] = new string(chars_g);
                            scr = true;
                            sc += 1;
                        }
                        else if (chars_o[j].ToString().ToLower() == c.ToString())
                        {
                            char[] chars_g = a_group[i].ToCharArray();
                            chars_g[j] = chars_o[j];
                            a_group[i] = new string(chars_g);
                            scr = true;
                            sc += 1;
                        }
                        else if (chars_o[j].ToString().ToUpper() == c.ToString())
                        {
                            char[] chars_g = a_group[i].ToCharArray();
                            chars_g[j] = chars_o[j];
                            a_group[i] = new string(chars_g);
                            scr = true;
                            sc += 1;
                        }
                    }
                }
                if (scr)
                {
                    if (sc > 5)
                    {
                        scores[turns] += 1;
                    }
                    scores[turns] += 1;
                }
                message.Text($"开字母{c} {_membersNick[(int)turns]}+{((sc > 5) ? 2 : (scr ? 1 : 0))}分\n");
                turns++;
                if (turns >= scores.Length)
                    turns = 0;
                if (!CheckWin(message))
                    PrintList(message);
            }

            public void Guess(string s, MessageChain chain, MessageBuilder message)
            {
                if (!StartStatus)
                {
                    message.Text("游戏未启动");
                    return;
                }
                else if (chain.FriendUin != _membersUin[(int)turns])
                {
                    message.Text("没到你的回合");
                    message.Mention(chain.FriendUin);
                    return;
                }
                bool right = false;
                for (int i = 0; i < _chosen.Count; i++)
                {
                    if (s == _chosen[i].Name)
                    {
                        right = true;
                        a_group[i] = a_origin[i];
                        opened[i] = true;
                        break;
                    }
                    else if (s == _chosen[i].Name.ToLower())
                    {
                        right = true;
                        a_group[i] = a_origin[i];
                        opened[i] = true;
                        break;
                    }
                    else if (s == _chosen[i].Name.ToUpper())
                    {
                        right = true;
                        a_group[i] = a_origin[i];
                        opened[i] = true;
                        break;
                    }
                    else if (_chosen[i].Alias != null)
                    {
                        for (int j = 0; j < _chosen[i].Alias.Length; j++)
                        {
                            if (s == _chosen[i].Alias[j])
                            {
                                right = true;
                                a_group[i] = a_origin[i];
                                opened[i] = true;
                                break;
                            }
                            else if (s == _chosen[i].Alias[j].ToLower())
                            {
                                right = true;
                                a_group[i] = a_origin[i];
                                opened[i] = true;
                                break;
                            }
                            else if (s == _chosen[i].Alias[j].ToUpper())
                            {
                                right = true;
                                a_group[i] = a_origin[i];
                                opened[i] = true;
                                break;
                            }
                        }
                    }
                }
                if (right)
                    scores[turns] += 5;
                message.Text($"{s} {(right ? " " : "不")}正确 {_membersNick[(int)turns]}+{(right ? 5 : 0)}分\n");
                turns++;
                if (turns >= scores.Length)
                    turns = 0;
                if (!CheckWin(message))
                    PrintList(message);
            }

            protected bool CheckWin(MessageBuilder message)
            {
                bool end = true;
                for (int i = 0; i < opened.Length; i++)
                {
                    if (!opened[i])
                    {
                        end = false;
                    }
                }
                if (end)
                {
                    string s = "\n";
                    for (int i = 0; i < scores.Length; i++)
                    {
                        s += $"{_membersNick[i]}：{scores[i]}\n";
                    }
                    message.Text($"游戏结束，所有字母都开出来啦~真厉害~\n\n玩家得分：{s}\n\n");
                    this.GameStop(message);
                    _games.Remove(this);
                }
                return end;
            }

            public void PrintList(MessageBuilder message)
            {
                string open = "";
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
                if (StartStatus)
                {
                    message.Text("游戏运行中不可修改");
                    return;
                }
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
                if (StartStatus)
                {
                    message.Text("游戏运行中不可修改");
                    return;
                }
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
                    return;
                }
                GuessCount = count;
                message.Text("已设置曲目数量");
            }

            protected void RandomSongs()
            {
                a_group.Clear();
                a_origin.Clear();
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
                var members = await Message.Message.bot.FetchGroupMembers(this.RoomNumber);
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
                var members = await Message.Message.bot.FetchGroupMembers(this.RoomNumber);
                foreach (var member in members)
                {
                    if (member.Uin == FriendUin)
                    {
                        _membersNick.Remove(member.MemberName);
                    }
                }
                message.Text("再见……");
                if (_membersUin.Count < 1 && StartStatus)
                {
                    this.GameStop(message);
                    _games.Remove(this);
                }
            }
        }
    }
}
