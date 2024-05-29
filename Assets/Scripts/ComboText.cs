using UnityEngine;
using UnityEngine.UI;

public class ComboText : MonoBehaviour
{
    float time = 0;
    const int colorNum = 2;
    [SerializeField] Color[] colors = new Color[colorNum] { 
        Color.white,
        Color.black
    };
    [SerializeField] Text textCmp;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        time += Time.deltaTime;
        textCmp.color = colors[(int)(time * 10) % colorNum];
        if (time >= GameSystem.Functions.timeDrawCombo) gameObject.SetActive(false);
    }

    public void Init(Vector3 pos)
    {
        gameObject.SetActive(true);
        time = 0;
        transform.position = pos;
    }
}