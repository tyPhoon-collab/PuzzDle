using System.Collections.Generic;

namespace ItemsEnum
{
    //確率を書いておく。
    public enum Rarities
    {
        N = 66,
        R = 25 + Rarities.N,
        SR = 7 + Rarities.R,
        SSR = 2 + Rarities.SR,
    }

    public enum Items {
        POTION_HP,
        FOOD,
        SWORD,
        SHIELD,
        BLACKHOLE,
        FAN,
        EARTHQUAKE,
        GRAVITY,
        EXPLOSION,
        ADD_MOVEABLE,
        MAX,
        TOMATO,
        SUSHI,
        YAKINIKU,
        CURRY,
        GAMEMACHINE,
        UMA,
        PALETTE,
        UTIWA,
        TIRE,
        DRONE,
        MACHINEGUN,
        LUCKBOOK,
        EXPPLUS,
        FOODPLUS,
        PETBOTTLE,
        ENERGYDRINK,
    }

    public struct ItemInfo
    {
        public ItemInfo(string name, int co_value = 0, int cr_value = 0){
            Name = name;
            CoinValue = co_value;
            CrystalValue = cr_value;
        }
        public string Name{ get; set; }
        public int CoinValue{ get; set; }
        public int CrystalValue{ get; set; }
    }

    public static class Infos {
        public static readonly Dictionary<Items, ItemInfo> ItemsName = new Dictionary<Items, ItemInfo>() {
            {Items.POTION_HP,   new ItemInfo("回復薬", 50)},
            {Items.FOOD ,       new ItemInfo("食べ物", 50)},
            {Items.SWORD ,      new ItemInfo("ソード強化", 10)},
            {Items.SHIELD ,     new ItemInfo("シールド強化", 10)},
            {Items.FAN ,        new ItemInfo("斬撃", 500)},
            {Items.EARTHQUAKE , new ItemInfo("地震", 1000)},
            {Items.BLACKHOLE ,  new ItemInfo("ブラックホール", 100)},
            {Items.ADD_MOVEABLE,new ItemInfo("移動可能回数券", 0, 10)},
            {Items.GRAVITY ,    new ItemInfo("アンチグラビティ", 3000)},
            {Items.EXPLOSION ,  new ItemInfo("爆発", 3000)},
            //{Items.TOMATO ,     new ItemInfo("回復薬", 10)},
            //{Items.UTIWA ,      new ItemInfo("うちわ", 10)},
            //{Items.DRONE ,      new ItemInfo("ドローン", 10)},
        };


        public static readonly Dictionary<Rarities, Items[]> rareItems = new Dictionary<Rarities, Items[]>()
        {
            {Rarities.N,   new Items[]{ Items.POTION_HP, Items.FOOD, Items.SWORD, Items.SHIELD } },
            {Rarities.R,   new Items[]{ Items.FAN, Items.EARTHQUAKE } },
            {Rarities.SR,  new Items[]{ Items.BLACKHOLE, Items.ADD_MOVEABLE } },
            {Rarities.SSR, new Items[]{ Items.GRAVITY, Items.EXPLOSION } },
        };


        public static ItemInfo GetItemInfo(this Items value){
            if (ItemsName.TryGetValue(value, out ItemInfo info)) {
                return info;
            } 
            return new ItemInfo();
        }
    }
}