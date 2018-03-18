using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Timers;

namespace Server_AdventureGame_wpf.Core
{
    public class Server
    {
        public Server _instance;

        public string Expression { get; set; } = "No Expression";
        public Socket Listenfb { get; set; }
        public Connection[] conns { get; set; }
        public int MaxCapacity { get; set; } = 50;

        //心跳时间
        public int HeartBeatTime = 180;
        //协议
        public ProtocolBase proto;

        private Timer timer = new Timer(1000);

        private Server()
        {
            Listenfb = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            for (int index = 0; index < MaxCapacity; index++)
            {
                conns[index] = new Connection();
            }
        }

        public Server GetUniqueServer()
        {
            if (_instance == null)
            {
                lock (_instance)
                {
                    return new Server();
                }
            }
            return _instance;
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
            ProtocolBase protocol = proto.Decode(conn.BufferRead, sizeof(int), conn.LenMsg);

        }

        private void MessageHandle(Connection conn, ProtocolBase protocol)
        {
            Console.WriteLine($"[Protocol] {protocol.Name}.");


        }

        public void Send(Connection conn, ProtocolBase protocol)
        {
            byte[] bytes = protocol.Encode();
            byte[] lenBytes = BitConverter.GetBytes(bytes.Length);
            byte[] sendBytes = lenBytes.Concat(bytes).ToArray();
            //未完成
        }

        public void Broadcast(ProtocolBase protocol)
        {
            foreach (Connection  conn in conns)
            {
                if (!conn.IsUse) continue;
                Send(conn, protocol);
            }
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
