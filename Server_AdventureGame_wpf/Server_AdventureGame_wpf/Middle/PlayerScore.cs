using System;

namespace Server_AdventureGame_wpf.Middle
{
    /// <summary>
    /// 玩家临时数据缓存
    /// </summary>
    [Serializable]
    public class PlayerScore
    {
        public int Coin { get; set; }
        public int Money { get; set; }
        public int Star { get; set; }
        public int Diamand { get; set; }

        public PlayerScore() : this(100, 100, 100, 100)
        {

        }

        public PlayerScore(int coin, int money, int star, int diamand)
        {
            Coin = coin;
            Money = money;
            Star = star;
            Diamand = diamand;
        }
    }
}
