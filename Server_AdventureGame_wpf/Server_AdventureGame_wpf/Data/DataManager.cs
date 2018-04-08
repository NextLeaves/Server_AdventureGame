using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Server_AdventureGame_wpf.Middle;

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

        public bool CreatePlayerData(string account)
        {
            Middle.PlayerScore score = new Middle.PlayerScore(100, 100, 100, 100);
            return SavePlayer(account, DataManager.GetSingleton().ConvertScore(score));
        }

        public bool SavePlayer(string account, byte[] data)
        {
            if (!IsSafeString(account))
            {
                //非法字符
                return false;
            }

            Player p = new Player();
            p.Account = account;
            p.Data = data;

            using (var db = new UserContext())
            {
                var queryRresult = from pl in db.PlayerDatas
                                   where pl.Account == account
                                   select pl;
                if (queryRresult.Count() <= 0)
                {
                    db.PlayerDatas.Add(p);
                }
                else
                {
                    foreach (var item in queryRresult)
                    {
                        item.Data = data;
                    }
                }
                db.SaveChanges();
            }
            return true;
        }

        public byte[] GetPlayerData(string account)
        {

            using (var db = new UserContext())
            {
                var queryResult = from p in db.PlayerDatas
                                  where p.Account == account
                                  select p;
                if (queryResult.Count() <= 0) return null;
                foreach (var item in queryResult)
                {
                    if (item.Data != null)
                        return item.Data;
                }
                return null;
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

        public Middle.PlayerScore ConvertData(byte[] originData)
        {
            using (MemoryStream ms = new MemoryStream(originData))
            {
                BinaryFormatter bf = new BinaryFormatter();
                PlayerScore socre = bf.Deserialize(ms) as PlayerScore;
                if (socre == null) throw new IOException($"[Error]ConvertData is fail.");
                return socre;
            }
        }

        public byte[] ConvertScore(PlayerScore score)
        {
            using (Stream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, score);
                if (ms == null) throw new IOException($"[Error]ConvertPlayerdata is fail.");
                return (ms as MemoryStream).ToArray();
            }
        }

        public bool ClearPlayersTable()
        {
            using (var db = new UserContext())
            {
                var queryResult = from p in db.PlayerDatas
                                  select p;
                if (queryResult.Count() <= 0) return true;
                foreach (var item in queryResult)
                {
                    db.PlayerDatas.Remove(item);
                }
                db.SaveChanges();
                return true;
            }

        }

        public bool ClearUsersTable()
        {
            using (var db = new UserContext())
            {
                var queryResult = from p in db.Users
                                  select p;
                if (queryResult.Count() <= 0) return true;
                foreach (var item in queryResult)
                {
                    db.Users.Remove(item);

                }
                db.SaveChanges();
                return true;
            }
        }

        public bool ClearAllTables()
        {
            bool isClear = false;
            if (ClearUsersTable() && ClearPlayersTable())
                isClear = true;
            else isClear = false;
            return isClear;
        }

    }
}
