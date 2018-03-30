﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_AdventureGame_wpf.Logic
{
    public static class NamesOfProtocol
    {
        public const string Register = "Register";        
        public const string HeartBeat = "HeartBeat";
        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string FindPassword = "FindPassword";
        public const string ChangePassword = "ChangePassword";
        public const string ReadPlayerData = "ReadPlayerData";
        public const string CreatePlayer = "CreatePlayer";

    }
}
