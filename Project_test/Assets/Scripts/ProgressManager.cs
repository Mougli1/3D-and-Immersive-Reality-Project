using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public List<string> collectedTrashIds = new List<string>();
    public List<string> seenDialogueIds = new List<string>();

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

    public event Action OnTrashObjectiveCompleted;
    public bool IsTrashObjectiveCompleted => trashTotal > 0 && TrashCollected >= trashTotal;

    private bool trashObjectiveCompletedFired = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        Debug.Log("[Progress] Save path = " + savePath);

        Load();
        RebuildTrashSet();
        RebuildDialogueSet();
        NotifyTrash();
    }

    private void RebuildDialogueSet()
    {
        seenDialogueSet.Clear();
        if (data.seenDialogueIds == null) data.seenDialogueIds = new List<string>();

        foreach (var id in data.seenDialogueIds)
            if (!string.IsNullOrEmpty(id))
                seenDialogueSet.Add(id);
    }

    private void NotifyTrash()
    {
        OnTrashProgressChanged?.Invoke(TrashCollected, trashTotal);
        TryFireTrashCompleted();
    }

    private void TryFireTrashCompleted()
    {
        if (trashObjectiveCompletedFired) return;
        if (!IsTrashObjectiveCompleted) return;

        trashObjectiveCompletedFired = true;
        Debug.Log("[Progress] Trash objective completed!");
        OnTrashObjectiveCompleted?.Invoke();
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
        seenDialogueSet.Clear();
        trashObjectiveCompletedFired = false; // ✅ AJOUT
        Save();
        NotifyTrash();
        Debug.Log("[Progress] Reset done.");
    }

    private readonly HashSet<string> seenDialogueSet = new HashSet<string>();

    public bool IsDialogueSeen(string id)
        => !string.IsNullOrEmpty(id) && seenDialogueSet.Contains(id);

    public void MarkDialogueSeen(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!seenDialogueSet.Add(id)) return;

        if (data.seenDialogueIds == null) data.seenDialogueIds = new List<string>();
        data.seenDialogueIds.Add(id);
        Save();
    }


}
