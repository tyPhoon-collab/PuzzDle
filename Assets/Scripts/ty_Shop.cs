using ItemsEnum;
using UnityEngine;
using UnityEngine.UI;

public class ty_Shop : MonoBehaviour {
    public Button button;
    public RectTransform content;

    private void Awake() {
        for (int n = 0; n < (int)Items.MAX; n++)
        {
            Items item = (Items)n;
            var script = Instantiate(button, content).GetComponent<ty_ShopButton>();
            script.Item = item;
        }
    }
}