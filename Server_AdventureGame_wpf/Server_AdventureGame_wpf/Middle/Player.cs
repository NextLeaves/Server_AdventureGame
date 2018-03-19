using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server_AdventureGame_wpf.Core;

namespace Server_AdventureGame_wpf.Middle
{
    public class Player
    {
        public string Id { get; set; }
        public Connection Conn { get; set; }
        public PlayerData Data { get; set; }
        public PlayerTempData TempData { get; set; }

        public Player(string id,Connection conn)
        {
            Id = id;
            this.Conn = conn;
            TempData = new PlayerTempData();
        }

        public void Send(ProtocolBase protocol)
        {
            if (Conn == null) return;
            Server.GetUniqueServer().Send(Conn, protocol);
        }

        public static bool KickOff(string id,ProtocolBase protocol)
        {
            foreach (Connection  conn in Server.GetUniqueServer().conns)
            {
                if (conn == null) continue;
                if (!conn.IsUse) continue;
                if (conn.Player == null) continue;
                if (conn.Player.Id == id)
                {
                    lock (conn.Player)
                    {
                        if (protocol != null)
                            conn.Player.Send(protocol);
                        return conn.Player.Logout();
                    }
                }
            }
            return true;
        }

        public bool Logout()
        {
            //事件处理
            //保存数据

            Conn.Player = null;
            Conn.Close();
            return true;
        }

    }
}
