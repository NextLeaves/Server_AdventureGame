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
        public byte[] Data { get; set; }
        public TempData Tempdata { get; set; } = new TempData();

        public Player(string account, Connection conn)
        {
            Account = account;
            this.Conn = conn;
        }

        public void Send(ProtocolBase protocol)
        {
            if (Conn == null) return;
            Server.GetUniqueServer().Send(Conn, protocol);
        }

        public void KickOff(Connection conn)
        {
            ProtocolByte protocol = new ProtocolByte();
            protocol.AddInfo<string>(Logic.NamesOfProtocol.Kickoff);
            conn.Send(protocol);
            this.Logout();
        }

        public bool Logout()
        {
            //事件处理
            Server._instance._playerEventHandle.OnLogout(this);

            Trace.WriteLine($"[Logout]{Conn.RemoteAddress}|{this.Account}:logout processed.");
            Sys.sb_Log.Append($"[Logout]{Conn.RemoteAddress}|{this.Account}:logout processed.");
            Conn.Player = null;
            Conn.Close();
            return true;
        }

    }
}
