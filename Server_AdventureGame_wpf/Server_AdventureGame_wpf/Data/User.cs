using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace Server_AdventureGame_wpf.Data
{
    public class User
    {
        [Key] public int UserCode { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public List<string> LastPd { get; set; }

        public User()
        {
            LastPd = new List<string>();
        }

        public User(string account, string password) : this()
        {
            Account = account;
            Password = password;
        }
    }
}
