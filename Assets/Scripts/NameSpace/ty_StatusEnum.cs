using System.Collections.Generic;

namespace StatusEnum
{
    public enum GameStatus{
        INIT,
        RESET,
        CLICKABLE,
        WAIT_MOUSE_UP,
        SWAPING,
        DELETING,
        WAIT_INTERVAL,
        ERROR
    }

    public static class ty_StatusEnum {               
        public static readonly Dictionary<GameStatus, string> gameStatusName = new Dictionary<GameStatus, string>() {
            {GameStatus.CLICKABLE , "操作可能"},
            {GameStatus.WAIT_INTERVAL , "待ち時間"},
            {GameStatus.WAIT_MOUSE_UP , "離し待ち"},
            {GameStatus.INIT , "初期化"},
            {GameStatus.RESET , "リセット中"},
            {GameStatus.SWAPING , "移動中"},
            {GameStatus.DELETING , "コンボ中"},
        };

        public static string GetName(this GameStatus status){
            if (gameStatusName.TryGetValue(status, out string name)) {
                return name;
            } 
            return status.ToString();
        }
    }
}