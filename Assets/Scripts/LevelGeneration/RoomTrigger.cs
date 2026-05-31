using UnityEngine;

public class RoomTrigger : MonoBehaviour {
    private RoomInstance _room;
    private MobSpawner _spawner;
    private bool _triggered = false;

    public void Initialize(RoomInstance room, MobSpawner spawner) {
        _room = room;
        _spawner = spawner;
        Debug.Log($"RoomTrigger initialized for {room?.name}");
    }

    void OnTriggerEnter2D(Collider2D other) {
        Debug.Log($"OnTriggerEnter2D called on {_room?.name}, other: {other.name}");

        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        Debug.Log($"Player entered room: {_room?.name}");

        _triggered = true;
        _spawner?.OnPlayerEnteredRoom(_room);
        Destroy(gameObject, 0.1f);
    }
}