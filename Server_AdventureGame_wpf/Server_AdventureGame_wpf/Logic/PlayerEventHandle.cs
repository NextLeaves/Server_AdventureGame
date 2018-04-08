using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server_AdventureGame_wpf.Middle;
using Server_AdventureGame_wpf;

namespace Server_AdventureGame_wpf.Logic
{
    public class PlayerEventHandle
    {
        public void OnLogin(Player player)
        {

        }

        public void OnLogout(Player player)
        {
            //把临时数据写入数据库
            byte[] data = Data.DataManager.GetSingleton().ConvertScore(player.Tempdata.Score);
            bool isLogout = Data.DataManager.GetSingleton().SavePlayer(player.Account, data);
            if (isLogout)
            {
                Console.WriteLine($"Player:{player.Account} is quit game.");
            }
            else
            {
                Console.Write("SaveData is error.[Location]PlayerEventhandle-OnLogout-30");
            }
        }
    }
}
