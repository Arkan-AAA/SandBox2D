using UnityEngine;

public class DestructibleBarrel : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Sword>()) {
            Destroy(gameObject);
        }
    }
}
