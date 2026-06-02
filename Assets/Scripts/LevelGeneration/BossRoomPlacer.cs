using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BossRoomPlacer : MonoBehaviour {
    [Header("Настройки")]
    public LevelGenerator levelGenerator;
    public float corridorWidth = 2f; // ширина коридора

    [Header("Отладка")]
    public bool showDebugLogs = true;

    public void PlaceBossRoom(RoomInstance targetRoom, DoorPoint targetDoor, RoomData bossRoomData) {
        if (targetRoom == null || targetDoor == null || bossRoomData == null) {
            Debug.LogError("Missing references for boss placement!");
            return;
        }

        // Получаем размеры комнат
        Vector2 targetSize = GetRoomSize(targetRoom.data);
        Vector2 bossSize = GetRoomSize(bossRoomData);

        // Определяем направление и рассчитываем позицию
        Vector3 bossCenter = CalculateOptimalPosition(targetRoom, targetDoor, targetSize, bossSize);

        // Проверяем коллизии с существующими комнатами
        if (CheckCollision(bossCenter, bossSize)) {
            Debug.LogWarning("Boss room collides with existing rooms! Adjusting position...");
            bossCenter = AdjustPositionToAvoidCollision(bossCenter, bossSize, targetRoom, targetDoor);
        }

        // Спавним босс-комнату
        GameObject go = Instantiate(bossRoomData.prefab, bossCenter, Quaternion.identity, levelGenerator.transform);
        go.name = $"Room_Boss_{targetRoom.gridPosition + DirToGrid(targetDoor.direction)}";

        RoomInstance bossRoom = go.GetComponent<RoomInstance>();
        if (bossRoom == null) bossRoom = go.AddComponent<RoomInstance>();
        bossRoom.data = bossRoomData;
        bossRoom.gridPosition = targetRoom.gridPosition + DirToGrid(targetDoor.direction);

        // Находим дверь в босс-комнате
        DoorPoint bossDoor = bossRoom.GetDoor(OppositeDir(targetDoor.direction));
        if (bossDoor == null) {
            Debug.LogError("Boss room missing required door!");
            DestroyImmediate(go);
            return;
        }

        // Соединяем двери
        ConnectDoors(targetDoor, bossDoor);

        if (showDebugLogs) {
            Debug.Log($"Boss room placed at {bossCenter}");
            Debug.Log($"Target size: {targetSize}, Boss size: {bossSize}");
            Debug.Log($"Distance between centers: {Vector3.Distance(targetRoom.transform.position, bossCenter)}");
        }
    }

    private Vector3 CalculateOptimalPosition(RoomInstance targetRoom, DoorPoint targetDoor, Vector2 targetSize, Vector2 bossSize) {
        Vector3 direction = GetDirectionVector(targetDoor.direction);
        float distance = (targetSize.x / 2f) + (bossSize.x / 2f) + corridorWidth;

        // Для вертикального направления используем Y размер
        if (targetDoor.direction == DoorDirection.Up || targetDoor.direction == DoorDirection.Down) {
            distance = (targetSize.y / 2f) + (bossSize.y / 2f) + corridorWidth;
        }

        return targetRoom.transform.position + (direction * distance);
    }

    private Vector3 GetDirectionVector(DoorDirection direction) {
        return direction switch {
            DoorDirection.Right => Vector3.right,
            DoorDirection.Left => Vector3.left,
            DoorDirection.Up => Vector3.up,
            DoorDirection.Down => Vector3.down,
            _ => Vector3.zero
        };
    }

    private bool CheckCollision(Vector3 position, Vector2 size) {
        var allRooms = levelGenerator.GetAllRooms();
        Bounds bossBounds = new Bounds(position, new Vector3(size.x, size.y, 0));

        foreach (RoomInstance room in allRooms) {
            if (room == null) continue;
            Vector2 roomSize = GetRoomSize(room.data);
            Bounds roomBounds = new Bounds(room.transform.position, new Vector3(roomSize.x, roomSize.y, 0));
            bossBounds.Expand(0.5f);
            roomBounds.Expand(0.5f);
            if (bossBounds.Intersects(roomBounds)) return true;
        }
        return false;
    }

    private Vector3 AdjustPositionToAvoidCollision(Vector3 originalPos, Vector2 bossSize, RoomInstance targetRoom, DoorPoint targetDoor) {
        Vector3 direction = GetDirectionVector(targetDoor.direction);
        float step = 1f;
        float maxAttempts = 10;

        for (int i = 1; i <= maxAttempts; i++) {
            Vector3 newPos = originalPos + (direction * step * i);

            if (!CheckCollision(newPos, bossSize)) {
                Debug.Log($"Collision avoided by moving {step * i} units");
                return newPos;
            }
        }

        Debug.LogWarning("Could not avoid collision, using original position");
        return originalPos;
    }

    private void ConnectDoors(DoorPoint a, DoorPoint b) {
        a.isConnected = true;
        b.isConnected = true;
        a.connectedTo = b;
        b.connectedTo = a;

        SpawnCorridor(a, b);
        OpenDoor(a);
        OpenDoor(b);
    }

    private void SpawnCorridor(DoorPoint a, DoorPoint b) {
        bool horizontal = a.direction == DoorDirection.Left || a.direction == DoorDirection.Right;
        GameObject prefab = horizontal ? levelGenerator.corridorHorizontalPrefab : levelGenerator.corridorVerticalPrefab;
        if (prefab == null) return;

        Vector3 midpoint = (a.transform.position + b.transform.position) / 2f;
        GameObject corridor = Instantiate(prefab, midpoint, Quaternion.identity, levelGenerator.transform);
        corridor.name = $"Corridor_{a.direction}";
    }

    private void OpenDoor(DoorPoint door) {
        Transform roomRoot = door.transform.parent;
        Transform passageBlockers = roomRoot.Find("PassageBlockers");
        if (passageBlockers == null) return;

        string passageName = door.direction switch {
            DoorDirection.Up => "Passage_Up",
            DoorDirection.Down => "Passage_Down",
            DoorDirection.Left => "Passage_Left",
            DoorDirection.Right => "Passage_Right",
            _ => ""
        };

        Transform passage = passageBlockers.Find(passageName);
        if (passage != null) passage.gameObject.SetActive(false);
    }

    private Vector2 GetRoomSize(RoomData data) {
        if (data != null && data.roomSize != Vector2.zero) return data.roomSize;
        return new Vector2(24f, 24f);
    }

    private Vector2Int DirToGrid(DoorDirection dir) => dir switch {
        DoorDirection.Up => Vector2Int.up,
        DoorDirection.Down => Vector2Int.down,
        DoorDirection.Left => Vector2Int.left,
        DoorDirection.Right => Vector2Int.right,
        _ => Vector2Int.zero
    };

    private DoorDirection OppositeDir(DoorDirection dir) => dir switch {
        DoorDirection.Up => DoorDirection.Down,
        DoorDirection.Down => DoorDirection.Up,
        DoorDirection.Left => DoorDirection.Right,
        DoorDirection.Right => DoorDirection.Left,
        _ => DoorDirection.Up
    };
}