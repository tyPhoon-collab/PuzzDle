using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DropItem : MonoBehaviour {
    private void Start() {
        GameSystem.Functions.AddUpForce(gameObject);
    }
}

