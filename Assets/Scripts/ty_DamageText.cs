using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ty_DamageText : MonoBehaviour {
    float time;
    Rigidbody2D rb;
    Text textCmp;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        textCmp = GetComponent<Text>();
        gameObject.SetActive(false);
    }

    public void Init(string text, Vector3 pos, Color col){
        gameObject.SetActive(true);
        rb.velocity= Vector2.zero;
        rb.AddForce(Vector3.up*100);
        time = 0;
        textCmp.text = text;
        textCmp.color = col;
        transform.position = pos;
    }

    private void FixedUpdate() {
        time += Time.deltaTime;
        if (time >= GameSystem.Functions.timeDrawDamage) gameObject.SetActive(false);
    }
}