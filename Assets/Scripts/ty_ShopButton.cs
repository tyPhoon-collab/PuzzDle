using UnityEngine.UI;
using UnityEngine;

using System.Collections;


public class ty_ShopButton : ty_Button {
    ty_Hero tyHero;
    GameObject popUp;
    [SerializeField] Text textValue;

    const string coinName = " G";
    const string crystalName = " C";

    protected override void OnLabalChanged(){
        text.text = Label;
        textValue.text = $"{ItemInfo.CoinValue}{coinName}, {ItemInfo.CrystalValue}{crystalName}";
    }

    protected override void OnLongPress() => Buy();
    private void Start() {
        tyHero = GameObject.FindGameObjectWithTag("Player").GetComponent<ty_Hero>();
        popUp = GameObject.FindGameObjectWithTag("PopUpParent").transform.Find("NoResourceCheck").gameObject;
    }

    public void Buy(){
        if (tyHero.Crystal < ItemInfo.CrystalValue || tyHero.Money < ItemInfo.CoinValue)
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);

            popUp.SetActive(true);
            return;
        }
        tyHero.Crystal -= ItemInfo.CrystalValue;
        tyHero.Money -= ItemInfo.CoinValue;
        
        TyItem.AddItem(Item);
    }
}