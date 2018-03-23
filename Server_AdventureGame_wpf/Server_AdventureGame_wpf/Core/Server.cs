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

namespace Server_AdventureGame_wpf.Core
{
    public class Server
    {
        public static Server _instance = new Server();

        public string Expression { get; set; } = "No Expression";
        public Socket Listenfb { get; set; }
        public Connection[] conns { get; set; }
        public int MaxCapacity { get; set; } = 50;

        //心跳时间
        public int HeartBeatTime = 180;
        //协议
        public ProtocolBase proto = new ProtocolByte();
        private ConnMsgHandle _connMsgHandle;
        private PlayerEventHandle _playerEventHandle;
        private PlayerMsgHandle _playerMsgHandle;

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
                    Console.WriteLine("[Warning] This server is full.");
                }
                else
                {
                    Connection conn = conns[index];
                    conn.Initialize(targetSocket);
                    Console.WriteLine($"[Info] Server is connected client:{conn.RemoteAddress}.");
                    conn.Socket.BeginReceive(conn.BufferRead, conn.BufferCount, conn.BufferRemain, SocketFlags.None, ReceiveCb, conn);
                }

                Listenfb.BeginAccept(AcceptCb, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Exception] AcceptCb Method is error.");
                Console.WriteLine($"[Error Info] {ex.Message}");
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
                Console.WriteLine("[Exception] ReceiveCb Method is error.");
                Console.WriteLine($"[Error Info] {ex.Message}");
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

                object[] objs = new object[] { conn.Player, protocol };
                Console.WriteLine($"[PlayerHandle] {conn.Player.Id} : {name}.");
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

        public void Broadcast(ProtocolBase protocol)
        {
            foreach (Connection conn in conns)
            {
                if (!conn.IsUse) continue;
                Send(conn, protocol);
            }
        }

        public void PrintInformation()
        {
            Console.WriteLine("---Server Login Infomation---");
            foreach (Connection conn in conns)
            {
                if (conn == null) continue;
                if (!conn.IsUse) continue;
                if (conn.Player == null) Console.WriteLine($"[Connected]Player Ip:{conn.RemoteAddress},Player Id is not read.");
                Console.WriteLine($"[Connected]Player Ip:{conn.RemoteAddress},Player Id:{conn.Player.Id}.");
            }
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
