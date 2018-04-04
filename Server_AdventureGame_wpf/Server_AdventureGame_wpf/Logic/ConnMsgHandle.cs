using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server_AdventureGame_wpf.Core;
using Server_AdventureGame_wpf.Data;

namespace Server_AdventureGame_wpf.Logic
{
    public partial class ConnMsgHandle
    {
        public void MsgHeartBeat(Connection conn, ProtocolBase protocol)
        {
            conn.LastTickTime = Sys.GetTimeStamp();
            Console.WriteLine($"[Heartbeat] conn:{conn.RemoteAddress}.");
        }

        public void MsgRegister(Connection conn, ProtocolBase protocol)
        {
            ProtocolByte proto = protocol as ProtocolByte;
            string protoName = proto.Name;
            string id = proto.GetString(1);
            string pw = proto.GetString(2);
            int code = int.MinValue;

            //准备返回协议对象
            ProtocolByte protoRet = new ProtocolByte();
            protoRet.AddInfo<string>(NamesOfProtocol.Register);

            //从数据库判断
            bool isChecked = DataManager.GetSingleton().CanRegister(id);
            if (isChecked)
            {
                DataManager.GetSingleton().Register(id, pw, out code);
                Console.WriteLine($"[Register] User:{id},Info:{conn.RemoteAddress}.");
                protoRet.AddInfo<int>(1);
                protoRet.AddInfo<int>(code);
                conn.Send(protoRet);
                return;
            }
            else
            {
                protoRet.AddInfo<int>(-1);
                conn.Send(protoRet);
                return;
            }
        }

        public void MsgLogin(Connection conn, ProtocolBase protocol)
        {
            ProtocolByte proto = protocol as ProtocolByte;
            string id = proto.GetString(1);
            string pw = proto.GetString(2);

            //准备返回协议对象
            ProtocolByte protoRet = new ProtocolByte();
            protoRet.AddInfo<string>(NamesOfProtocol.Login);

            //从数据库判断
            bool isChecked = DataManager.GetSingleton().CheckPassword(id, pw);

            if (isChecked)
            {
                Middle.Player playerMiddle = new Middle.Player(id, conn);
                conn.Player = playerMiddle;

                Console.WriteLine($"[Conected] User:{id},Info:{conn.RemoteAddress}.");
                protoRet.AddInfo<int>(1);
                conn.Send(protoRet);
                return;
            }
            else
            {
                protoRet.AddInfo<int>(0);
                conn.Send(protoRet);
                return;
            }
        }

        public void MsgReceivePlayerData(Connection conn, ProtocolBase protocol)
        {
            ProtocolByte proto = protocol as ProtocolByte;
            string protoName = proto.Name;
            string id = proto.GetString(1);

            //准备返回协议对象
            ProtocolByte protoRet = new ProtocolByte();
            protoRet.AddInfo<string>(NamesOfProtocol.ReceivePlayerData);

            //从数据库判断           
            byte[] data = DataManager.GetSingleton().GetPlayerData(id);
            if (data != null)
            {
                conn.Player.Data = data;

                string tempS = Encoding.Default.GetString(data);
                Console.WriteLine($"[ReceivePlayerData] User:{id},Info:{conn.RemoteAddress}.");
                protoRet.AddInfo<string>(tempS);
                conn.Send(protoRet);
            }
            else
            {
                protoRet.AddInfo<int>(-1);
                conn.Send(protoRet);
            }
        }

        public void MsgLogout(Connection conn, ProtocolBase protocol)
        {
            ProtocolByte proto = new ProtocolByte();
            proto.AddInfo<string>(NamesOfProtocol.Logout);
            proto.AddInfo<int>(1);
            if (conn.Player == null)
            {
                conn.Send(proto);
                conn.Close();
            }
            else
            {
                conn.Send(proto);
                conn.Player.Logout();
            }
        }

        public void MsgFindPassword(Connection conn, ProtocolBase protocol)
        {
            ProtocolByte proto = protocol as ProtocolByte;
            string protoName = proto.Name;
            string id = proto.GetString(1);
            string code_str = proto.GetString(2);
            int code_i = Convert.ToInt32(code_str);

            //准备返回协议对象
            ProtocolByte protoRet = new ProtocolByte();
            protoRet.AddInfo<string>(NamesOfProtocol.FindPassword);

            //从数据库判断           
            bool isChecked = DataManager.GetSingleton().FindoutPassword(id, code_i);
            if (isChecked)
            {
                Console.WriteLine($"[FindPassword] User:{id},Info:{conn.RemoteAddress}.");
                protoRet.AddInfo<int>(1);
                conn.Send(protoRet);
                return;
            }
            else
            {
                protoRet.AddInfo<int>(-1);
                conn.Send(protoRet);
                return;
            }
        }

        public void MsgChangePassword(Connection conn, ProtocolBase protocol)
        {
            ProtocolByte proto = protocol as ProtocolByte;
            string protoName = proto.Name;
            string id = proto.GetString(1);
            string password = proto.GetString(2);

            //准备返回协议对象
            ProtocolByte protoRet = new ProtocolByte();
            protoRet.AddInfo<string>(NamesOfProtocol.ChangePassword);

            //从数据库判断           
            bool isChecked = DataManager.GetSingleton().ChangePassword(id, password);
            if (isChecked)
            {
                Console.WriteLine($"[ChangePassword] User:{id},Info:{conn.RemoteAddress}.");
                protoRet.AddInfo<int>(1);
                conn.Send(protoRet);
                return;
            }
            else
            {
                protoRet.AddInfo<int>(-1);
                conn.Send(protoRet);
                return;
            }
        }

        public void MsgSendPlayerData(Connection conn, ProtocolBase protocol)
        {
            ProtocolByte proto = protocol as ProtocolByte;
            string protoName = proto.Name;
            string id = proto.GetString(1);
            byte[] data = Encoding.Default.GetBytes(proto.GetString(2));

            //准备返回协议对象
            ProtocolByte protoRet = new ProtocolByte();
            protoRet.AddInfo<string>(NamesOfProtocol.SendPlayerData);

            //从数据库判断           
            bool isChecked = DataManager.GetSingleton().SavePlayer(id, data);
            if (isChecked)
            {
                Console.WriteLine($"[SendPlayerData] User:{id},Info:{conn.RemoteAddress}.");
                protoRet.AddInfo<int>(1);
                conn.Send(protoRet);
                return;
            }
            else
            {
                protoRet.AddInfo<int>(-1);
                conn.Send(protoRet);
                return;
            }
        }
        
    }
}
