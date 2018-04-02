using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_AdventureGame_wpf.Data
{
    public class ScenePlayersManager
    {
        public static ScenePlayersManager _instance = new ScenePlayersManager();

        public List<PlayerPosInfomation> PlayersPos { get; }

        public ScenePlayersManager()
        {
            PlayersPos = new List<PlayerPosInfomation>();
        }

        public bool Add(PlayerPosInfomation playerPos)
        {
            if (!PlayersPos.Contains(playerPos))
            {
                PlayersPos.Add(playerPos);
                return true;
            }
            return false;
        }

        public bool Remove(PlayerPosInfomation playerPos)
        {
            if (PlayersPos.Contains(playerPos))
            {
                PlayersPos.Remove(playerPos);
                return true;
            }
            return false;
        }


    }
}
