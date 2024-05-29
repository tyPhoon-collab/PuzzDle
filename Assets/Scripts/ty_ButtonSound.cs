using UnityEngine;
using UnityEngine.EventSystems;

public class ty_ButtonSound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] AudioSource audioSource;
    // Start is called before the first frame update
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioSource.enabled) audioSource.Play();
    }
}
