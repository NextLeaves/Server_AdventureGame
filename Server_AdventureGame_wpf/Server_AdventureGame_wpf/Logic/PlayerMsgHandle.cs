using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server_AdventureGame_wpf.Core;
using Server_AdventureGame_wpf.Data;

namespace Server_AdventureGame_wpf.Logic
{
    public class PlayerMsgHandle
    {

        public void MsgSendOriginPos(Connection conn, ProtocolBase protocol)
        {
            ProtocolByte proto = protocol as ProtocolByte;            
            string id = proto.GetString(1);
            string x = proto.GetString(2);
            string y = proto.GetString(3);
            string z = proto.GetString(4);
            float x_f = Convert.ToSingle(x);
            float y_f = Convert.ToSingle(y);
            float z_f = Convert.ToSingle(z);

            PlayerPosInfomation playerPos = new PlayerPosInfomation(id, x_f, y_f, z_f);


            //准备返回协议对象
            ProtocolByte protoRet = new ProtocolByte();
            protoRet.AddInfo<string>(NamesOfProtocol.SendOriginPos);

            if (ScenePlayersManager._instance.Add(playerPos))
            {
                SendAllPlayerPos(conn);
                return;
            }
            else
            {
                protoRet.AddInfo<int>(-1);
                conn.Send(protoRet);
                return;
            }
        }

        private void SendAllPlayerPos(Connection conn)
        {

            foreach (var item in ScenePlayersManager._instance.PlayersPos)
            {
                if (item.Account == conn.Player.Account) return;
                ProtocolByte protoRet = new ProtocolByte();
                protoRet.AddInfo<string>(NamesOfProtocol.SendOriginPos);
                protoRet.AddInfo<string>(conn.Player.Account);
                protoRet.AddInfo<float>(item.x);
                protoRet.AddInfo<float>(item.y);
                protoRet.AddInfo<float>(item.z);
                conn.Send(protoRet);
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
