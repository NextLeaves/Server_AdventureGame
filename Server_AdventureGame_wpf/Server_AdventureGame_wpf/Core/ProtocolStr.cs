using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_AdventureGame_wpf.Core
{
    public class ProtocolStr : ProtocolBase
    {
        public string Message { get; set; }

        public override ProtocolBase Decode(byte[] bufferRead, int start, int length)
        {
            ProtocolBase protocol = new ProtocolStr();
            (protocol as ProtocolStr).Message = Encoding.UTF8.GetString(bufferRead, start, length);

            if (Message.Length == 0) protocol.Name = "Test";
            protocol.Name = Message.Split(',')[0];

            if (Message.Length == 0) protocol.Expression = "No Expression";
            protocol.Expression = Message;

            return protocol;
        }

        public override byte[] Encode()
        {
            if (Message == null || Message.Length <= 0)
            {
                return Encoding.Default.GetBytes("Error");
            }
            return Encoding.Default.GetBytes(Message);
        }
    }
}
