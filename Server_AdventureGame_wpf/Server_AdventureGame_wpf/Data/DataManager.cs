using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server_AdventureGame_wpf.Data
{
    public class DataManager
    {
        private static DataManager _instance = new DataManager();

        private DataManager()
        {

        }

        public static DataManager GetSingleton()
        {
            if (_instance != null)
            {
                lock (_instance)
                {
                    return _instance ?? new DataManager();
                }
            }
            return _instance;
        }

        public bool IsSafeString(string str) => !Regex.IsMatch(str, @"[-|,|*|(|)]");

        public bool CanRegister(string account)
        {
            if (!IsSafeString(account))
            {
                //非法字符
                return false;
            }

            using (var db = new UserContext())
            {
                var query = from u in db.Users
                            where u.Account == account
                            select u;
                if (query.Count() != 0)
                {
                    return false;
                }

                return true;
            }
        }

        public bool Register(string account, string password, out int code)
        {
            code = int.MinValue;
            if (!IsSafeString(account) || !IsSafeString(password))
            {
                //非法字符
                return false;
            }

            using (var db = new UserContext())
            {
                User user = new User(account, password);
                code = user.CommandCode;
                db.Users.Add(user);

                db.SaveChanges();
            }
            return true;
        }

        public bool CreatePlayer(string account)
        {
            if (!IsSafeString(account))
            {
                //非法字符
                return false;
            }

            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            Middle.PlayerData playerData = new Middle.PlayerData();

            try
            {
                formatter.Serialize(stream, playerData);
            }
            catch (IOException ex)
            {
                Console.WriteLine("[Error]Create Player Method is error.[Line:75]");
                Console.WriteLine(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            byte[] byteData = stream.ToArray();
            Player player = new Player(account, byteData);

            using (var db = new UserContext())
            {
                db.PlayerDatas.Add(player);

                db.SaveChanges();
            }
            return true;
        }

        public Middle.PlayerData GetPlayerData(string account)
        {
            Middle.PlayerData playerDataRet = null;
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();

            try
            {
                using (var db = new UserContext())
                {
                    var queryResult = db.PlayerDatas.Where(p => p.Account == account).Select(p => p);
                    if (queryResult == null) throw new ArgumentNullException($"PlayerData is null.[Line:128]");
                    stream.Write((queryResult as Player).Data, 0, (queryResult as Player).Data.Length);
                    if (stream == null) throw new ArgumentNullException($"PlayerData is null.[Line:128]");
                    playerDataRet = formatter.Deserialize(stream) as Middle.PlayerData;
                }
                return playerDataRet;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Error]Create Player Method is error.[Line:137]");
                Console.WriteLine(ex.Message);
                throw new ArgumentNullException($"PlayerData is null.[Line:128]");
            }

        }

        public bool SavePlayerData(Middle.Player player)
        {
            string account_tp = player.Account;
            Middle.PlayerData playerData_tp = player.Data;

            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();

            try
            {
                formatter.Serialize(stream, playerData_tp);
                using (var db = new UserContext())
                {
                    var queryResult = db.PlayerDatas.Where(p => p.Account == account_tp).Select(p => p);
                    if (queryResult == null) return false;
                    (queryResult as Player).Data = stream.ToArray();

                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Error]Create Player Method is error.[Line:144]");
                Console.WriteLine(ex.Message);
                throw new ArgumentNullException($"PlayerData is null.[Line:144]");
            }
        }

        public bool CheckPassword(string account, string password)
        {
            if (!IsSafeString(account) || !IsSafeString(password))
            {
                //非法字符
                return false;
            }

            using (var db = new UserContext())
            {
                var queryResult = from u in db.Users
                                  where (u.Account == account && u.Password == password)
                                  select u;

                return queryResult.Count() > 0 ? true : false;
            }
        }

        public bool FindoutPassword(string account, int commandCode)
        {

            if (!IsSafeString(account) || !IsSafeString(Convert.ToString(commandCode)))
            {
                //非法字符
                return false;
            }

            using (var db = new UserContext())
            {
                var queryResult = db.Users.Where(u => u.Account == account && u.CommandCode == commandCode).Select(u => u);
                if (queryResult.Count() == 1) return true;
            }

            return false;
        }

        public bool ChangePassword(string account, string password)
        {
            if (!IsSafeString(account) || !IsSafeString(password))
            {
                //非法字符
                return false;
            }

            using (var db = new UserContext())
            {
                var queryResult = from u in db.Users
                                  where u.Account == account
                                  select u;

                foreach (var item in queryResult)
                {
                    item.Password = password;
                    item.LastPd.Add(password);
                }

                db.SaveChanges();
                return true;
            }
        }


    }
}
