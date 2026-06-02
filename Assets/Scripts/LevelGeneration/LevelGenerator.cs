using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using Audio;

public class LevelGenerator : MonoBehaviour {
    public static LevelGenerator Instance { get; private set; }

    [Header("Настройки уровня")]
    public int minRoomCount = 8;
    public int maxRoomCount = 15;
    public int floorIndex = 0;
    public int seed = 0;
    public bool useRandomSeed = true;

    [Header("Комнаты")]
    public RoomData startRoomData;
    public RoomData bossRoomData;
    public RoomData exitRoomData;
    public List<RoomData> combatRooms;
    public List<RoomData> specialRooms;
    public List<RoomData> eliteRooms;
    public List<RoomData> treasureRooms;
    public List<RoomData> shopRooms;

    [Header("Коридоры")]
    public GameObject corridorHorizontalPrefab;
    public GameObject corridorVerticalPrefab;
    public float corridorWidth = 2f;

    [Header("Продвинутые настройки генерации")]
    public bool enableBranching = true;
    public float branchChance = 0.3f;
    public int maxBranchDepth = 2;
    public bool enableDeadEnds = true;
    public float deadEndChance = 0.5f;
    public int minRoomsBeforeBoss = 5;

    [Header("Music")]
    [SerializeField] private PlaylistSO _levelPlaylist;

    private Dictionary<Vector2Int, RoomInstance> _grid = new();
    private List<RoomInstance> _allRooms = new();
    private Dictionary<Vector2Int, int> _roomDepth = new();
    private int _targetRoomCount;
    private System.Random _rng;
    private Dictionary<RoomData, DoorDirection[]> _prefabDoorCache = new();

    public event System.Action OnGenerated;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() => StartCoroutine(GenerateNextFrame());

    [ContextMenu("Generate")]
    public void Generate() {
        Clear();

        int actualSeed = useRandomSeed ? Random.Range(0, int.MaxValue) : seed;
        _rng = new System.Random(actualSeed);
        _targetRoomCount = _rng.Next(minRoomCount, maxRoomCount + 1);

        Debug.Log($"[Generator] Seed: {actualSeed}, Target rooms: {_targetRoomCount}");

        SpawnRoom(startRoomData, Vector2Int.zero);
        _roomDepth[Vector2Int.zero] = 0;

        Queue<RoomInstance> mainPath = new();
        Queue<RoomInstance> branches = new();
        mainPath.Enqueue(_allRooms[0]);

        GenerateMainPath(mainPath, branches);
        if (enableBranching) GenerateBranches(branches);
        FillRemainingRooms();
        SpawnBossRoom();

        Debug.Log($"[Generator] Сгенерировано {_allRooms.Count} комнат (цель: {_targetRoomCount})");
        LogGenerationStats();

        if (AudioManager.Instance != null && _levelPlaylist != null)
            AudioManager.Instance.StartPlaylist(_levelPlaylist);

        OnGenerated?.Invoke();
    }

    private IEnumerator GenerateNextFrame() {
        yield return null;
        Generate();
    }

    private Vector3 CalculateBossRoomPosition(RoomInstance targetRoom, DoorPoint targetDoor) {
        Vector2 targetSize = GetRoomSize(targetRoom.data);
        Vector2 bossSize = GetRoomSize(bossRoomData);

        Vector3 direction = targetDoor.direction switch {
            DoorDirection.Right => Vector3.right,
            DoorDirection.Left => Vector3.left,
            DoorDirection.Up => Vector3.up,
            DoorDirection.Down => Vector3.down,
            _ => Vector3.zero
        };

        float distance = (targetSize.x / 2f) + (bossSize.x / 2f) + corridorWidth;
        if (targetDoor.direction == DoorDirection.Up || targetDoor.direction == DoorDirection.Down) {
            distance = (targetSize.y / 2f) + (bossSize.y / 2f) + corridorWidth;
        }

        return targetRoom.transform.position + (direction * distance);
    }

    private Vector3 CalculateRoomPosition(RoomData data, Vector2Int gridPos, RoomInstance neighbor = null, DoorDirection? direction = null) {
        Vector2 roomSize = GetRoomSize(data);

        if (neighbor != null && direction.HasValue) {
            Vector2 neighborSize = GetRoomSize(neighbor.data);
            Vector3 dirVector = direction.Value switch {
                DoorDirection.Right => Vector3.right,
                DoorDirection.Left => Vector3.left,
                DoorDirection.Up => Vector3.up,
                DoorDirection.Down => Vector3.down,
                _ => Vector3.zero
            };

            float distance = (roomSize.x / 2f) + (neighborSize.x / 2f) + corridorWidth;
            if (direction.Value == DoorDirection.Up || direction.Value == DoorDirection.Down) {
                distance = (roomSize.y / 2f) + (neighborSize.y / 2f) + corridorWidth;
            }

            return neighbor.transform.position + (dirVector * distance);
        }

        return Vector3.zero;
    }

    private Vector2 GetRoomSize(RoomData data) {
        if (data != null && data.roomSize != Vector2.zero) return data.roomSize;
        return new Vector2(24f, 24f);
    }

    private RoomInstance SpawnRoom(RoomData data, Vector2Int gridPos, RoomInstance neighbor = null, DoorPoint door = null) {
        if (data == null || data.prefab == null) return null;

        Vector3 worldPos;
        if (neighbor != null && door != null) {
            worldPos = CalculateRoomPosition(data, gridPos, neighbor, door.direction);
        }
        else {
            worldPos = CalculateRoomPosition(data, gridPos);
        }

        GameObject go = Instantiate(data.prefab, worldPos, Quaternion.identity, transform);
        go.name = $"Room_{data.roomType}_{gridPos}";

        RoomInstance instance = go.GetComponent<RoomInstance>();
        if (instance == null) instance = go.AddComponent<RoomInstance>();
        instance.data = data;
        instance.gridPosition = gridPos;

        _grid.Add(gridPos, instance);
        _allRooms.Add(instance);

        return instance;
    }

    private void GenerateMainPath(Queue<RoomInstance> mainPath, Queue<RoomInstance> branches) {
        int attempts = 0;
        int maxAttempts = 500;

        while (_allRooms.Count < _targetRoomCount / 2 && mainPath.Count > 0 && attempts < maxAttempts) {
            attempts++;
            RoomInstance current = mainPath.Dequeue();
            int currentDepth = _roomDepth[current.gridPosition];

            foreach (DoorPoint door in current.GetFreeDoors().ToList().OrderBy(x => _rng.Next())) {
                if (_allRooms.Count >= _targetRoomCount / 2) break;

                Vector2Int nextPos = current.gridPosition + DirToGrid(door.direction);
                if (_grid.ContainsKey(nextPos)) continue;

                bool isDeadEnd = enableDeadEnds && (currentDepth >= 3 && _rng.NextDouble() < deadEndChance);
                RoomData candidate = isDeadEnd ? PickDeadEndRoom(door.direction) : PickRoom(door.direction, false);
                if (candidate == null) candidate = PickRoom(door.direction, true);
                if (candidate == null) continue;

                RoomInstance newRoom = SpawnRoom(candidate, nextPos, current, door);
                if (newRoom == null) continue;

                DoorPoint otherDoor = newRoom.GetDoor(door.Opposite());
                if (otherDoor == null) continue;

                ConnectDoors(door, otherDoor);
                _roomDepth[nextPos] = currentDepth + 1;

                if (isDeadEnd) branches.Enqueue(newRoom);
                else mainPath.Enqueue(newRoom);
            }
        }
    }

    private void GenerateBranches(Queue<RoomInstance> branches) {
        int branchAttempts = 0;
        int maxBranchAttempts = 100;

        while (_allRooms.Count < _targetRoomCount - 1 && branches.Count > 0 && branchAttempts < maxBranchAttempts) {
            branchAttempts++;
            RoomInstance current = branches.Dequeue();
            int currentDepth = _roomDepth[current.gridPosition];
            if (currentDepth >= maxBranchDepth) continue;

            foreach (DoorPoint door in current.GetFreeDoors().ToList().OrderBy(x => _rng.Next())) {
                if (_allRooms.Count >= _targetRoomCount - 1) break;
                if (_rng.NextDouble() > branchChance) continue;

                Vector2Int nextPos = current.gridPosition + DirToGrid(door.direction);
                if (_grid.ContainsKey(nextPos)) continue;

                RoomData candidate = PickRoom(door.direction, true);
                if (candidate == null) continue;

                RoomInstance newRoom = SpawnRoom(candidate, nextPos, current, door);
                if (newRoom == null) continue;

                DoorPoint otherDoor = newRoom.GetDoor(door.Opposite());
                if (otherDoor == null) continue;

                ConnectDoors(door, otherDoor);
                _roomDepth[nextPos] = currentDepth + 1;
                branches.Enqueue(newRoom);
            }
        }
    }

    private void FillRemainingRooms() {
        int fillAttempts = 0;
        int maxFillAttempts = 200;

        while (_allRooms.Count < _targetRoomCount - 1 && fillAttempts < maxFillAttempts) {
            fillAttempts++;

            var roomsWithFreeDoors = _allRooms
                .Where(r => r.GetFreeDoors().Count > 0 && _roomDepth[r.gridPosition] < 5)
                .OrderBy(r => _rng.Next())
                .ToList();

            if (roomsWithFreeDoors.Count == 0) break;

            RoomInstance current = roomsWithFreeDoors.First();
            DoorPoint door = current.GetFreeDoors().First();

            Vector2Int nextPos = current.gridPosition + DirToGrid(door.direction);
            if (_grid.ContainsKey(nextPos)) continue;

            RoomData candidate = PickRoom(door.direction, true);
            if (candidate == null) continue;

            RoomInstance newRoom = SpawnRoom(candidate, nextPos, current, door);
            if (newRoom == null) continue;

            DoorPoint otherDoor = newRoom.GetDoor(door.Opposite());
            if (otherDoor == null) continue;

            ConnectDoors(door, otherDoor);
            _roomDepth[nextPos] = _roomDepth[current.gridPosition] + 1;
        }
    }

    private void SpawnBossRoom() {
        if (bossRoomData == null) {
            Debug.LogWarning("BossRoomData not assigned!");
            return;
        }

        RoomInstance target = _allRooms
            .Where(r => r.data.roomType != RoomType.Start &&
                       r.data.roomType != RoomType.Boss &&
                       r.GetFreeDoors().Count > 0)
            .OrderByDescending(r => r.gridPosition.sqrMagnitude)
            .FirstOrDefault();

        if (target == null) {
            Debug.LogWarning("No target room for boss!");
            return;
        }

        DoorPoint freeDoor = target.GetFreeDoors().FirstOrDefault();
        if (freeDoor == null) return;

        Vector2Int bossPos = target.gridPosition + DirToGrid(freeDoor.direction);
        if (_grid.ContainsKey(bossPos)) return;

        Vector3 bossPosition = CalculateBossRoomPosition(target, freeDoor);

        GameObject go = Instantiate(bossRoomData.prefab, bossPosition, Quaternion.identity, transform);
        go.name = $"Room_Boss_{bossPos}";

        RoomInstance bossRoom = go.GetComponent<RoomInstance>();
        if (bossRoom == null) bossRoom = go.AddComponent<RoomInstance>();
        bossRoom.data = bossRoomData;
        bossRoom.gridPosition = bossPos;

        _grid.Add(bossPos, bossRoom);
        _allRooms.Add(bossRoom);

        DoorPoint bossDoor = bossRoom.GetDoor(freeDoor.Opposite());
        if (bossDoor == null) {
            Debug.LogError("Boss room missing required door!");
            DestroyImmediate(go);
            return;
        }

        ConnectDoors(freeDoor, bossDoor);

        Debug.Log($"Boss room placed at {bossPosition}, distance: {Vector3.Distance(target.transform.position, bossPosition)}");
    }

    public void SpawnExitRoom() {
        if (exitRoomData == null) {
            Debug.LogWarning("ExitRoomData not assigned in LevelGenerator!");
            return;
        }

        var bossRoom = _allRooms.FirstOrDefault(r => r.data.roomType == RoomType.Boss);
        if (bossRoom == null) {
            Debug.LogWarning("Boss room not found, cannot spawn exit.");
            return;
        }

        var freeDoor = bossRoom.GetFreeDoors().FirstOrDefault();
        if (freeDoor == null) {
            Debug.LogWarning("Boss room has no free door for exit.");
            return;
        }

        Vector2Int exitPos = bossRoom.gridPosition + DirToGrid(freeDoor.direction);
        if (_grid.ContainsKey(exitPos)) {
            Debug.LogWarning("Exit position already occupied.");
            return;
        }

        RoomInstance exitRoom = SpawnRoom(exitRoomData, exitPos, bossRoom, freeDoor);
        if (exitRoom == null) return;

        DoorPoint exitDoor = exitRoom.GetDoor(freeDoor.Opposite());
        if (exitDoor == null) {
            Debug.LogError("Exit room missing required door!");
            DestroyImmediate(exitRoom.gameObject);
            return;
        }

        ConnectDoors(freeDoor, exitDoor);
        Debug.Log("Exit room spawned after boss defeat.");
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
        GameObject prefab = horizontal ? corridorHorizontalPrefab : corridorVerticalPrefab;
        if (prefab == null) return;

        Vector3 midpoint = (a.transform.position + b.transform.position) / 2f;
        GameObject corridor = Instantiate(prefab, midpoint, Quaternion.identity, transform);
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

    private RoomData PickRoom(DoorDirection incomingDirection, bool allowSpecial = true) {
        DoorDirection neededDoor = OppositeDir(incomingDirection);
        float roll = (float)_rng.NextDouble();

        if (eliteRooms != null && eliteRooms.Count > 0 && roll < 0.1f && floorIndex >= 2) {
            var pool = GetRoomsWithDoor(eliteRooms, neededDoor);
            if (pool.Count > 0) return WeightedRandom(pool);
        }
        if (treasureRooms != null && treasureRooms.Count > 0 && roll >= 0.1f && roll < 0.2f) {
            var pool = GetRoomsWithDoor(treasureRooms, neededDoor);
            if (pool.Count > 0) return WeightedRandom(pool);
        }
        if (shopRooms != null && shopRooms.Count > 0 && roll >= 0.2f && roll < 0.3f && floorIndex >= 1) {
            var pool = GetRoomsWithDoor(shopRooms, neededDoor);
            if (pool.Count > 0) return WeightedRandom(pool);
        }
        if (allowSpecial && specialRooms != null && specialRooms.Count > 0 && _rng.NextDouble() < 0.25f) {
            var pool = GetRoomsWithDoor(specialRooms, neededDoor);
            if (pool.Count > 0) return WeightedRandom(pool);
        }
        if (combatRooms != null && combatRooms.Count > 0) {
            var combatPool = GetRoomsWithDoor(combatRooms, neededDoor);
            if (combatPool.Count > 0) return WeightedRandom(combatPool);
        }
        return null;
    }

    private RoomData PickDeadEndRoom(DoorDirection incomingDirection) {
        DoorDirection neededDoor = OppositeDir(incomingDirection);
        var allRooms = combatRooms.Concat(specialRooms ?? Enumerable.Empty<RoomData>()).ToList();
        var deadEndCandidates = allRooms.Where(rd => {
            if (rd == null || rd.prefab == null) return false;
            var doors = GetPrefabDoors(rd);
            return doors.Length == 1 && doors[0] == neededDoor;
        }).ToList();

        if (deadEndCandidates.Count > 0) return WeightedRandom(deadEndCandidates);
        return PickRoom(incomingDirection, true);
    }

    private List<RoomData> GetRoomsWithDoor(List<RoomData> rooms, DoorDirection neededDoor) {
        List<RoomData> result = new();
        foreach (RoomData rd in rooms) {
            if (rd == null || rd.prefab == null) continue;
            if (rd.minFloor > floorIndex) continue;
            // Было: GetComponentsInChildren каждый раз
            if (GetPrefabDoors(rd).Contains(neededDoor)) result.Add(rd);
        }
        return result;
    }

    private RoomData WeightedRandom(List<RoomData> pool) {
        float total = pool.Sum(r => r.spawnWeight);
        float roll = (float)(_rng.NextDouble() * total);
        foreach (RoomData room in pool) {
            roll -= room.spawnWeight;
            if (roll <= 0f) return room;
        }
        return pool[^1];
    }

    private void LogGenerationStats() {
        if (_roomDepth.Count == 0) return;
        int combatCount = _allRooms.Count(r => r.data.roomType == RoomType.Combat);
        int specialCount = _allRooms.Count(r => r.data.roomType == RoomType.Special);
        int bossCount = _allRooms.Count(r => r.data.roomType == RoomType.Boss);
        int maxDepth = _roomDepth.Values.Max();
        Debug.Log($"Stats: Combat={combatCount}, Special={specialCount}, Boss={bossCount}, Max Depth={maxDepth}");
    }

    private void Clear() {
        foreach (Transform child in transform) Destroy(child.gameObject);
        _grid.Clear();
        _allRooms.Clear();
        _roomDepth.Clear();
        _prefabDoorCache.Clear();
    }

    private static Vector2Int DirToGrid(DoorDirection dir) => dir switch {
        DoorDirection.Up => Vector2Int.up,
        DoorDirection.Down => Vector2Int.down,
        DoorDirection.Left => Vector2Int.left,
        DoorDirection.Right => Vector2Int.right,
        _ => Vector2Int.zero
    };

    private static DoorDirection OppositeDir(DoorDirection dir) => dir switch {
        DoorDirection.Up => DoorDirection.Down,
        DoorDirection.Down => DoorDirection.Up,
        DoorDirection.Left => DoorDirection.Right,
        DoorDirection.Right => DoorDirection.Left,
        _ => DoorDirection.Up
    };

    // Возвращает копию списка всех комнат (для внешнего доступа)
    public List<RoomInstance> GetAllRooms() => new List<RoomInstance>(_allRooms);

    public void NextFloor() {
        floorIndex++;
        Clear();
        Generate();
        TeleportPlayerToStart();
    }

    private void TeleportPlayerToStart() {
        var startRoom = _allRooms.FirstOrDefault(r => r.data.roomType == RoomType.Start);
        if (startRoom != null && Player.Instance != null)
            Player.Instance.transform.position = startRoom.transform.position;
    }

    private DoorDirection[] GetPrefabDoors(RoomData rd) {
        if (!_prefabDoorCache.TryGetValue(rd, out var dirs)) {
            var points = rd.prefab.GetComponentsInChildren<DoorPoint>(true);
            dirs = points.Select(d => d.direction).ToArray();
            _prefabDoorCache[rd] = dirs;
        }
        return dirs;
    }
}