using System.IO;
using UnityEngine;

public static class SaveSystem {
    private static string SavePath => Application.persistentDataPath + "/save.json";

    public static void SaveGame(PlayerProgress progress) {
        try {
            string json = JsonUtility.ToJson(progress, true);
            File.WriteAllText(SavePath, json);
        } catch (System.Exception e) {
            Debug.LogError($"SaveGame failed: {e.Message}");
        }
    }

    public static PlayerProgress LoadGame() {
        if (!File.Exists(SavePath)) return new PlayerProgress();
        try {
            string json = File.ReadAllText(SavePath);
            var progress = JsonUtility.FromJson<PlayerProgress>(json);
            return progress ?? new PlayerProgress();
        } catch (System.Exception e) {
            Debug.LogError($"LoadGame failed: {e.Message}");
            return new PlayerProgress();
        }
    }

    public static void DeleteSave() {
        try {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        } catch (System.Exception e) {
            Debug.LogError($"DeleteSave failed: {e.Message}");
        }
    }
}