#define RELEASE

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Timers;
using System.Reflection;

using Server_AdventureGame_wpf.Logic;
using System.Diagnostics;

namespace Server_AdventureGame_wpf.Core
{
    public class Server
    {
        public static Server _instance = new Server();

        public string Expression { get; set; } = "Server_001";
        public Socket Listenfb { get; set; }
        public Connection[] conns { get; set; }
        public int MaxCapacity { get; set; } = 50;

        //心跳时间
        public int HeartBeatTime = 180;
        //协议
        public ProtocolBase proto = new ProtocolByte();
        public ConnMsgHandle _connMsgHandle;
        public PlayerEventHandle _playerEventHandle;
        public PlayerMsgHandle _playerMsgHandle;

        private Timer timer = new Timer(1000);

        private Server()
        {
            Listenfb = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            conns = new Connection[MaxCapacity];
            for (int index = 0; index < MaxCapacity; index++)
            {
                conns[index] = new Connection();
            }

            _connMsgHandle = new ConnMsgHandle();
            _playerEventHandle = new PlayerEventHandle();
            _playerMsgHandle = new PlayerMsgHandle();
        }

        public static Server GetUniqueServer()
        {

            lock (_instance)
            {
                return _instance;
            }

        }

        public int ContributeIndex()
        {
            for (int index = 0; index < conns.Length; index++)
            {
                if (conns[index] == null)
                {
                    conns[index] = new Connection();
                    return index;
                }
                else if (!conns[index].IsUse) return index;
            }
            return -1;
        }

        public void Startup(string ip, int port)
        {
            IPAddress ipAd = IPAddress.Parse(ip);
            IPEndPoint ipEp = new IPEndPoint(ipAd, port);
            Listenfb.Bind(ipEp);
            Listenfb.Listen(MaxCapacity);
            Listenfb.BeginAccept(AcceptCb, null);
            Console.WriteLine($"[Startup] {Expression} server.");

            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += HeartBeatElapsed;
            timer.Start();
        }

        private void HeartBeatElapsed(object sender, ElapsedEventArgs e)
        {
            HeartBeat();
        }
        public void HeartBeat()
        {
            double utcTimeNow = Sys.GetTimeStamp();
            foreach (Connection conn in conns)
            {
                if (conn == null) continue;
                if (!conn.IsUse) continue;
                if (conn.LastTickTime < utcTimeNow - HeartBeatTime)
                {
                    Console.WriteLine($"[Disconnected] Client:{conn.RemoteAddress}.");
                    lock (conn)
                    {
                        conn.Close();
                    }
                }
            }
        }

        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket targetSocket = Listenfb.EndAccept(ar);
                int index = ContributeIndex();
                if (index < 0)
                {
                    targetSocket.Close();
                    Trace.WriteLine("[Warning] This server is full.");
                }
                else
                {
                    Connection conn = conns[index];
                    conn.Initialize(targetSocket);
                    Debug.WriteLine($"[Info] Server is connected client:{conn.RemoteAddress}.");
                    Sys.sb_Log.Append($"[Info] Server is connected client:{conn.RemoteAddress}.");
                    conn.Socket.BeginReceive(conn.BufferRead, conn.BufferCount, conn.BufferRemain, SocketFlags.None, ReceiveCb, conn);
                }

                Listenfb.BeginAccept(AcceptCb, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Exception] AcceptCb Method is error.");
                Trace.Assert(ex == null, $"[Error Info] {ex.Message}");
            }
        }

        private void ReceiveCb(IAsyncResult ar)
        {
            Connection conn = ar.AsyncState as Connection;
            try
            {
                int count = conn.Socket.EndReceive(ar);
                conn.BufferCount += count;
                //数据处理
                ProcessData(conn);
                //分发消息

                conn.Socket.BeginReceive(conn.BufferRead, conn.BufferCount, conn.BufferRemain, SocketFlags.None, ReceiveCb, conn);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Exception] ReceiveCb Method is error.line : 153.");                
            }
        }

        private void ProcessData(Connection conn)
        {
            //消息长度
            if (conn.BufferCount < sizeof(int)) return;
            Array.Copy(conn.BufferRead, conn.LenBytes, sizeof(int));
            conn.LenMsg = BitConverter.ToInt32(conn.LenBytes, 0);
            if (conn.BufferCount < conn.LenMsg + sizeof(int)) return;

#if RELEASE
            //协议处理                       
            ProtocolBase protocol = proto.Decode(conn.BufferRead, 0, conn.BufferCount);
            MessageHandle(conn, protocol);

#elif DEBUG
            string m = Encoding.UTF8.GetString(conn.BufferRead, sizeof(Int32), conn.BufferCount);
            Console.WriteLine(m);
#endif

            //清除消息
            int count = conn.BufferCount - conn.LenMsg - sizeof(int);
            Array.Copy(conn.BufferRead, sizeof(int) + conn.LenMsg, conn.BufferRead, 0, count);
            conn.BufferCount = count;
            if (conn.BufferCount > 0) ProcessData(conn);
        }

        private void MessageHandle(Connection conn, ProtocolBase protocol)
        {
            Console.WriteLine($"[Protocol] {protocol.Name}.");
            Console.WriteLine($"[Protocol] {protocol.Expression}");
            string name = protocol.Name;
            string methodName = "Msg" + name;

            //连接协议分发
            if (conn.Player == null || name == "HeartBeat" || name == "Logout")
            {
                MethodInfo mInfo = _connMsgHandle.GetType().GetMethod(methodName);
                if (mInfo == null)
                {
                    Console.WriteLine($"[Error] ConnMsgHandle class have not {methodName}.");
                    return;
                }
                object[] objs = new object[] { conn, protocol };
                Console.WriteLine($"[ConnHandle] {conn.RemoteAddress} : {name}.");
                mInfo.Invoke(_connMsgHandle, objs);
            }
            //角色协议分发
            else
            {
                MethodInfo mInfo = _playerMsgHandle.GetType().GetMethod(methodName);
                if (mInfo == null)
                {
                    Console.WriteLine($"[Error] PlayerMsgHandle class have not {methodName}.");
                    return;
                }

                object[] objs = new object[] { conn, protocol };
                Console.WriteLine($"[PlayerHandle] {conn.Player.Account} : {name}.");
                mInfo.Invoke(_playerMsgHandle, objs);
            }
        }

        public void Send(Connection conn, ProtocolBase protocol)
        {
            try
            {
                conn.Send(protocol);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"[Error] Send Method is error.");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Send Method is error.");
                Console.WriteLine(ex.Message);
            }

        }
        /// <summary>
        /// 全局发送协议
        /// </summary>
        /// <param name="protocol">协议对象</param>
        public void Broadcast(ProtocolBase protocol)
        {
            ProtocolByte proto = protocol as ProtocolByte;
            string id = proto.GetString(1);
            foreach (Connection conn in conns)
            {
                if (!conn.IsUse) continue;
                if (conn.Player == null) continue;
                if (conn.Player.Account == id) continue;
                Send(conn, protocol);
            }
        }
        /// <summary>
        /// 全局寻找缓存池内conn的引用
        /// </summary>
        /// <param name="value">赋值初识位置</param>
        public void Broadcast(string id, Middle.Vector3 position)
        {
            foreach (Connection conn in conns)
            {
                if (!conn.IsUse) continue;
                if (conn.Player.Account == id)
                {
                    conn.Player.Tempdata.Postion = position;
                }
            }
        }

        public bool Broadcast(string id, Middle.PlayerScore score)
        {
            foreach (Connection conn in conns)
            {
                if (!conn.IsUse) continue;
                if (conn.Player.Account == id)
                {
                    conn.Player.Tempdata.Score = score;
                    return true;
                }
            }
            return false;
        }

        public bool OnLogining(string account)
        {
            foreach (var item in conns)
            {
                if (!item.IsUse) continue;
                if (item.Player == null) continue;
                if (item.Player.Account == account)
                {
                    Trace.WriteLine($"[Kickoff]Account:{account}.");
                    Sys.sb_Log.Append($"[Kickoff]Account:{account}.");
                    return true;
                }
            }
            return false;
        }

        public Connection OnLoginned(string account)
        {
            foreach (var item in conns)
            {
                if (!item.IsUse) continue;
                if (item.Player == null) continue;
                if (item.Player.Account == account)
                {
                    Trace.WriteLine($"[Kickoff]Account:{account}.");
                    Sys.sb_Log.Append($"[Kickoff]Account:{account}.");
                    return item;
                }
            }
            throw new NullReferenceException("Logining is error.conn is not exsit.");
        }

        public string PrintInformation()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Connection conn in conns)
            {
                if (conn == null) continue;
                if (!conn.IsUse) continue;
                if (conn.Player == null)
                {
                    Debug.WriteLine($"[Connected]Player Ip:{conn.RemoteAddress},Player Id is not read.");
                    continue;
                }

                string msg = $"[Connected]Player Ip:{conn.RemoteAddress},Player Id:{conn.Player.Account}.";
                Debug.WriteLine(msg);
                sb.Append(msg);
            }
            return sb.ToString();
        }

        public void Close()
        {
            foreach (Connection conn in conns)
            {
                if (conn == null) continue;
                if (!conn.IsUse) continue;
                lock (conn)
                {
                    Console.WriteLine($"[Disconnected] Client:{conn.RemoteAddress}.");
                    conn.Close();
                }
            }
        }

    }
}
