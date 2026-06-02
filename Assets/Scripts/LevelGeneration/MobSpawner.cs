using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MobData {
    public GameObject mobPrefab;
    public string mobName;
    public int spawnWeight = 10;
    public bool isBoss = false;
}

[System.Serializable]
public class RoomSpawnConfig {
    public RoomType roomType;
    public int minMobs = 1;
    public int maxMobs = 5;
    public float spawnChance = 1f;
}

public class MobSpawner : MonoBehaviour {
    [Header("Настройки")]
    public LevelGenerator levelGenerator;
    public List<MobData> allMobs = new List<MobData>();
    public List<RoomSpawnConfig> roomConfigs = new List<RoomSpawnConfig>();

    [Header("Количество мобов")]
    public int defaultMinMobs = 1;
    public int defaultMaxMobs = 5;
    public float defaultSpawnChance = 0.8f;

    [Header("Размер пола (в Unity единицах)")]
    public float floorWidth = 14f;
    public float floorHeight = 18f;

    [Header("Отступ от стен")]
    public float wallOffset = 1.5f;

    [Header("Босс")]
    public MobData bossMob;

    private System.Random _rng;
    private Dictionary<RoomInstance, List<GameObject>> _spawnedMobs = new();

    void Awake() {
        if (levelGenerator == null)
            levelGenerator = FindAnyObjectByType<LevelGenerator>();

        if (levelGenerator != null)
            levelGenerator.OnGenerated += OnLevelGenerated;
        else
            Debug.LogWarning("MobSpawner: LevelGenerator not found!");
    }

    void Start() {
        _rng = new System.Random();
    }

    void OnDestroy() {
        if (levelGenerator != null)
            levelGenerator.OnGenerated -= OnLevelGenerated;
    }

    void OnLevelGenerated() {
        var rooms = GetAllRooms();
        foreach (var room in rooms) {
            if (room == null) continue;
            if (room.data.roomType == RoomType.Start) continue;
            if (room.GetComponentInChildren<RoomTrigger>() != null) continue;
            AddRoomTrigger(room);
        }
        SpawnAllMobs();
    }

    void AddRoomTrigger(RoomInstance room) {
        GameObject triggerObj = new GameObject("RoomTrigger");
        triggerObj.transform.SetParent(room.transform);
        triggerObj.transform.localPosition = Vector3.zero;

        BoxCollider2D col = triggerObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(floorWidth, floorHeight);

        triggerObj.AddComponent<RoomTrigger>().Initialize(room, this);
    }

    void SpawnAllMobs() {
        var rooms = GetAllRooms();
        foreach (var room in rooms) {
            if (room == null) continue;
            SpawnMobsInRoom(room);
        }
    }

    void SpawnMobsInRoom(RoomInstance room) {
        if (room.data.roomType == RoomType.Boss) {
            SpawnBoss(room);
            return;
        }

        if (room.data.roomType == RoomType.Start) return;

        RoomSpawnConfig config = roomConfigs.Find(c => c.roomType == room.data.roomType);
        int mobCount;
        float chance;

        if (config != null) {
            mobCount = Random.Range(config.minMobs, config.maxMobs + 1);
            chance = config.spawnChance;
        }
        else {
            mobCount = Random.Range(defaultMinMobs, defaultMaxMobs + 1);
            chance = defaultSpawnChance;
        }

        if (Random.value > chance || mobCount <= 0) return;

        List<MobData> availableMobs = allMobs.Where(m => !m.isBoss).ToList();
        if (availableMobs.Count == 0) return;

        List<Vector3> spawnPoints = GetValidSpawnPointsInRoom(room, mobCount);

        List<GameObject> spawned = new List<GameObject>();

        foreach (Vector3 pos in spawnPoints) {
            MobData mob = WeightedRandom(availableMobs);
            if (mob?.mobPrefab == null) continue;

            GameObject mobObj = Instantiate(mob.mobPrefab, pos, Quaternion.identity, room.transform);

            var enemyAI = mobObj.GetComponent<EnemyAI>();
            if (enemyAI != null) enemyAI.enabled = false;

            spawned.Add(mobObj);
        }

        if (spawned.Count > 0) {
            _spawnedMobs[room] = spawned;
        }
    }

    void SpawnBoss(RoomInstance room) {
        if (bossMob?.mobPrefab == null) return;

        Vector3 spawnPos = room.transform.position;
        GameObject boss = Instantiate(bossMob.mobPrefab, spawnPos, Quaternion.identity, room.transform);

        var enemyAI = boss.GetComponent<EnemyAI>();
        if (enemyAI != null) enemyAI.enabled = false;

        _spawnedMobs[room] = new List<GameObject> { boss };
    }

    public void OnPlayerEnteredRoom(RoomInstance room) {
        if (!_spawnedMobs.ContainsKey(room)) return;
        foreach (var mob in _spawnedMobs[room]) {
            if (mob == null) continue;
            var enemyAI = mob.GetComponent<EnemyAI>();
            if (enemyAI != null) enemyAI.enabled = true;
        }
    }

    List<Vector3> GetValidSpawnPointsInRoom(RoomInstance room, int count) {
        List<Vector3> points = new List<Vector3>();
        Vector3 roomPos = room.transform.position;

        float halfWidth = floorWidth / 2f;
        float halfHeight = floorHeight / 2f;

        float left = roomPos.x - halfWidth + wallOffset;
        float right = roomPos.x + halfWidth - wallOffset;
        float bottom = roomPos.y - halfHeight + wallOffset;
        float top = roomPos.y + halfHeight - wallOffset;

        Vector3[] corners = {
            new Vector3(left, top, 0),
            new Vector3(right, top, 0),
            new Vector3(left, bottom, 0),
            new Vector3(right, bottom, 0)
        };

        points.AddRange(corners);

        for (int i = 0; i < count; i++) {
            float x = Random.Range(left, right);
            float y = Random.Range(bottom, top);
            points.Add(new Vector3(x, y, 0));
        }

        return points.Distinct().Take(count).ToList();
    }

    MobData WeightedRandom(List<MobData> mobs) {
        int total = mobs.Sum(m => m.spawnWeight);
        int roll = _rng.Next(total);

        foreach (MobData mob in mobs) {
            roll -= mob.spawnWeight;
            if (roll < 0) return mob;
        }
        return mobs.FirstOrDefault();
    }

    List<RoomInstance> GetAllRooms() {
        if (levelGenerator == null) return new List<RoomInstance>();
        return levelGenerator.GetAllRooms();
    }
}