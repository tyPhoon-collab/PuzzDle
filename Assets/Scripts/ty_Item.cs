using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using ItemsEnum;

public class ty_Item : MonoBehaviour {
    public Button button;
    public RectTransform content;
    public ty_Hero tyHero;
    public ty_Log tyLog;
    public Puzzle puzzle;

    //勇者がアイテム使用時に止まる時間
    float stopTime = 0.5f;

    public GameObject effectBlackhole;
    public GameObject effectFan;
    public GameObject effectFloat;
    public GameObject effectEarthquake;
    public GameObject effectExplosion1;
    public GameObject effectExplosion2;

    GameObject[] enemys;

    ty_ItemButton[] itemObj = new ty_ItemButton[(int)Items.MAX];
    int[] itemNum = new int[(int)Items.MAX]; //セーブのときにアイテムの数を保持しておく必要がある。正直保持しなくても良い。

    public int[] ItemNum{
        get { return itemNum; }
        set { 
            for (int i = 0; i < (int)Items.MAX; i++) {
                if (value.Length <= i)
                {
                    Debug.Log("セーブデータとゲームデータが一致しません。機能が新しく追加された可能性があります。");
                    itemNum[i] = 0;
                    itemObj[i].Num = 0;
                    continue;
                }
                itemNum[i] = value[i];
                itemObj[i].Num = itemNum[i];
            }

        }
    }

    private void Awake() {
        //ボタンを全種類生成しておく。GC Allocを減らすのと、参照を簡単にするため
        for (int i = 0; i < (int)Items.MAX; i++)
        {
            itemObj[i] = Instantiate(button, content).GetComponent<ty_ItemButton>();
            itemObj[i].Item = (Items)i;
        }
    }

    public void AddItem(Items item){
        int i = (int)item;
        tyLog.DrawPopUp(item);
        if (itemNum[i] >= 0) itemObj[i].gameObject.SetActive(true);
        itemNum[i] = ++itemObj[i].Num;
    }

    public void ItemEffect(Items item, int strength){
        //アイテムが使われたら場にいる敵を取得します。enemysはグローバル変数にしました。
        enemys = GameObject.FindGameObjectsWithTag("Enemy");
        //数を減らす
        itemNum[(int)item]--;
        //StartCoroutine(PlayerControl());

        switch(item){
            case Items.POTION_HP:
                tyHero.Hp += 10 * strength;  break;
            case Items.FOOD:
                tyHero.Hunger += 25 * strength; break;
            case Items.SWORD: 
                tyHero.EquipLvUp(item); break;
            case Items.SHIELD: 
                tyHero.EquipLvUp(item); break;
            case Items.ADD_MOVEABLE: 
                puzzle.AdditionalMoveableTimes += 10; break;

            case Items.GRAVITY   : GravityEffect();                     break;
            case Items.EARTHQUAKE: EarthquakeEffect();                  break;
            case Items.FAN       : FanEffect();                         break;
            case Items.BLACKHOLE : BlackholeEffect();                   break;
            case Items.EXPLOSION : StartCoroutine(ExplosionEffect());   break;
        }
    }

    /// <summary>
    /// アイテムの使用によるバグをなくすために勇者を一時停止する。
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayerControl(){
        tyHero.IsMove = false;
        yield return new WaitForSeconds(stopTime);
        tyHero.IsMove = true;
    }
    
    public void GravityEffect(){
        StartCoroutine(PlayerControl());

        foreach (GameObject enemy in enemys){
            enemy.GetComponent<ty_Enemy>().ChangeGravity(-1f);
            Instantiate(effectFloat, enemy.transform);
        }
    }

    public void EarthquakeEffect(){
        StartCoroutine(PlayerControl());
        int damage = tyHero.Atk / 2 + 1;
        foreach (GameObject enemy in enemys){
            enemy.GetComponent<ty_Enemy>().Hp -= damage;
            Instantiate(effectEarthquake, enemy.transform.position, Quaternion.identity);
        }
    }

    public void FanEffect(){
        StartCoroutine(PlayerControl());

        Instantiate(
            effectFan,
            new Vector3(tyHero.transform.position.x,2.8f,0),
            Quaternion.identity
        );
        tyHero.animator.SetTrigger("Attack1");
    }

    public void BlackholeEffect(){
        StartCoroutine(PlayerControl());

        Instantiate(
            effectBlackhole,
            new Vector3(14.5f, 2.4f, 0),
            Quaternion.identity
        );
        foreach (GameObject enemy in enemys){
            enemy.GetComponent<ty_Enemy>().isBlackhole = true;
        }
    }

    IEnumerator ExplosionEffect()
    {
        StartCoroutine(PlayerControl());

        GameObject effect = Instantiate(
            effectExplosion1,
            new Vector3(14.5f, 2.4f, 0),
            Quaternion.identity
        );
        foreach (GameObject enemy in enemys){
            enemy.GetComponent<ty_Enemy>().isBlackhole = true;
        }

        yield return new WaitForSeconds(1.5f);
        
        enemys = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemys){
            enemy.GetComponent<ty_Enemy>().AddForce(
                new Vector3(
                    Random.Range(-50,50),
                    Random.Range(300,350),
                    0
                )
            );
            enemy.GetComponent<ty_Enemy>().IsTrigger = true;
        }
        Destroy(effect);
        Instantiate(
            effectExplosion2,
            new Vector3(14.5f, 3.0f, 0),
            Quaternion.identity
        );
    }
}
