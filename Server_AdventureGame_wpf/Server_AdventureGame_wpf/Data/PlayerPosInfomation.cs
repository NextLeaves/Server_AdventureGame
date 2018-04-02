using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_AdventureGame_wpf.Data
{
    public class PlayerPosInfomation
    {
        public string Account { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public PlayerPosInfomation() : this(null, 0f, 0f, 0f)
        {

        }

        public PlayerPosInfomation(string account, float x, float y, float z)
        {
            Account = account;
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
