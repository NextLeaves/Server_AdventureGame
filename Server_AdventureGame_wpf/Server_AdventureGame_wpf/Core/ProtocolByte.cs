using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Server_AdventureGame_wpf.Core
{
    public class ProtocolByte : ProtocolBase
    {
        public byte[] Data { get; set; }

        public ProtocolByte()
        {

        }

        private void InitInfomation()
        {
            this.Name = GetString(0);
            if (Data == null) return;
            for (int i = 0; i < Data.Length; i++)
            {
                int b = (int)Data[i];
                Expression += b.ToString() + " ";
            }

        }

        public override ProtocolBase Decode(byte[] bufferRead, int start, int length)
        {
            ProtocolBase protocol = new ProtocolByte();
            (protocol as ProtocolByte).Data = new byte[length];
            Array.Copy(bufferRead, (protocol as ProtocolByte).Data, length);
            //刷新协议
            InitInfomation();
            return protocol;
        }



        public override byte[] Encode()
        {
            return Data;
        }

        public void AddString(string message)
        {
            message += ",";
            int lenMsg = message.Length;
            byte[] lenMsgBytes = BitConverter.GetBytes(lenMsg);
            byte[] MsgBytes = Encoding.Default.GetBytes(message);
            if (Data == null)
                Data = lenMsgBytes.Concat(MsgBytes).ToArray();
            else
                Data = Data.Concat(lenMsgBytes).Concat(MsgBytes).ToArray();

            InitInfomation();
        }

        public string GetString(int indexof)
        {
            if (Data == null) return "Error Data In ProtocolByte";
            if (Data.Length < sizeof(Int32)) return "Error Data In ProtocolByte";
            int lenMsg = BitConverter.ToInt32(Data, 0);
            if (Data.Length < sizeof(Int32) + lenMsg) return "Error Data In ProtocolByte";

            string msgData = Encoding.UTF8.GetString(Data, sizeof(Int32), lenMsg);
            string[] indexs = msgData.Split(',');
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
