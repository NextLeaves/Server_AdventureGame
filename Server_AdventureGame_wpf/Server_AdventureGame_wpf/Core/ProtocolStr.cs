using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server_AdventureGame_wpf.Core
{

    public class ProtocolStr : ProtocolBase
    {
        private string _name;
        private string _expression;

        public string Data { get; set; }
        public override string Name { get => _name; set => _name = value; }
        public override string Expression { get => _expression; set => _expression = value; }

        public ProtocolStr()
        {
            _name = "";
            _expression = "";
        }

        private void InitInfomation()
        {
            _name = GetString(0);            
            _expression = Data;
        }

        public override ProtocolBase Decode(byte[] bufferRead, int start, int length)
        {
            ProtocolStr proto = new ProtocolStr();
            byte[] tempBuffer = new byte[length - sizeof(Int32)];
            Array.Copy(bufferRead, sizeof(Int32), tempBuffer, 0, tempBuffer.Length);
            proto.Data = Encoding.UTF8.GetString(tempBuffer);
            //刷新协议
            InitInfomation();
            return proto;
        }

        public override byte[] Encode()
        {
            int lenMsg = Data.Length;
            byte[] lenMsgBytes = BitConverter.GetBytes(lenMsg);
            byte[] msgBytes = Encoding.Default.GetBytes(Data);
            byte[] encodingMsg = lenMsgBytes.Concat(msgBytes).ToArray();
            return encodingMsg;
        }

        public void AddInfo<T>(T message)
        {
            Expression += message.ToString() + " ";
        }

        public string GetString(int indexof)
        {            
            string[] indexs = Data.Split(' ');
            if (indexof >= 0 && indexof < indexs.Length)
                return indexs[indexof];            
            else
                throw new IndexOutOfRangeException("indexof must be between 0 and indexs's length.");
        }
        
        /*   感觉存在问题，几乎不怎么用到，以字符串为主要解析方式
        public int GetInt(int start, ref int end)
        {
            if (Data == null) return 0;
            if (Data.Length < start + sizeof(int)) return 0;
            end = start + sizeof(int);
            return BitConverter.ToInt32(Data, start);
        }

        public int GetInt(int start)
        {
            int end = 0;
            return GetInt(start, ref end);
        }
        */

    }

}
