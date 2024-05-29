using UnityEngine;

public class BigPopUp : PopUp
{
    //����PopUp���\������Ă���Ԃ́A�Q�[���̐i�s���~�߂�BTest
    float timeScale = 1;
    public bool isStopTime = true;


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (time >= popTime)
        {
            if (isStopTime) Time.timeScale = 0; //�������������Ǔ����Ȃ����B�܂�����Ă݂�B
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
