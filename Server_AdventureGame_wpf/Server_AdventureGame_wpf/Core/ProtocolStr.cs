using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server_AdventureGame_wpf.Core
{

    public class ProtocolStr : ProtocolBase
    {
        public string Data { get; set; }

        public ProtocolStr()
        {
            Data = string.Empty;
        }

        private void InitInfomation()
        {
            this.Name = GetString(0);
            if (Data == null) return;
            Expression = Data;

        }

        public override ProtocolBase Decode(byte[] bufferRead, int start, int length)
        {
            ProtocolStr proto = new ProtocolStr();
            byte[] tempBuffer = new byte[length - sizeof(Int32)];
            Array.Copy(bufferRead, sizeof(Int32), tempBuffer, 0, length);
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

        public void AddString(string message)
        {
            if (Data == null) return;
            Data += message + ",";
            InitInfomation();
        }

        public string GetString(int indexof)
        {
            byte[] checkMsgBytes = Encode();
            if (checkMsgBytes == null) return "Error Data In ProtocolByte";
            string[] indexs = Data.Split(',');
            if (indexof >= 0 && indexof < indexs.Length)
                return indexs[indexof];
            else
                throw new IndexOutOfRangeException("indexof must be between 0 and indexs's length.");
        }

        public void AddInt(int num)
        {
            string message = num.ToString();
            AddString(message);
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
