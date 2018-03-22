using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server_AdventureGame_wpf.Core;

namespace Server_AdventureGame_wpf.Logic
{
    public partial class ConnMsgHandle
    {
        public void MsgHeartBeat(Connection conn, ProtocolBase protocol)
        {
            conn.LastTickTime = Sys.GetTimeStamp();
            Console.WriteLine($"[Heartbeat] conn:{conn.RemoteAddress}.");
        }

        public void MsgRegister(Connection conn,ProtocolBase protocol)
        {

        }

        public void MsgLogin(Connection conn,ProtocolBase protocol)
        {            
            ProtocolByte proto = protocol as ProtocolByte;
            string protoName = proto.Name;
            string id = proto.GetString(1);
            string pw = proto.GetString(2);

            ProtocolByte protoRet = new ProtocolByte();
            protoRet.AddString("Login");
            if (id == "123" && pw == "123")
            {
                protoRet.AddInt(1);
                conn.Send(protoRet);
                return;
            }
        }

        public void MsgLogout(Connection conn,ProtocolBase protocol)
        {

        }

        
        
    }
}
