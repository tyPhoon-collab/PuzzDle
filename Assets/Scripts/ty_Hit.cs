using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ty_Hit : MonoBehaviour
{
    ty_Hero script;

    void Start()
    {
        script = gameObject.GetComponentInParent<ty_Hero>();
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            script.HitFunc();
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            script.DetouchedFunc();
        }
    }
}
