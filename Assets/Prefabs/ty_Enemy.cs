using AccsEnum;
using GameSystem;
using System.Collections;
using UnityEngine;
using static GameSystem.Functions;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]

public class ty_Enemy : ty_Alives
{
    Rigidbody2D rb;
    BoxCollider2D bc;
    ty_Hero tyHero;

    public GameObject coin;
    public GameObject crystal;
    public GameObject treasureBox;

    public Accs dropAcc = Accs.NONE;

    Transform dropItemParent;
    bool isHit = true;
    bool isTrigger = false;
    public bool isBlackhole = false;
    public bool isBoss = false;

    public int coinNum;
    public int expValue;
    public int addMoveableTimes;

    int coinWeight = 1;
    int dropWeight = 1;

    protected override void OnHpChanged()
    {
        if (!isBoss) return;
        tyLog.UpdateEnemyHpBar(HpMax, Hp);
    }

    public bool IsHit{
        get { return this.isHit; }
        set { 
            this.isHit = value; 
            if (this.isHit) StartCoroutine(EnemyAttack());
            else StopCoroutine(EnemyAttack());
        }
    }

    public bool IsTrigger{
        set {
            isTrigger = value;
            bc.isTrigger = value;
        }
    }

    //hpが0以下になったら呼ばれます。
    protected override void Dead() {
        tyGameObject.GetComponent<Puzzle>().MoveableTimes += addMoveableTimes;
        tyHero.AddExp(expValue);
        tyHero.EnemyDestroyNum++;
        
        IsTrigger = false;
        isBlackhole = false;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 1;

        DropItem();
        gameObject.SetActive(false);
    }

    protected override void Awake()
    {
        base.Awake();
        GetComponent<Animator>().speed = enemyAnimSpeed;
        dropItemParent = GameObject.FindGameObjectWithTag("DropItemParent").transform;
        tyHero = GameObject.FindGameObjectWithTag("Player").GetComponent<ty_Hero>();
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        gameObject.SetActive(false);
    }
        
    //様々な初期化。やや煩雑で分かりにくい。
    public void SetStatus(){
        gameObject.SetActive(true);

        int floor = tyHero.Floor;
        EnemyStatus s = GetEnemyStatus(floor);

        coinWeight = tyHero.CoinWeight;
        dropWeight = tyHero.DropWeight;

        if (isBoss)
        {
            transform.position = GetBossSpawnPos();
            SetBossStatus(s);

            if (Random.value <= accsDropRate)
                dropAcc = (Accs)Random.Range(0, (int)Accs.MAX);
        }
        else
        {
            transform.position = GetEnemySpawnPos();
            if (Random.value <= addMoveableTimesRate) addMoveableTimes = 1;
            else addMoveableTimes = 0;
        }

        Atk    = s.Atk;
        Def    = s.Def;
        HpMax  = s.HpMax;
        expValue = s.Exp;
        coinNum  = s.Coin;

        Hp = HpMax;
    }

    void SetBossStatus(EnemyStatus s)
    {
        s.Atk *= weightBossStatus;
        s.Def *= weightBossStatus;
        s.HpMax *= weightBossStatus;
        s.Coin *= weightBossCoin;
        s.Exp *= weightBossExp;
    }

    void DropItem(){
        for (int i = 0; i < coinNum * coinWeight; i++) Drop(coin);
        if (Random.value < crystalDropRate) Drop(crystal);
        if (Random.value < treasureBoxRate * dropWeight) Drop(treasureBox);
        if (dropAcc != Accs.NONE) tyHero.SetAccAcquire(dropAcc); 
    }

    void Update() {
        if (IsOutOfScreen()) Hp = 0;
    }

    void FixedUpdate() {
        if (isBlackhole) {
            AddForce(Vector3.right*20);
            if (transform.position.x >= effectLimitPosX) {
                isBlackhole = false;
                rb.velocity = Vector2.zero;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Sword") {
            //防御力が高いとダメージが0になる可能性がある。このとき、最低1ダメージは食らうようにする
            int damage = tyHero.Atk - Def;
            if(damage > 0) Hp -= damage;
            else Hp -= 1;
        }
    }
    IEnumerator EnemyAttack()
    {
        while(true){
            yield return new WaitForSeconds(1.0f);
            int damage = Atk - tyHero.Def;
            if (!tyHero.isAlive) yield break;
            
            if (damage > 0) tyHero.Hp -= damage;
            else tyHero.Hp -= 1;
            
            yield return new WaitForSeconds(1.0f);
        }
    }

    public void ChangeGravity(float gravityScale){
        rb.velocity = Vector2.zero;
        rb.gravityScale = gravityScale;
    }

    bool IsOutOfScreen(){
        float posY = transform.position.y;
        float posX = transform.position.x;
        return posY >= 3f + groundPosY || posY <= groundPosY - 0.5f || posX > limitPosX;
    }

    public void AddForce(Vector3 vec){
        rb.AddForce(vec);
    }

    void Drop(GameObject dropItem){
        Vector3 pos = new Vector3(transform.position.x + Random.Range(0, 0.5f), groundPosY + 0.5f, 0);
        if(pos.x > limitPosX) pos.x = Random.Range(16.5f,17.5f);
        Instantiate(dropItem, pos, Quaternion.identity, dropItemParent);
    }
    //暫定的においておく。正直いいコードとは言えない。
    public int GetHeroAtk()
    {
        return tyHero.Atk;
    }

    private void Reset() { //スクリプトをアタッチ、Reset時に呼ばれす。気にしなくていいです
        transform.position = Vector3.zero;
        transform.localScale = new Vector3(1,1,1);

        coin = Resources.Load<GameObject>("Coin");
        crystal = Resources.Load<GameObject>("Crystal");
        treasureBox = Resources.Load<GameObject>("TreasureBox");

        BoxCollider2D bc2 = GetComponent<BoxCollider2D>();
        bc2.offset = new Vector2(0f, 0.4f);
        bc2.size = new Vector2(0.6f, 0.8f);

        Rigidbody2D rb2  = GetComponent<Rigidbody2D>();
        rb2.freezeRotation = true;

        gameObject.tag = "Enemy";
        gameObject.layer = 7;

        if (gameObject.transform.childCount == 0) {
            GameObject obj = Instantiate(Resources.Load<GameObject>("Hit"), transform);
            if (obj.transform.parent == null) {
                DestroyImmediate(obj); //パッチです。後で原因を突き止めます。
                Debug.Log(
                    "Hitオブジェクトが正常にアタッチされませんでした。\n" +
                    "シーンにいる状態でこのスクリプトをアタッチし、プレハブにしてください。"
                );
            }
        }
    }

}
