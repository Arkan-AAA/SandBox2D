using UnityEngine;

public class ExitDoor : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            LevelGenerator.Instance?.NextFloor();
        }
    }
}