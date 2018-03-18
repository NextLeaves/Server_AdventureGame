using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_AdventureGame_wpf.Core
{
    public class ProtocolByte : ProtocolBase
    {
        public byte[] Message { get; set; }

        public override ProtocolBase Decode(byte[] bufferRead, int start, int length)
        {
            ProtocolBase protocol = new ProtocolByte();
            (protocol as ProtocolByte).Message = bufferRead;
            if (!(length < start + sizeof(int)))
            {
                StringBuilder sb = new StringBuilder();
                for (int i = start; i < sizeof(int); i++)
                {
                    sb.Append(bufferRead.ToString());
                }
                protocol.Name = sb.ToString();
            }                

            return protocol;
        }

        public override byte[] Encode()
        {
            return Message;
        }

        public void AddString(string message)
        {
            int lenMessage = message.Length;
            byte[] lenBytes = BitConverter.GetBytes(lenMessage);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            if (Message == null)
                Message = lenBytes.Concat(messageBytes).ToArray();
            else
                Message = Message.Concat(lenBytes).Concat(messageBytes).ToArray();
        }

        public string GetString(int start, ref int end)
        {
            if (Message == null) return "Error";
            if (Message.Length < start + sizeof(int)) return "Error";
            int lenMessage = BitConverter.ToInt32(Message, start);
            if (Message.Length < start + sizeof(int) + lenMessage) return "Error";

            string message = Encoding.UTF8.GetString(Message, start + sizeof(int), lenMessage);
            end = start + sizeof(int) + lenMessage;
            return message;
        }

        public string GetString(int start)
        {
            int end = 0;
            return GetString(start, ref end);
        }
    }
}
