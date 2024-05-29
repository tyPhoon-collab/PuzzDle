using AccsEnum;
using GameSystem;
using ItemsEnum;
using System.Collections;
using UnityEngine;
using static GameSystem.Functions;

public class ty_Hero : ty_Alives
{
    public Animator animator;
    public GameDirector directorScirpt;

    readonly LevelManager lvManager = new LevelManager();
    readonly AccsManager accsManager = new AccsManager();
    readonly EquipManager eqManager = new EquipManager();

    float speed;
    public Vector3 initPos;
    bool isHit = false;
    public bool isAlive = true;

    int lv;
    int exp;
    int hunger = 0;
    int hungerMax = 0;

    int enemyDestroyNum = 0;

    private int money;
    private int crystal;

    int nowPlayingSoundNum = 0;


    public ty_Item tyItem;

    public AudioClip getCoinSound;

    AudioClip playSound;

    //親クラスの関数のoverrideをする。
    protected override void OnHpChanged() {
        tyLog.UpdateHp(Hp);
    }
    protected override void OnHpMaxChanged() {
        tyLog.UpdateHpMax(HpMax);
    }
    protected override void OnAtkChanged() {
        tyLog.UpdateAtk(Atk);
    }
    protected override void OnDefChanged() {
        tyLog.UpdateDef(Def);
    }
    protected override void Dead() {
        if (!isAlive) return;
        isAlive = false;
        isHit = false;
        Hunger = 0;
        StopCoroutine("HungerCheck");
        animator.SetTrigger("Death");
        StartCoroutine(directorScirpt.ResetFloor());
    }
    //プロパティー
    public int Hunger {
        get { return hunger; }
        set {
            hunger = Mathf.Clamp(value, 0, HungerMax);
            tyLog.UpdateHunger(hunger);
        }
    }
    public int HungerMax {
        get { return hungerMax; }
        set {
            hungerMax = value;
            if (hungerMax < 0) hungerMax = 0;
            tyLog.UpdateHungerMax(hungerMax);
        }
    }
    public int Money {
        get { return money; }
        set {
            money = Mathf.Clamp(value, 0, value);
            tyLog.UpdateCoin(money);
        }
    }
    public int Crystal {
        get { return crystal; }
        set {
            crystal = value;
            tyLog.UpdateCrystal(crystal);
        }
    }

    public int Lv {
        get { return lv; }
        set {
            lv = value;
            SetStatus();
            tyLog.UpdateLv(value);
        }
    }
    public int Exp {
        get { return exp; }
        set {
            exp = value;
            if (exp == 0)
            {
                lv = 0;
                lvManager.Init(5f);
            }
            
            while (lvManager.IsNextLv(exp, Lv)) Lv++;
            tyLog.UpdateExp(value);
        }
    }
    public int CoinWeight
    {
        get { return accsManager.CoinWeight; }
    }
    public int DropWeight
    {
        get { return accsManager.DropWeight; }
    }

    AudioClip PlaySound
    {
        set
        {
            playSound = value;
            if (!IsSound) return;
            StartCoroutine(PlaySoundCoroutine());
        }
    }

    IEnumerator PlaySoundCoroutine ()
    {
        if (nowPlayingSoundNum >= canPlaySoundNum) yield break;
        AudioSource.PlayClipAtPoint(playSound, Camera.main.transform.position, 0.5f);
        nowPlayingSoundNum++;
        yield return new WaitForSecondsRealtime(0.5f);
        nowPlayingSoundNum--;
    }
    public int EnemyDestroyNum
    {
        get { return enemyDestroyNum; }
        set
        {
            enemyDestroyNum = value;
            tyLog.UpdateEnemyDestroyNum(value);
        }
    }
    public bool IsMove { get; set; }
    public bool IsSound { get; set; }
    public int Floor { get; set; }

    private void Start() {
        IsSound = true;
        isDrawRecover = true;
        string json = PlayerPrefs.GetString("HeroData", JsonUtility.ToJson(initSaveData));
        var saveData = JsonUtility.FromJson<HeroSaveData>(json);
        initPos = transform.position;
        damageColor = Color.red;
        
        tyItem.ItemNum = saveData.items;

        Money = saveData.coin;
        Crystal = saveData.crystal;
        EnemyDestroyNum = saveData.enemyDestroyNum;

        EquipLvUp(Items.SWORD, saveData.swordLv);
        EquipLvUp(Items.SHIELD, saveData.shiledLv);

        for (int i = 0; i < saveData.accs.Length; i++)
            if (saveData.accs[i]) SetAccAcquire((Accs)i, false);

        Init(saveData.exp);

        Hp = saveData.hp;
        Hunger = saveData.hunger;
    }

    public void Init(int exp = 0) {
        isAlive = true;

        Exp = exp;
        animator.Play("Idle");
        animator.SetInteger("AnimState", 1);
        
        transform.position = initPos;

        IsMove = true;
        StartCoroutine("HungerCheck");

        if (exp == 0) 
        { 
            Hp = HpMax;
            Hunger = HungerMax;
        }
    }

    public void SetAccAcquire(Accs acc, bool isPopUp = true) {
        bool isFirst = accsManager.SetAcquire(acc);
        if (!isFirst) return;
        if (isPopUp) tyLog.DrawPopUp(acc);
        tyLog.ChangeToggle(acc, true);
        SetStatus();
    }

    public void EquipLvUp(Items item, int value = 1) {
        if (item == Items.SWORD) {
            eqManager.SwordLv += value;
            value = eqManager.SwordLv;
        } else if (item == Items.SHIELD) {
            eqManager.ShiledLv += value;
            value = eqManager.ShiledLv;
        }
        tyLog.UpdateEquipLv(item, value);
        SetStatus();
    }

    void SetStatus() {
        HeroStatus s = GetHeroStatus(Lv, accsManager, eqManager);
        HpMax = s.HpMax;
        HungerMax = s.HungerMax;
        Atk = s.Atk;
        Def = s.Def;
        speed = s.Speed;
    }

    void Update()
    {
        if (isAlive) AliveMove();
    }

    void AliveMove()
    {
        if (!isHit && IsMove && !IsAnimating("Attack1")) {
            transform.position += Vector3.right * speed * Time.deltaTime;
        }
        if (transform.position.x >= limitPosX) {
            transform.position = initPos;
            directorScirpt.NextFloor();
        }
    }


    bool IsAnimating(string name) {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(name);
    }

    IEnumerator Attack()
    {
        while (true)
        {
            if (isAlive) animator.SetTrigger("Attack1");
            IsMove = false;
            yield return new WaitForSeconds(1f);
            IsMove = true;
        }
    }
    public void HitFunc()
    {
        if (!isHit && isAlive)
        {
            StartCoroutine("Attack");
            isHit = true;
            if (isAlive) animator.SetInteger("AnimState", 0);
        }
    }

    public void DetouchedFunc()
    {
        StopCoroutine("Attack");
        IsMove = true;
        isHit = false;
        if (isAlive) animator.SetInteger("AnimState", 1);
    }
    public void Save()
    {
        var data = new HeroSaveData(
            tyItem.ItemNum, accsManager.IsAcquired, 
            Hunger, Hp, Exp, Money, Crystal, 
            eqManager.SwordLv, eqManager.ShiledLv, EnemyDestroyNum
        );
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("HeroData", json);
    }
    IEnumerator HungerCheck()
    {
        while (isAlive) {
            yield return new WaitForSeconds(5f);
            if (Hunger > 0) {
                Hunger -= 1;
                if (Hp < HpMax) Hp += Lv;
            } else {
                Hp -= 10;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject obj = collision.gameObject;
        switch (obj.tag)
        {
            case "Coin": 
                AddCoin(obj); break;
            case "Crystal": 
                AddCrystal(obj); break;
            case "TreasureBox": 
                AddTreasureBox(obj); break;
        }
    }

    public void AddCoin(GameObject obj) {
        PlaySound = getCoinSound;
        Money++;
        Destroy(obj);
    }
    public void AddCrystal(GameObject obj = null) {
        Crystal++;
        if (obj != null) Destroy(obj);
    }
    public void AddTreasureBox(GameObject obj = null) {
        Items item = GetWeightedRandomItem();
        tyItem.AddItem(item);
        if (obj != null) Destroy(obj);
    }

    public void AddExp(int exp) =>
        Exp += exp * accsManager.ExpWeight;
    
}

