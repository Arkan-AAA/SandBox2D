using UnityEngine;

public enum DoorDirection { Up, Down, Left, Right }

public class DoorPoint : MonoBehaviour {
    public DoorDirection direction;

    [HideInInspector] public bool isConnected;
    [HideInInspector] public DoorPoint connectedTo;

    public DoorDirection Opposite() {
        return direction switch {
            DoorDirection.Up => DoorDirection.Down,
            DoorDirection.Down => DoorDirection.Up,
            DoorDirection.Left => DoorDirection.Right,
            DoorDirection.Right => DoorDirection.Left,
            _ => DoorDirection.Up
        };
    }

    void OnDrawGizmos() {
        Gizmos.color = isConnected ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.4f);

        Gizmos.color = Color.yellow;
        Vector3 dir = direction switch {
            DoorDirection.Up => Vector3.up,
            DoorDirection.Down => Vector3.down,
            DoorDirection.Left => Vector3.left,
            DoorDirection.Right => Vector3.right,
            _ => Vector3.up
        };
        Gizmos.DrawRay(transform.position, dir * 0.6f);
    }
}