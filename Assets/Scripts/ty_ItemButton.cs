using System.Collections;
using UnityEngine;

public class ty_ItemButton : ty_Button {
    private int strength = 1;
    private int num;
    
    protected override void OnLabalChanged(){
        text.text = GetText();
    }

    protected override void OnLongPress() => Use();

    public int Num {
        get { return num; }
        set {
            num = value;
            
            if (value <= 0) {
                gameObject.SetActive(false);
                return;
            } 

            if (value > GameSystem.Functions.maxItemNum) return;
            text.text = GetText();
        }
    }

    public int Strength{
        set { this.strength = value; }
    }

    public void Use(){
        TyItem.ItemEffect(Item, strength);
        Num--; //数を減らす。
    }

    string GetText(){
        return Label + " [" + Num.ToString() + "] ";
    }
}