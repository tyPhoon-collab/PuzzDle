//ゲーム中に出てくる特別な式や関数、定数（ゲームの難易度などに関係する）はここに記述する。
//why -> ここを見ればゲームの難易度を調整できるようにしたい
//how to use -> 該当するスクリプトの文頭にnamespaceをusingします。 ex) using GameSystem;
//密な関係にはなるが、今回は密のほうがありがたい。そこまで大規模なプログラムでもない。
using AccsEnum;
using ItemsEnum;
using UnityEngine;

namespace GameSystem
{
    public struct SaveData {
        public int floorMax;
        public int floor;

        public SaveData(int floorMax, int floor) { 
            this.floor = floor;
            this.floorMax = floorMax;
        }
    }

    public struct PuzzleSaveData {
        public int moveable;
        public int additionalMoveable;
        public int deletePieceSum;
        public PuzzleSaveData(int moveable, int additionalMoveable, int sum)
        {
            this.moveable = moveable;
            this.additionalMoveable = additionalMoveable;
            this.deletePieceSum = sum;
        }
    }

    public class HeroSaveData {
        public int[] items;
        public bool[] accs;
        public int hunger;
        public int hp;
        public int exp;
        public int coin;
        public int crystal;
        public int swordLv;
        public int shiledLv;
        public int enemyDestroyNum;
        public HeroSaveData(int[] items, bool[] accs, int hunger, int hp, int exp, int coin, int crystal, 
            int swordLv, int shiledLv, int enemyDestroyNum)
        {
            this.items = items;
            this.accs = accs;
            this.hunger = hunger;
            this.hp = hp;
            this.exp = exp; 
            this.coin = coin;
            this.crystal = crystal;
            this.swordLv = swordLv;
            this.shiledLv = shiledLv;
            this.enemyDestroyNum = enemyDestroyNum;
        }
    }

    public class Status {
        public int HpMax{get; set;}
        public int Atk{get; set;}
        public int Def{get; set;}
        public Status(int hpMax, int atk, int def){
            HpMax = hpMax;
            Atk = atk;
            Def = def;
        }
    }

    public class EnemyStatus : Status
    {
        public int Exp { get; set; }
        public int Coin { get; set; }

        public EnemyStatus(int hpMax, int atk, int def, int coin, int exp) : base(hpMax, atk, def)
        {
            Exp = exp;
            Coin = coin;
        }
    }

    public class HeroStatus : Status{
        public int HungerMax{get; set;}
        public float Speed{get; set;}
        public HeroStatus(int hpMax, int atk, int def, int hungerMax, float speed = 0) : base (hpMax, atk, def) {
            HungerMax = hungerMax;
            Speed = speed;
        }
        
        public static HeroStatus operator+ (HeroStatus s1, HeroStatus s2)
        {
            return new HeroStatus(
                s1.HpMax + s2.HpMax,
                s1.Atk + s2.Atk,
                s1.Def + s2.Def,
                s1.HungerMax + s2.HungerMax,
                s1.Speed + s2.Speed
            );
        }
    }

    //ここにすべての主要なゲーム難易度を決める関数を定義する。

    //百分率でなく確率で設定します。評価するときはRandom.valueという[0,1)を返す値を用いましょう。
    public static class Functions {

        //クリスタルが出現する確率
        public const float crystalDropRate = 0.05f; 
        //宝箱が出現する確率
        public const float treasureBoxRate = 0.05f;
        //移動可能回数が増える確率
        public const float addMoveableTimesRate = 0.25f;
        //アクセサリーが落ちる確率。いまのところボスのみで参照される
        public const float accsDropRate = 0.02f;

        //Puzzleの初期値
        public const int initMoveableTimes = 10;

        //アイテム
        public const int maxItemNum = 999;       //個々の数

        //ダメージが表示される時間
        public const float timeDrawDamage = 1f;
        public const float timeDrawCombo = 1.5f;
        //ポップアップがでる時間
        public const float timeDrawPopUp = 5f;
        //フェードアウトの所要時間
        public const float timeFadeout = 2f;

        //ボスの時のステータスの上昇ウェイト。とりあえず何倍かで管理。
        public const int weightBossStatus = 5;
        public const int weightBossCoin = 3;
        public const int weightBossExp  = 10;


        //オートパズルを解禁する階数
        public const int canAutoPuzzleFloor = 51;
        //スピードアップを解禁する階数
        public const int canSpeedUpFloor = 11;

        //地面の座標
        public const float groundPosY = 2.0f;
        //右端
        public const float limitPosX = 19f;
        //爆発とかブラックホールとかの制限位置。
        public const float effectLimitPosX = 14.5f;   


        //フロアにでる敵の数
        public const int minEnemyNum = 3;
        public const int maxEnemyNum = 10;

        //Puzzleのボーナス
        public const int treasureBonusMinutesInterval = 3;
        public const int crystalBonusMinutesInterval = 5;

        //長押しようの設定
        public const float buttonLongPressRealTime = 1f;
        public const float buttonIntervalRealTime = 0.1f;
        //敵のアニメーションの速度。1が初期値で値が低いと遅くなります
        public const float enemyAnimSpeed = 0.5f;

        public const int canPlaySoundNum = 5;

        //ヒーローの初期値
        public static HeroStatus initStatus = new HeroStatus( 
            10,     //HpM
            0,      //Atk
            0,      //Def
            100,    //HungerM
            1.8f      //Speed
        );

        public static HeroSaveData initSaveData = new HeroSaveData(
            new int[(int)Items.MAX], //Items
            new bool[(int)Accs.MAX], //Accs
            initStatus.HungerMax, 
            initStatus.HpMax, 
            0, //exp
            0, //coin
            0, //cry
            1, //SwordLv
            1, //ShieldLv
            0  //enemyDestroyNum
        );

        //敵のステータスの設定
        public static EnemyStatus GetEnemyStatus(int floor) { 
            return new EnemyStatus( 
                (int)(floor * floor * 0.01f) + 1,
                (int)(floor * floor * 0.01f) + 1,
                (int)(floor * floor * 0.005f),
                GetCoinNum(floor),
                GetExpValue(floor)
            ); 
        }

        //コインやクリスタルで使われる。
        public static void AddUpForce(GameObject obj){
            obj.GetComponent<Rigidbody2D>().AddForce(
                new Vector3(
                    Random.Range(1.0f,2.0f),
                    Random.Range(12,17),
                    0
                )
            );
        }

        //敵が湧く範囲
        public static Vector3 GetEnemySpawnPos() {
            return new Vector3(
                Random.Range(5.0f,14.5f),
                groundPosY + Random.Range(0.5f,3.0f), 
                0
            );
        }
        public static Items GetWeightedRandomItem()
        {
            Rarities rarity;
            int rand = Random.Range(0, 100) + 1;

            if (rand <= (int)Rarities.N) 
                rarity = Rarities.N;
            else if (rand <= (int)Rarities.R) 
                rarity = Rarities.R;
            else if (rand <= (int)Rarities.SR) 
                rarity = Rarities.SR;
            else 
                rarity = Rarities.SSR;

            return GetRandom(Infos.rareItems[rarity]);
        }

        //階数による敵の湧く種類。配列のインデックスを返す。よって、プレハブの割り当て方に依存する。
        //まだ未設定。
        public static int GetEnemyKindsNum(int floor)
        {
            switch((int)((floor - 1) % 200 * 0.1f)){
                case 0: return GetRandom(0, 1);
                case 1: return GetRandom(6, 7);
                case 2: return GetRandom(3, 4);
                case 3: return GetRandom(1, 12);
                case 4: return GetRandom(8, 9, 13);
                case 5: return GetRandom(4, 5, 6);
                case 6: return GetRandom(10, 19);
                case 7: return GetRandom(2, 3);
                case 8: return GetRandom(21, 22);
                case 9: return GetRandom(10, 11, 12);

                case 10: return GetRandom(14, 15);
                case 11: return GetRandom(17, 18);
                case 12: return GetRandom(1, 3);
                case 13: return GetRandom(2, 6, 13);
                case 14: return GetRandom(4, 8, 22);
                case 15: return GetRandom(12, 13, 14);
                case 16: return GetRandom(10, 19);
                case 17: return GetRandom(5, 16);
                case 18: return GetRandom(3, 20);
                case 19: return GetRandom(22, 23, 24);
            }
            return 0;
        }
        public static int GetBossKindsNum(int floor)
        {
            switch((int)((floor - 1) % 200 * 0.1f)){
                case 0: return 0;
                case 1: return 5;
                case 2: return 4;
                case 3: return 1;
                case 4: return 9;
                case 5: return 6;
                case 6: return 10;
                case 7: return 2;
                case 8: return 21;
                case 9: return 11;

                case 10: return 14;
                case 11: return 18;
                case 12: return 3;
                case 13: return 13;
                case 14: return 22;
                case 15: return 12;
                case 16: return 19;
                case 17: return 16;
                case 18: return 20;
                case 19: return 24;
            }
            return 0;
        }
        public static Vector3 GetBossSpawnPos(){
            return new Vector3(15.0f,3.2f,0);
        }

        //次のレベルに必要な経験値。漸化式的に考えている
        public static float GetNextDeltaExp(float deltaExp, int lv) { 
            return (deltaExp * 1.1f + lv * 5) * 0.5f; 
        } 
    

        //敵がおとす経験値の設定
        public static int GetExpValue(int floor){ 
            return (int)(floor * 0.1f) + 1;  
        }
        
        //敵が落とすコインの設定
        public static int GetCoinNum(int floor){ 
            return (int)(floor * 0.01f) + 1;
        }

        //ヒーローのステータスの設定
        public static HeroStatus GetHeroStatus(int lv, AccsManager accm, EquipManager eqm) {
            HeroStatus s = initStatus + accm.AddStatus + eqm.AddStatus;
            return new HeroStatus(
                s.HpMax + (int)((lv - 1) * lv * 0.5f), 
                s.Atk + (int)((lv - 1) * lv * 0.25f), 
                s.Def + (int)((lv - 1) * lv * 0.25f), 
                s.HungerMax,
                s.Speed
            );
        }

        static T GetRandom<T>(params T[] indexs)
        {
            return indexs[Random.Range(0, indexs.Length)];
        } 
    }

    public class LevelManager {
        float deltaExp; //次のレベルまでに必要な経験値
        float sumExp;   //次のレベルまでに必要な合計経験値
        //実際は、上記の二つの変数は保持する必要がなく、指数関数の計算を行えばよい
        //しかし、指数関数は超越関数であるため、計算を避けるために、上記の2つの変数を保持することにする
        
        void SetNextDeltaExp(int lv){
            deltaExp = Functions.GetNextDeltaExp(deltaExp, lv);
        }

        public bool IsNextLv(int exp, int lv){
            if (exp < sumExp) return false; 
            sumExp += deltaExp;
            SetNextDeltaExp(++lv);
            return true;
        }
        public void Init(float deltaExp)
        {
            this.deltaExp = deltaExp;
            sumExp = 0;
        }
    }

    public class AccsManager {
        bool[] isAcquired = new bool[(int)Accs.MAX];
        HeroStatus addStatus = new HeroStatus(0, 0, 0, 0, 0);

        public bool[] IsAcquired{ 
            get { return isAcquired;} 
        }
        public int ExpWeight { get; private set; }
        public int CoinWeight { get; private set; }
        public int DropWeight { get; private set; }
        public HeroStatus AddStatus { get { return addStatus; } }

        public AccsManager(){
            for (int n = 0; n < (int)Accs.MAX; n++)
                isAcquired[n] = false;

            ExpWeight = 1;
            CoinWeight = 1;
            DropWeight = 1;
        }

        /// <summary>
        /// Acquireは獲得という意味。アクセサリーを獲得するときの処理を行う。
        /// </summary>
        /// <param name="acc"></param>
        /// <returns></returns>
        public bool SetAcquire(Accs acc){
            if (isAcquired[(int)acc]) return false; //いったんこうしておく。アクセサリーはいくらでも取得できるようにした方が面白いかも
            isAcquired[(int)acc] = true;
            switch (acc) {
                case Accs.HP_MAX_PLUS:
                    addStatus.HpMax += 500;
                    break;
                case Accs.HUNGER_MAX_PLUS:
                    addStatus.HungerMax += 50; 
                    break;
                case Accs.POWER_PLUS:
                    addStatus.Atk += 50;
                    addStatus.Def += 50;
                    break;
                case Accs.SPEED_PLUS:
                    addStatus.Speed += 0.5f;
                    break;
                case Accs.EXP_PLUS:
                    ExpWeight += 1;
                    break;
                case Accs.COIN_PLUS:
                    CoinWeight += 1;
                    break;
                case Accs.DROP_PLUS:
                    DropWeight += 1;
                    break;
            }
            return true;
        }
    }

    public class EquipManager {
        public int swordLv;
        public int shiledLv;
        public int SwordLv { 
            get { return swordLv; }
            set { 
                swordLv = value;
                addStatus.Atk = swordLv * Weight; 
            }
        }
        public int ShiledLv{
            get { return shiledLv; }
            set { 
                shiledLv = value;
                addStatus.Def = shiledLv * Weight; 
            }
        }
        public int Weight { get; set; }
        HeroStatus addStatus = new HeroStatus(0,0,0,0,0);

        public HeroStatus AddStatus {
            get { return addStatus; }
        }

        public EquipManager(){
            Weight = 1;
        }
    }
}
