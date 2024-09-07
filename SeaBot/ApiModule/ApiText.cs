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
        public DateTime SendTime { get; protected set; }
        
        public string? Action { get; set; }

        public int StatusCode { get; set; }

        public Guid Guid { get; set; }

        public EventArgs? Args { get; set; }

        public EMessageType Type { get; set; }


        public ApiText()
        {
            SendTime = DateTime.Now;
            Guid = Guid.NewGuid();
        }


        public class ApiTextHello : ApiText
        {
            public string Hello { get; set; }

            public ApiTextHello()
            {
                Hello = "Hello";
                StatusCode = (int)EStatusCode.Hello;
            }
        }

        public class ApiTextRequest : ApiText
        {
            public ApiTextRequest()
            {
                StatusCode = (int)EStatusCode.NotResponse;
            }
        }

        public class ApiTextResponse : ApiText
        {
            public ApiTextResponse()
            {
                StatusCode = (int)EStatusCode.Success;
            }
        }
    }
}
