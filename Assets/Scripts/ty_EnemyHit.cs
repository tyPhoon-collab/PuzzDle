using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ty_EnemyHit : MonoBehaviour
{
    ty_Enemy script;

    void Start()
    {
        script = gameObject.GetComponentInParent<ty_Enemy>();
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player"){
            script.IsHit = true;
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player"){
            script.IsHit = false;
        }
    }
}
