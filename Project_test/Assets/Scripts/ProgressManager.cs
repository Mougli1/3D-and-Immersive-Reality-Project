using System;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int trashCollected = 0;

    // (plus tard) tri / arbres
    public int sortedItems = 0;
    public int treesPlanted = 0;
}

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    [Header("Mission 1 - Déchets (configurable par zone plus tard)")]
    [SerializeField] private int trashTotal = 10;

    public int TrashCollected => data.trashCollected;
    public int TrashTotal => trashTotal;

    public event Action<int, int> OnTrashProgressChanged; // collected, total

    private SaveData data = new SaveData();
    private string savePath;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        Debug.Log("[Progress] Save path = " + savePath);
        Load();
        NotifyTrash();
    }

    public void AddTrash(int amount = 1)
    {
        data.trashCollected = Mathf.Clamp(data.trashCollected + amount, 0, trashTotal);
        Save();
        NotifyTrash();
        Debug.Log($"[Progress] Trash: {data.trashCollected}/{trashTotal}");
    }

    public void ResetAllProgress()
    {
        data = new SaveData();
        Save();
        NotifyTrash();
        Debug.Log("[Progress] Reset done.");
    }

    private void NotifyTrash()
    {
        OnTrashProgressChanged?.Invoke(data.trashCollected, trashTotal);
    }

    private void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("[Progress] Save failed: " + e.Message);
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(savePath))
                return;

            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("[Progress] Load failed: " + e.Message);
            data = new SaveData();
        }
    }

    public string SavePath => savePath;

    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }

    public void DeleteSaveFile()
    {
        try
        {
            if (File.Exists(savePath))
                File.Delete(savePath);

            data = new SaveData();
            NotifyTrash();
            Debug.Log("[Progress] Save deleted.");
        }
        catch (Exception e)
        {
            Debug.LogError("[Progress] Delete failed: " + e.Message);
        }
    }
}

