using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public List<string> collectedTrashIds = new List<string>();
}

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    [SerializeField] private int trashTotal = 10;
    public int TrashTotal => trashTotal;

    public event Action<int, int> OnTrashProgressChanged; // collected, total

    private SaveData data = new SaveData();
    private readonly HashSet<string> collectedTrashSet = new HashSet<string>();
    private string savePath;

    public int TrashCollected => collectedTrashSet.Count;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        Debug.Log("[Progress] Save path = " + savePath);

        Load();
        RebuildTrashSet();
        NotifyTrash();
    }

    private void NotifyTrash()
    {
        OnTrashProgressChanged?.Invoke(TrashCollected, trashTotal);
    }

    private void Save()
    {
        try
        {
            File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
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
            if (!File.Exists(savePath)) return;
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath)) ?? new SaveData();
        }
        catch
        {
            data = new SaveData();
        }
    }

    private void RebuildTrashSet()
    {
        collectedTrashSet.Clear();
        if (data.collectedTrashIds == null) data.collectedTrashIds = new List<string>();

        foreach (var id in data.collectedTrashIds)
            if (!string.IsNullOrEmpty(id))
                collectedTrashSet.Add(id);
    }

    public bool IsTrashCollected(string id)
        => !string.IsNullOrEmpty(id) && collectedTrashSet.Contains(id);

    public bool MarkTrashCollected(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        if (!collectedTrashSet.Add(id)) return false;

        data.collectedTrashIds.Add(id);
        Save();
        NotifyTrash();
        return true;
    }

    public void ResetAllProgress()
    {
        data = new SaveData();
        collectedTrashSet.Clear();
        Save();
        NotifyTrash();
        Debug.Log("[Progress] Reset done.");
    }
}
