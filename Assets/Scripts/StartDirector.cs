using UnityEngine;
using UnityEngine.UI;

public class StartDirector : MonoBehaviour {
    [SerializeField] Button b_continue;
    [SerializeField] Text t_version;
    private void Start() {
        if (PlayerPrefs.HasKey("HeroData"))
            b_continue.gameObject.SetActive(true);
        else 
            b_continue.gameObject.SetActive(false);

        t_version.text = "Ver. " + Application.version;
    }
}