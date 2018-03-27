using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using Server_AdventureGame_wpf.Core;
using System.ComponentModel.DataAnnotations;
using Server_AdventureGame_wpf.Data;

namespace Server_AdventureGame_wpf.Middle
{
    public class Player
    {
        public string Account { get; set; }
        public Connection Conn { get; set; }
        public PlayerData Data { get; set; }
        public PlayerTempData TempData { get; set; }

        public Player(string account, Connection conn)
        {
            Account = account;
            this.Conn = conn;
            TempData = new PlayerTempData();
        }

        public void Send(ProtocolBase protocol)
        {
            if (Conn == null) return;
            Server.GetUniqueServer().Send(Conn, protocol);
        }

        public static bool KickOff(string account, ProtocolBase protocol)
        {
            foreach (Connection conn in Server.GetUniqueServer().conns)
            {
                if (conn == null) continue;
                if (!conn.IsUse) continue;
                if (conn.Player == null) continue;
                if (conn.Player.Account == account)
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
            Server._instance._playerEventHandle.OnLogout(this);

            //保存数据
            bool isChecked = DataManager.GetSingleton().SavePlayerData(this);
            if (!isChecked)
            {
                Debug.WriteLine($"[Error]{Conn.RemoteAddress}|{this.Account}:Save PlayerData is error");
                return false;
            }

            Console.WriteLine($"[Logout]{Conn.RemoteAddress}|{this.Account}:logout processed.");
            Conn.Player = null;
            Conn.Close();
            return true;
        }

    }
}
