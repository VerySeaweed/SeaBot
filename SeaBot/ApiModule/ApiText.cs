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

        }
    }
}
