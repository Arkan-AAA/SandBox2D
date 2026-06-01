using System.IO;
using UnityEngine;

public static class SaveSystem {
    private static string SavePath => Application.persistentDataPath + "/save.json";

    public static void SaveGame(PlayerProgress progress) {
        string json = JsonUtility.ToJson(progress, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game saved to {SavePath}");
    }

    public static PlayerProgress LoadGame() {
        if (File.Exists(SavePath)) {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<PlayerProgress>(json);
        }
        return new PlayerProgress();
    }

    public static void DeleteSave() {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }
}