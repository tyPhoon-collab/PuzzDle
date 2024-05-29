using UnityEngine;

public class PopUp : MonoBehaviour
{
    protected float time = 0;
    protected const float popTime = 0.5f;

    Vector3 originalScale;
    [SerializeField] AudioSource audioSource;

    private void Awake()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    public virtual void Init(string text = null)
    {
        gameObject.SetActive(true);
        if (audioSource.enabled)
        {
            audioSource.Stop();
            audioSource.Play();
        }

        time = 0;
        transform.localScale = Vector3.zero;
    }

    protected virtual void FixedUpdate()
    {
        //LerpÇÃÇŸÇ§Ç™Ç¢Ç¢Ç©Ç‡ÇµÇÍÇ»Ç¢ÅBímÇÁÇ»Ç¢ÅB
        if (time <= popTime) transform.localScale = originalScale * time * (1 / popTime);
        time += Time.deltaTime;
    }
}
