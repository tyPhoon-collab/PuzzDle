using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Utils : MonoBehaviour
{
    /// <summary>
    /// シーンをチェンジする。シーンはSingleで引数はindexとする
    /// </summary>
    /// <param name="index"></param>
    public void ChangeScene(int index){
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(index);
        //このゲームはTimescaleを変更する。Timescaleはシーンチェンジしても保持されるっぽいので、ChangeSceneしたときにTimescaleを1に戻す。
    }

    /// <summary>
    /// Time.timeScaleを変更する。
    /// </summary>
    float[] timeScales = new float[3] {1, 3, 5};
    int index = 0;
    [SerializeField] float pitchWeight = 0.13f;
    [SerializeField] Text textNowTimeScale;
    [SerializeField] bool isChangeBgmPitch = true;
    [SerializeField] AudioSource audioSource;
    
    public void ChangeTimeScale(){
        index = (index + 1) % timeScales.Length;
        Time.timeScale = timeScales[index];

        textNowTimeScale.text = "×" + Time.timeScale.ToString();
        if (isChangeBgmPitch)
        {
            audioSource.pitch = 1 + pitchWeight * index;
        }
        else audioSource.pitch = 1;
    }

    //int _index;

    //private void OnApplicationFocus(bool focus)
    //{
    //    if (focus)
    //    {
    //        index = _index - 1;
    //    }
    //    else
    //    {
    //        _index = index;
    //        index = -1;
    //    }
    //    ChangeTimeScale();
    //}

    /// <summary>
    /// SliderとInputFieldの値をシンクロする
    /// </summary>
    public InputField inputField;
    public Slider slider;

    public void SyncValueOnChangedSlider(){
        inputField.text = slider.value.ToString();
    }

    public void SyncValueOnChangedInput(){
        if (inputField.text == "") inputField.text = "1";
        slider.value = Convert.ToInt32(inputField.text);
        SyncValueOnChangedSlider(); //規定値を超えるときに呼ぶべきだが、面倒なので、常に呼ぶことにする。
    }

    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("JsonData was Deleted");
    }

    public void CallBigPopUp(GameObject popUp)
    {
        popUp.SetActive(true);
    }
}
