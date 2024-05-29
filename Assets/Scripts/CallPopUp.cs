using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallPopUp : MonoBehaviour
{
    [SerializeField] GameObject popUp;

    private void OnDisable()
    {
        if (popUp != null)
            popUp.SetActive(true);
    }
}
