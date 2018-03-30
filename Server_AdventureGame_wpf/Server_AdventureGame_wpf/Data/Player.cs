using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace Server_AdventureGame_wpf.Data
{
    public class Player
    {
        [Key]
        public int RoleCode { get; set; }
        public string Account { get; set; }
        public byte[] Data { get; set; }

        public Player(string account,byte[] data)
        {
            Account = account;
            Data = data;
        }
        public Player()
        {

        }
    }
}
