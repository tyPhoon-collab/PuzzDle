using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

using ItemsEnum;

public abstract class ty_Button : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Items item;
    string label;
    
    float longPressTime = GameSystem.Functions.buttonLongPressRealTime;
    float interval = GameSystem.Functions.buttonIntervalRealTime;

    public Text text;

    protected IEnumerator onLongPress;

    private void Awake() {
        TyItem = GameObject.FindGameObjectWithTag("GameController").GetComponent<ty_Item>();
    }

    protected ty_Item TyItem {get; set;}
    protected ItemInfo ItemInfo {get; set;}
    protected string Label{
        get { return label; }
        set {
            label = value;
            OnLabalChanged();
        }
    }

    public Items Item{
        get { return item; }
        set { 
            item = value;
            ItemInfo = item.GetItemInfo(); 
            Label = ItemInfo.Name;
        }
    }

    IEnumerator LongPressCoroutine()
    {
        yield return new WaitForSecondsRealtime(longPressTime);

        while (true)
        {
            OnLongPress();
            yield return new WaitForSecondsRealtime(interval);
        }
    }

    protected abstract void OnLongPress();
    protected abstract void OnLabalChanged();

    public void OnPointerDown(PointerEventData eventData)
    {
        if (onLongPress != null) return;
        onLongPress = LongPressCoroutine();
        StartCoroutine(onLongPress);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopLongPressCoroutine();
    }

    protected void StopLongPressCoroutine()
    {
        if (onLongPress == null) return;
        StopCoroutine(onLongPress);
        onLongPress = null;
    }
}