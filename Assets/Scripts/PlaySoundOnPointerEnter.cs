using UnityEngine;
using UnityEngine.EventSystems;

public class PlaySoundOnPointerEnter : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] float volume = 0.5f;
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position, volume);
    }
}
