using UnityEngine;

public enum RoomType { Start, Combat, Special, Shop, Elite, Boss, Treasure }

[CreateAssetMenu(menuName = "RogueLite/Room Data")]
public class RoomData : ScriptableObject {
    public RoomType roomType;
    public GameObject prefab;

    [Range(0f, 1f)]
    public float spawnWeight = 1f;

    [Tooltip("Минимальный этаж, на котором может появиться")]
    public int minFloor = 0;

    [Header("Размер комнаты")]
    public Vector2 roomSize = new Vector2(24f, 24f); // Реальный размер в Unity единицах
    public Vector2Int roomTileSize = new Vector2Int(14, 18); // Размер в тайлах (опционально)
}