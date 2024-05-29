using GameSystem;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static GameSystem.Functions;
using PiecesEnum;

public class GameDirector : MonoBehaviour
{
    [SerializeField] GameObject[] enemys;

    ty_Enemy[,] enemyObjs;

    public Transform enemysParent;
    public Transform dropItemParent;
    
    int floor;
    public ty_Log tyLog;
    public ty_Hero tyHero;
    public Puzzle puzzle;
    public Slider entryFloorSlider;
    public GameObject maskImage;

    bool canAutoPuzzle = false;
    bool canSpeedUp = false;
    public GameObject autoPuzzleMaskImage;
    public GameObject speedUpMaskImage;

    public AudioSource audioSource;

    int maxFloor;

    int t_bonusMinutes = treasureBonusMinutesInterval;
    int c_bonusMinutes = crystalBonusMinutesInterval;

    System.TimeSpan preTimeSpan;
    System.DateTime startDateTime;
    System.TimeSpan playTimeSpan;

    public System.TimeSpan PlayTimeSpan {
        get { return playTimeSpan;  }
        set { 
            playTimeSpan = value;
            DropBonus();
            tyLog.UpdatePlayTime((playTimeSpan + preTimeSpan).ToString(@"dd\.hh\:mm\:ss"));
        }
    }

    void DropBonus()
    {
        if (IsBonus(t_bonusMinutes))
        {
            t_bonusMinutes += treasureBonusMinutesInterval;
            puzzle.AddLqueRarePieces(Effects.TREASURE);
        }
        if (IsBonus(c_bonusMinutes))
        {
            c_bonusMinutes += crystalBonusMinutesInterval;
            puzzle.AddLqueRarePieces(Effects.CRYSTAL);
        }
    }

    public int Floor {
        get { return this.floor; }
        set { 
            this.floor = value;
            if (MaxFloor < value) MaxFloor = value;
            tyHero.Floor = value;
            tyLog.UpdateFloor(value);
        }
    }
    public int MaxFloor {
        get { return maxFloor; }
        set {
            maxFloor = value;
            entryFloorSlider.maxValue = value;
            if (!canAutoPuzzle && value >= canAutoPuzzleFloor)
            {
                canAutoPuzzle = true;
                autoPuzzleMaskImage.SetActive(false);
            }
            if (!canSpeedUp && value >= canSpeedUpFloor)
            {
                canSpeedUp = true;
                speedUpMaskImage.SetActive(false);
            }
            tyLog.UpdateMaxFloor(value);
        }
    }

    void Start() {
        startDateTime = System.DateTime.Now;

        CheckSaveData();        

        //先に出現しうる敵を生成する。頻繁にInstantiateされるのでSetActiveを変更する方法にする
        enemyObjs = new ty_Enemy[enemys.Length, maxEnemyNum + 1]; //ボス用にもうひとつ確保。
        
        for (int i = 0; i < enemyObjs.GetLength(0); i++) {
            for (int j = 0; j < maxEnemyNum + 1; j++)
            {
                enemyObjs[i, j] = Instantiate(enemys[i], enemysParent).GetComponent<ty_Enemy>();
                if (j == maxEnemyNum)
                {
                    enemyObjs[i, j].isBoss = true;
                    enemyObjs[i,j].transform.localScale = new Vector3(2,2,1);
                }
            }
        }
        
        NextFloor();
    }

    void CheckSaveData()
    {
        string json = PlayerPrefs.GetString("FloorData", JsonUtility.ToJson(new SaveData(1, 1)));
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);
        MaxFloor = saveData.floorMax;
        floor = saveData.floor - 1;

        string s = PlayerPrefs.GetString("PlayTimeData", System.TimeSpan.Zero.ToString());
        preTimeSpan = System.TimeSpan.Parse(s);
    }

    public void NextFloor()
    {
        CleanFloor();
        //フロアが変わるときにプレイ時間を評価する。リアルタイムに見せるため。
        PlayTimeSpan = System.DateTime.Now - startDateTime;
        Floor += 1;
        SpawnEnemy();
        if (audioSource.enabled) audioSource.Play();
    }

    bool IsBonus(int minute)
    {
        return (int)(PlayTimeSpan.TotalMinutes) >= minute;
    }

    void SpawnEnemy()
    {
        int num;
        DeleteAllEnemy(); //敵が残っていたら消す 
        num = Random.Range(minEnemyNum, maxEnemyNum);
        for (int j = 0; j < num; j++) { 
            enemyObjs[GetEnemyKindsNum(Floor), j].SetStatus();
        }

        if (Floor % 10 == 0)
        {
            tyLog.enemyHp.gameObject.SetActive(true);
            enemyObjs[GetBossKindsNum(Floor), maxEnemyNum].SetStatus();
        }
        else tyLog.enemyHp.gameObject.SetActive(false);
    }

    public void Save()
    {
        SaveData data = new SaveData(maxFloor, floor);
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("FloorData", json);
        PlayerPrefs.SetString("PlayTimeData", 
            (System.DateTime.Now - startDateTime + preTimeSpan).ToString()
        );

        puzzle.Save();
        tyHero.Save();

        PlayerPrefs.Save(); //一応呼ぶ。

        Debug.Log(PlayerPrefs.GetString("HeroData"));
        Debug.Log(PlayerPrefs.GetString("FloorData"));
        Debug.Log(PlayerPrefs.GetString("PuzzleData"));
        Debug.Log(PlayerPrefs.GetString("PlayTimeData"));
    }

    public IEnumerator ResetFloor(){
        maskImage.SetActive(true); //maskImageのFixedUpdateで処理が行われる。
        yield return new WaitForSeconds(timeFadeout);
        Floor = (int)entryFloorSlider.value - 1;
        //puzzle.Init();
        puzzle.MoveableTimes = initMoveableTimes;
        tyHero.Init();
        tyLog.ResetConsole();
        NextFloor();
    }

    void DeleteAllEnemy()
    {
        for (int i = 0; i < enemysParent.childCount; i++)
            enemysParent.GetChild(i).gameObject.SetActive(false);
    }

    void CleanFloor()
    {
        for (int i = 0; i < dropItemParent.childCount; i++)
        {
            GameObject obj = dropItemParent.GetChild(i).gameObject;
            Destroy(obj);
        }
    }
}
