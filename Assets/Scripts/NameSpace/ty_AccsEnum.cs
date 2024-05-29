using System.Collections.Generic;
namespace AccsEnum
{
    public enum Accs
    {
        NONE = -1,      
        HP_MAX_PLUS,
        HUNGER_MAX_PLUS,
        POWER_PLUS,
        SPEED_PLUS,
        EXP_PLUS,
        COIN_PLUS,
        DROP_PLUS,
        MAX
    }

    public struct AccsInfo
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public AccsInfo (string name, string summary){
            Name = name;
            Summary = summary;
        }
    }

    public static class ty_AccsEnum {               
        public static readonly Dictionary<Accs, AccsInfo> accsName = new Dictionary<Accs, AccsInfo>() {
            {Accs.HP_MAX_PLUS, new AccsInfo("体力バッチ", "最大体力が上昇します。")},
            {Accs.HUNGER_MAX_PLUS, new AccsInfo("お腹バッチ", "最大満腹度が上昇します。")},
            {Accs.POWER_PLUS, new AccsInfo("腕力バッチ", "筋力が上がり、攻撃力と防御力が上昇します。")},
            {Accs.SPEED_PLUS, new AccsInfo("脚力バッチ", "脚力が上がり、足が速くなります。")},
            {Accs.EXP_PLUS, new AccsInfo("経験値の水晶", "物覚えが良くなり、獲得する経験値が増えます。")},
            {Accs.COIN_PLUS, new AccsInfo("コインの水晶", "金運が上がり、獲得するコインが増えます。")},
            {Accs.DROP_PLUS, new AccsInfo("メガネ", "視力が上がり、アイテムのドロップ率が増えます。")},
        };

        public static string GetName(this Accs acc){
            if (accsName.TryGetValue(acc, out AccsInfo accsInfo)) {
                return accsInfo.Name;
            } 
            return acc.ToString();
        }

        public static string GetSummary(this Accs acc)
        {
            if (accsName.TryGetValue(acc, out AccsInfo accsInfo))
            {
                return accsInfo.Summary;
            }
            return acc.ToString();
        }
    }
}