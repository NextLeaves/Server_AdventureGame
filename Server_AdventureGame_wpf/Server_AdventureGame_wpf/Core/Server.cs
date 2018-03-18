using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Server_AdventureGame_wpf.Core
{
    public class Server
    {
        public Server _instance;

        public string Expression { get; set; } = "No Expression";
        public Socket Listenfb { get; set; }
        public Connection[] conns { get; set; }
        public int MaxCapacity { get; set; } = 50;

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
                //数据处理
                //分发消息

                conn.Socket.BeginReceive(conn.BufferRead, conn.BufferCount, conn.BufferRemain, SocketFlags.None, ReceiveCb, conn);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Exception] ReceiveCb Method is error.");
                Console.WriteLine($"[Error Info] {ex.Message}");
            }
        }

    }
}
