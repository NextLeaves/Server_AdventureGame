using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_AdventureGame_wpf.Middle
{
    public class TempData
    {
        public Vector3 Postion { get; set; } = new Vector3();
        public Vector3 Rotation { get; set; } = new Vector3();
        public PlayerScore Score { get; set; } = new PlayerScore();
     
    }
}
