using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Server_AdventureGame_wpf.Core;
using Server_AdventureGame_wpf.Data;

namespace Server_AdventureGame_wpf.Logic
{
    public class PlayerMsgHandle
    {
        /// <summary>
        /// 初始化玩家初始位置
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="protocol"></param>
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

            Middle.Vector3 pos = new Middle.Vector3(x_f, y_f, z_f);

            Server._instance.Broadcast(id, pos);
            SendAllPlayerPos(conn);
        }

        /// <summary>
        /// 客户端请求获取数据
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="protocol"></param>
        public void MsgReceivePlayerData(Connection conn, ProtocolBase protocol)
        {
            ProtocolByte proto = protocol as ProtocolByte;
            string id = proto.GetString(1);

            //准备返回协议对象
            ProtocolByte protoRet = new ProtocolByte();
            protoRet.AddInfo<string>(NamesOfProtocol.ReceivePlayerData);

            //Console.WriteLine("从数据库判断");
            //从数据库判断           
            byte[] data = DataManager.GetSingleton().GetPlayerData(id);
            if (data != null)
            {
                Middle.PlayerScore socre = DataManager.GetSingleton().ConvertData(data);
                conn.Player.Data = data;

                Debug.WriteLine($"[ReceivePlayerData] User:{id},Info:{conn.RemoteAddress}.");
                protoRet.AddInfo<string>(id);
                protoRet.AddInfo<int>(socre.Coin);
                protoRet.AddInfo<int>(socre.Money);
                protoRet.AddInfo<int>(socre.Star);
                protoRet.AddInfo<int>(socre.Diamand);
                conn.Send(protoRet);
            }
            else
            {
                //Console.WriteLine("创建数据");
                if (DataManager.GetSingleton().CreatePlayerData(id))
                {
                    Debug.Write($"[InitData]Account:{id}.");
                    MsgReceivePlayerData(conn, protocol);
                }
                else
                {
                    throw new Exception($"[InitData]Fail with {id}.");
                }
            }
        }

        /// <summary>
        /// 转发已存在玩家数据位置
        /// </summary>
        /// <param name="conn"></param>
        private void SendAllPlayerPos(Connection conn)
        {

            foreach (Connection item in Server._instance.conns)
            {
                if (!item.IsUse) continue;
                Middle.Vector3 position = item.Player.Tempdata.Postion;
                if (item == conn) continue;
                ProtocolByte protoRet = new ProtocolByte();
                protoRet.AddInfo<string>(NamesOfProtocol.SendOriginPos);
                protoRet.AddInfo<float>(position.x);
                protoRet.AddInfo<float>(position.y);
                protoRet.AddInfo<float>(position.z);

                Debug.Write($"[Return Position] {item.Player.Account} to {conn.Player.Account}.Positon:({position.x},{position.y},{position.z}).");

                Server._instance.Send(conn, protoRet);
            }




        }

        public void MsgSendPlayerData(Connection conn, ProtocolBase protocol)
        {
            ProtocolByte proto = protocol as ProtocolByte;
            string id = proto.GetString(1);

            //it's not must to return protocol            
            bool isSend = false;
            int coin = Convert.ToInt32(proto.GetString(2));
            int money = Convert.ToInt32(proto.GetString(3));
            int star = Convert.ToInt32(proto.GetString(4));
            int diamand = Convert.ToInt32(proto.GetString(5));

            Middle.PlayerScore score = new Middle.PlayerScore(coin, money, star, diamand);
            isSend = Server._instance.Broadcast(id, score);
            if (isSend)
            {
                Debug.WriteLine($"[Update Data]Account:{id}'s score is refresh.");
                return;
            }
            else
            {
                Debug.WriteLine($"[Update Data Error]Account:{id}'s score is not refresh.");
                return;
            }
        }

        public void MsgUpdatePosition(Connection conn, ProtocolBase protocol)
        {
            Server._instance.Broadcast(protocol);
        }

        public void MsgLogout(Connection conn, ProtocolBase protocol)
        {
            ProtocolByte proto = protocol as ProtocolByte;
            string id = proto.GetString(1);

            foreach (var item in Server._instance.conns)
            {
                if (!item.IsUse) continue;
                if (item.Player == null) continue;
                if (item.Player.Account == id)
                    item.Player.Logout();
            }
        }
    }
}
