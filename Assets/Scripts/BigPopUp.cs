using UnityEngine;

public class BigPopUp : PopUp
{
    //このPopUpが表示されている間は、ゲームの進行を止める。Test
    float timeScale = 1;
    public bool isStopTime = true;


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (time >= popTime)
        {
            if (isStopTime) Time.timeScale = 0; //動きそうだけど動かなそう。まぁやってみる。
        }
    }

    private void OnEnable()
    {
        base.Init();
        timeScale = Time.timeScale;
    }
    private void OnDisable()
    {
        Time.timeScale = timeScale;
    }

}
