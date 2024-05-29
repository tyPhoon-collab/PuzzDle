using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]

public class FanEffect : MonoBehaviour {
    float limitPosX = GameSystem.Functions.limitPosX;
    int times = 5;

    private void FixedUpdate() {
        transform.position += transform.right * Time.deltaTime * 10;
        if (transform.position.x >= limitPosX) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) {
            ty_Enemy enemy = collision.GetComponent<ty_Enemy>();
            enemy.Hp -= enemy.GetHeroAtk();
            if(--times == 0) Destroy(gameObject);
        }
    }
}