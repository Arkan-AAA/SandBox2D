using UnityEngine;

public class RoomTrigger : MonoBehaviour {
    private RoomInstance _room;
    private MobSpawner _spawner;
    private bool _triggered = false;

    public void Initialize(RoomInstance room, MobSpawner spawner) {
        _room = room;
        _spawner = spawner;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        _spawner?.OnPlayerEnteredRoom(_room);
        Destroy(gameObject, 0.1f);
    }
}