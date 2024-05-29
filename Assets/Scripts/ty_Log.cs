using UnityEngine;
using UnityEngine.UI;
using MessagesEnum;
using StatusEnum;
using AccsEnum;
using ItemsEnum;

using GameSystem;

/// <summary>
/// Obj = Object  
/// Cmp = Component
/// EventSystemでやる手もあるが。。。
/// どのような処理が行われるか分からない以上、インスペクター上で管理するのはちょっと面倒かもしれない。
/// </summary>

public class ty_Log : MonoBehaviour {
    public Transform parent;
    public Transform comboTransform;
    
    public Text textDamage;
    public Text textFloor;
    public Text textMaxFloor;
    public Text textCoin;
    public Text textCrystal;
    public Text textMoveable;
    public Text textAdditionalMoveable;
    public Text textStatus;
    public Text textLv;
    public Text textExp;
    public Text textAtk;
    public Text textDef;
    public Text textAtkLv;
    public Text textDefLv;
    public Text textHp;
    public Text textHpMax;
    public Text textHunger;
    public Text textHungerMax;
    public Text textPlayTime;
    public Text textPieceSum;
    public Slider sliderHp;
    public Slider sliderHunger;
    public Slider enemyHp;

    [SerializeField] Text textAccName;
    [SerializeField] Text textAccSummary;
    [SerializeField] Text textEnemyDestroyNum;
    [SerializeField] GameObject moveableAttentionImage;

    [SerializeField] Text[] consoleTexts = new Text[5];
    int consoleHead = 0;

    //暗黙の了解として、列挙体と同じ順番にせっとする。
    [SerializeField] Toggle[] toggleAccs = new Toggle[(int)Accs.MAX];

    public Text debug;

    // DrawDamage用のオブジェクトを先に生成しておく。
    int damageTextObjsIndex = 0;
    const int textNum = 8;
    ty_DamageText[] damageTextObjs = new ty_DamageText[textNum]; //足りるかな。

    //Recover用
    int recoverTextObjsIndex = 0;
    ty_DamageText[] recoverTextObjs = new ty_DamageText[textNum]; //足りるかな。

    const int maxComboNum = 20; 
    ComboText[] comboTextObjs = new ComboText[maxComboNum];

    [SerializeField] SmallPopUp popUp;
    [SerializeField] GameObject comboTextPrefab;

    //文字リテラルの保持
    const string comboStr = "Combo ×";
    const string floorStr = " F";

    private void Awake() {
        ResetConsole();
        
        for (int i = 0; i < textNum; i++)
        {
            damageTextObjs[i] = Instantiate(textDamage, parent).GetComponent<ty_DamageText>();
            recoverTextObjs[i] = Instantiate(textDamage, parent).GetComponent<ty_DamageText>();
        }

        for (int i = 0; i < maxComboNum; i++)
        {
            GameObject obj = Instantiate(comboTextPrefab, comboTransform);
            comboTextObjs[i] = obj.GetComponent<ComboText>();
            obj.GetComponent<Text>().text = $"{comboStr}{(i + 1).ToString()}";
        }
    }

    public void DrawDamage(int damage, Vector3 pos, Color col) =>
        damageTextObjs[damageTextObjsIndex++ % textNum].Init(damage.ToString(), pos + Vector3.up * 0.5f, col);

    public void DrawRecover(int recover, Vector3 pos, Color col) => //実際、いらない。
        recoverTextObjs[recoverTextObjsIndex++ % textNum].Init(recover.ToString(), pos + Vector3.up * 0.5f, col);

    public void DrawCombo(Vector3 pos, int combo) =>
        comboTextObjs[(combo - 1) % maxComboNum].Init(pos);

    public void DrawPopUp(Accs acc) => 
        popUp.Init(Messages.Get_Accs.GetMessage(acc.GetName()));

    public void DrawPopUp(Items item) =>
        popUp.Init(Messages.Get_Item.GetMessage(item.GetItemInfo().Name));

    public void DrawAccsInfo(int accIndex)
    {
        Accs acc = (Accs)accIndex;
        textAccName.text = acc.GetName();
        textAccSummary.text = acc.GetSummary();
    }

    public void UpdateEnemyHpBar(int hpMax, int hp) {
        enemyHp.maxValue = hpMax;
        enemyHp.value = hp;
    }

    public void UpdateHp(int value){
        int diff = value - (int)sliderHp.value;
        sliderHp.value = value;
        textHp.text = value.ToString();
        if (diff > 0) AddTextToConsole(Messages.Recover, diff);
        if (value <= 0) AddTextToConsole(Messages.Dead);
    }

    public void UpdateHpMax(int value){
        sliderHp.maxValue = value;
        textHpMax.text = "/ " + value.ToString();
    }

    public void UpdateHunger(int value){
        int diff = value - (int)sliderHunger.value;
        sliderHunger.value = value;
        textHunger.text = value.ToString();
        if (diff == 0) return;
        if (diff < 0) AddTextToConsole(Messages.Hunger, -diff);
        else AddTextToConsole(Messages.Eat, diff);
    }
    public void UpdateHungerMax(int value){
        sliderHunger.maxValue = value;
        textHungerMax.text = "/ " + value.ToString();
    }
    
    public void UpdateGameStatus(GameStatus value){
        textStatus.text = value.GetName();
        if (value == GameStatus.INIT) ResetConsole();
    }
    
    public void UpdateFloor(int value) {
        textFloor.text = $"{value.ToString()}{floorStr}";
        EnemyStatus s = Functions.GetEnemyStatus(value);
        debug.text = $"H:{s.HpMax}, A:{s.Atk}, D:{s.Def}, E:{s.Exp}, C:{s.Coin}";
    } 

    public void UpdateMaxFloor(int value) => 
        textMaxFloor.text = $"{value.ToString()}{floorStr}";
    
    public void UpdateCoin(int value) => 
        textCoin.text = ": " + value.ToString("N0");
    
    public void UpdateCrystal(int value) => 
        textCrystal.text = ": " + value.ToString("N0");

    public void UpdateMoveable(int value)
    {
        textMoveable.text = value.ToString();
        if (value > 0 && moveableAttentionImage.activeSelf) 
            moveableAttentionImage.SetActive(false);
    }
    public void UpdateAdditionalMoveable(int value)
    {
        textAdditionalMoveable.text = "+" + value.ToString();
        if (value > 0 && moveableAttentionImage.activeSelf) 
            moveableAttentionImage.SetActive(false);
    }

    public void DrawMoveableAttentionImage() =>
        moveableAttentionImage.SetActive(true);


    public void UpdateLv(int value) => 
        textLv.text = "Lv  : " + value.ToString();
    
    public void UpdateExp(int value) => 
        textExp.text = "Exp : " + value.ToString();
    
    public void UpdateAtk(int value) => 
        textAtk.text = value.ToString();
    
    public void UpdateDef(int value) => 
        textDef.text = value.ToString();

    public void UpdatePlayTime(string value) => 
        textPlayTime.text = value;

    public void UpdatePieceSum(int value) => 
        textPieceSum.text = value.ToString();

    public void UpdateEnemyDestroyNum(int value) =>
        textEnemyDestroyNum.text = value.ToString();

    public void UpdateEquipLv(Items item, int value){
        switch (item)
        {
            case Items.SWORD:
                textAtkLv.text = "Lv." + value.ToString();
                AddTextToConsole(Messages.SwordLvUp, value);
                break;
            case Items.SHIELD:
                textDefLv.text = "Lv." + value.ToString();
                AddTextToConsole(Messages.ShiledLvUp, value);
                break;
        }
    }
    public void ChangeToggle(Accs acc, bool value){
        if ((int)acc >= toggleAccs.Length)
        {
            Debug.Log("ty_Logにtoggleが指定されていません。");
            return;
        }
        toggleAccs[(int)acc].isOn = value;
    }

    public void ResetConsole()
    {
        for (int i = 0; i < consoleTexts.Length; i++)
        {
            consoleTexts[i].text = "";
        }
    }

    void AddTextToConsole(Messages mess, int value = 0)
    {
        consoleTexts[consoleHead].transform.SetAsLastSibling();
        consoleTexts[consoleHead]
            .text = mess.GetMessage(value);

        consoleHead = (consoleHead + 1) % consoleTexts.Length;
    }
}