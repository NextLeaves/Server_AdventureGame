﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server_AdventureGame_wpf.Core
{
    public class Connection
    {
        public const int BUFFER_SIZE = 1024;

        public Socket Socket { get; set; }
        public bool IsUse { get; set; }
        public byte[] BufferRead { get; set; }
        public int BufferCount { get; set; }
        public int BufferRemain { get => BUFFER_SIZE - BufferCount; }
        public string RemoteAddress { get => IsUse ? Socket.RemoteEndPoint.ToString() : "[Error] Not Use."; }
        //粘包分包机制
        public byte[] LenBytes { get; set; } = new byte[sizeof(Int32)];
        public Int32 LenMsg { get; set; } = 0;
        //心跳时间
        public double LastTickTime { get; set; } = long.MinValue;
        


        public Connection()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IsUse = false;
            BufferRead = new byte[BUFFER_SIZE];
            BufferCount = 0;
        }

        public void Initialize(Socket socket)
        {
            Socket = socket;
            IsUse = true;
            BufferCount = 0;
            LastTickTime = Sys.GetTimeStamp();
        }

        public void Close()
        {
            if (!IsUse) Console.WriteLine("[Error] Not Use.");
            Console.WriteLine($"[Disconnection] Client:{RemoteAddress}.");
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
            IsUse = false;
        }
    }
}
