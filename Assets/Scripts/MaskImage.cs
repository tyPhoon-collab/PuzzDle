using UnityEngine.UI;
using UnityEngine;

public class MaskImage : MonoBehaviour
{
    float alpha = 0;
    [SerializeField] Image maskImage;

    void FixedUpdate()
    {
        FadeOut();
    }

    void FadeOut()
    {
        alpha += Time.deltaTime / GameSystem.Functions.timeFadeout;
        maskImage.color = new Color(0, 0, 0, alpha); //F‚Í•‚É‚µ‚¿‚á‚¤B
        if (alpha >= 1)
        {
            alpha = 0;
            maskImage.color = new Color(0, 0, 0, 0);
            gameObject.SetActive(false);
        }
    }
}
