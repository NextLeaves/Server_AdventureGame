using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_AdventureGame_wpf
{
    public class Sys
    {
        public static double GetTimeStamp()
        {
            TimeSpan time = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return time.TotalSeconds;
        }

        public static int CodeGenerator()
        {
            Random rand = new Random();
            return rand.Next(100000, 999999);
        }
    }
}
