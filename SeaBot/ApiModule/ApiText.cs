using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBot.ApiModule
{
    public enum EStatusCode
    {
        Success=0,
        Hello,
        Processing,
        Failed,
        NotResponse=-1,
    }
    public enum EMessageType
    {
        Hello=0,
        Request,
        Response,
        Event,
    }
    
    public class ApiText
    {
        public string? AccessCode { get; set; }
        
        public DateTime SendTime { get; private set; }
        
        public string? Action { get; set; }

        public EStatusCode StatusCode { get; set; }

        public Guid Guid { get; set; }

        public object? Data { get; set; }

        public EMessageType Type { get; set; }


        public ApiText()
        {
            SendTime = DateTime.Now;
            Guid = Guid.NewGuid();
        }


        public class Hello : ApiText
        {
            public Hello()
            {
                Data = "Hello";
                StatusCode = EStatusCode.Hello;
            }
        }

        public class Request : ApiText
        {
            public Request()
            {
                StatusCode = EStatusCode.NotResponse;
            }
        }

        public class Response : ApiText
        {
            public Response()
            {
                StatusCode = EStatusCode.Success;
            }
        }

        public class Message
        {
            public List<object> messageEntity { get; set; } = new List<object>();

            public uint? GroupUin { get; set; }

            public uint FriendUin { get; set; }


            public void Text(string text)
            {
                messageEntity.Add(new TextEntity(text));
            }

            public void Image(string imagePath)
            {
                messageEntity.Add(new ImageEntity(imagePath));
            }

            public void Forward(uint sequence)
            {
                messageEntity.Add(new ForwardEntity(sequence));
            }

            public void Mention(uint targetUin)
            {
                messageEntity.Add(new MentionEntity(targetUin));
            }


            public class TextEntity
            {
                public string Text { get; set; }
                public TextEntity(string text)
                {
                    this.Text = text;
                }
            }

            public class ImageEntity
            {
                public string ImagePath { get; set; }
                public ImageEntity(string imagePath)
                {
                    this.ImagePath = imagePath;
                }
            }

            public class ForwardEntity
            {
                public uint Sequence { get; set; }
                public ForwardEntity(uint sequence)
                {
                    this.Sequence = sequence;
                }
            }

            public class MentionEntity
            {
                public uint TargetUin { get; set; }
                public MentionEntity(uint targetUin)
                {
                    this.TargetUin = targetUin;
                }
            }
        }
    }
}
