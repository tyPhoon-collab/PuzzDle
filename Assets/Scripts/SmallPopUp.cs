using UnityEngine;
using UnityEngine.UI;

public class SmallPopUp : PopUp {
    [SerializeField] Text textCmp;

    public override void Init(string text)
    {
        base.Init();
        textCmp.text = text;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (time >= GameSystem.Functions.timeDrawPopUp) gameObject.SetActive(false);
    }
}