using Lagrange.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBot.Module
{
    internal class YouCarMa : ModuleBase
    {
        protected List<Car> Cars;

        protected class Car
        {
            public string? CarPlate { get; set; }
            public string? Description { get; set; }
            public bool? IsPublic { get; set; }
        }

        public YouCarMa()
        {
            Cars = new List<Car>();
        }

        protected override MessageBuilder Process(string command, MessageChain chain, MessageBuilder message)
        {
            string[] temps = command.Split(' ');
            if (temps.Length < 2)
            {
                if (Cars.Count == 0)
                    message.Forward(chain).Text("myc");
                else
                {
                    message.Forward(chain).Text("有这些车牌哦：");
                    int i = 0;
                    foreach (var item in Cars)
                    {
                        message.Text("\n" + item.CarPlate + " Public:" + item.IsPublic + " " + item.Description);
                        i++;
                        if (i > 4)
                            message.Text("车牌太多惹，只显示5条哦");
                        break;
                    }
                }
            }
            else
            {
                try
                {
                    if (temps[1] == "clear")
                    {
                        Cars = new List<Car>();
                        message.Text("已清空");
                    }
                    else if (temps[1] == "help")
                    {
                        message = Help(chain, message);
                    }
                    else if (temps[1] == "add")
                    {
                        Cars.Add(new Car()
                        {
                            CarPlate = temps[2],
                            Description = temps[4],
                            IsPublic = Convert.ToBoolean(temps[3])
                        });
                        message.Text("已添加指定车牌：" + temps[2]);
                    }
                    else if (temps[1] == "remove")
                    {
                        var item = Cars.Find(x => x.CarPlate == temps[2]);
                        if (item != null)
                        {
                            Cars.RemoveAt(Cars.IndexOf(item));
                        }
                        message.Text("已删除指定车牌：" + temps[2]);
                    }
                    else
                    {
                        message.Text("输入存在问题，请重试");
                    }
                }
                catch
                {
                    message.Text("输入存在问题，请重试");
                }
            }
            return message;
        }

        protected override MessageBuilder Help(MessageChain chain, MessageBuilder message)
        {
            message.Forward(chain);
            message.Text("ycm -主命令，返回车牌列表或myc（没有车）").Text("\nycm clear -清理车牌列表").Text("\nycm add [string:车牌号] [bool:是否为公开房间] [string:描述] -添加车牌");
            message.Text("\nycm remove [string:车牌] -删除对应车牌");
            return message;
        }
    }
}
