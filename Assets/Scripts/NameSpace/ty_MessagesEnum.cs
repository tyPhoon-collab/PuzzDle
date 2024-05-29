using System.Collections.Generic;

namespace MessagesEnum
{

    public enum Messages {
        Dead, 
        Hunger,
        Eat,
        Recover,
        SwordLvUp,
        ShiledLvUp,
        Get_Accs,
        Get_Item,
        //Damage,
    }

    /// <summary>
    /// Messageを日本語に変換します。
    /// ここで定義することで、表示する文字列を変えたいときに、このファイルを見るだけで良くなります。
    /// 特定の文字列は後で数値に変換される仕様です。
    /// </summary>
    public static class ty_MessagesEnum {       
        const string replaceString = "###";
        
        public static readonly Dictionary<Messages, string> MessagesName = new Dictionary<Messages, string>() {
            {Messages.Dead , "力尽きた"},
            {Messages.Hunger , $"お腹が {replaceString} 減った"},
            {Messages.Eat , $"お腹が {replaceString} 回復した"},
            {Messages.Recover , $"体力が {replaceString} 回復した"},
            {Messages.SwordLvUp , $"剣のレベルが {replaceString} になった"},
            {Messages.ShiledLvUp , $"盾のレベルが {replaceString} になった"},
            {Messages.Get_Accs , $"アクセサリー {replaceString} を取得しました"},
            {Messages.Get_Item , $"アイテム {replaceString} を取得しました"},
            //{Messages.Damage , $"\n{replaceString} のダメージを受けた"},
        };

        public static string GetMessage(this Messages message, int value = 0){
            if (MessagesName.TryGetValue(message, out string messageName)) {
                return messageName.Replace(replaceString, value.ToString());
            } 
            return message.ToString();
        }
        public static string GetMessage(this Messages message, string value)
        {
            if (MessagesName.TryGetValue(message, out string messageName))
            {
                return messageName.Replace(replaceString, value);
            }
            return message.ToString();
        }
    }
}