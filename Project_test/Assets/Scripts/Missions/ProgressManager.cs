using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public List<string> collectedTrashIds = new List<string>();
    public List<string> seenDialogueIds = new List<string>();
    public List<string> sortedTrashIds = new List<string>();
    public List<string> grownTreePlotIds = new List<string>();
    public List<TreePlotState> treePlots = new List<TreePlotState>();

    [System.Serializable]
    public class TreePlotState
    {
        public string plotId;
        public int stage;
    }   

}

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    [SerializeField] private int trashTotal = 10;
    public int TrashTotal => trashTotal;

    public event Action<int, int> OnTrashProgressChanged;

    private SaveData data = new SaveData();
    private readonly HashSet<string> collectedTrashSet = new HashSet<string>();
    private string savePath;

    public int TrashCollected => collectedTrashSet.Count;

    public event Action OnTrashObjectiveCompleted;
    public bool IsTrashObjectiveCompleted => trashTotal > 0 && TrashCollected >= trashTotal;

    private bool trashObjectiveCompletedFired = false;

    [SerializeField] private int sortedTotal = 10;
    public int SortedTotal => sortedTotal;

    public event Action<int, int> OnSortProgressChanged;

    private readonly HashSet<string> sortedTrashSet = new HashSet<string>();
    public int SortedCount => sortedTrashSet.Count;

    [SerializeField] private int treesTotal = 3;
    public int TreesTotal => treesTotal;

    public event Action<int, int> OnTreeProgressChanged;

    private readonly HashSet<string> grownTreeSet = new HashSet<string>();
    public int TreesGrown => grownTreeSet.Count;


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
        RebuildSortedSet();
        NotifySort();
        NotifyTrash();
        RebuildGrownTreesSet();
        NotifyTrees();
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
        sortedTrashSet.Clear();
        grownTreeSet.Clear();
        trashObjectiveCompletedFired = false;
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

    private void RebuildSortedSet()
    {
        sortedTrashSet.Clear();
        if (data.sortedTrashIds == null) data.sortedTrashIds = new List<string>();

        foreach (var id in data.sortedTrashIds)
            if (!string.IsNullOrEmpty(id))
                sortedTrashSet.Add(id);
    }

    private void NotifySort()
    {
        OnSortProgressChanged?.Invoke(SortedCount, sortedTotal);
    }

    public bool IsTrashSorted(string id)
        => !string.IsNullOrEmpty(id) && sortedTrashSet.Contains(id);

    public bool MarkTrashSorted(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;

        if (!sortedTrashSet.Add(id)) return false;

        if (data.sortedTrashIds == null) data.sortedTrashIds = new List<string>();
        data.sortedTrashIds.Add(id);

        Save();
        NotifySort();
        return true;
    }
    private void RebuildGrownTreesSet()
    {
        grownTreeSet.Clear();
        if (data.grownTreePlotIds == null) data.grownTreePlotIds = new List<string>();

        foreach (var id in data.grownTreePlotIds)
            if (!string.IsNullOrEmpty(id))
                grownTreeSet.Add(id);
    }

    private void NotifyTrees()
    {
        OnTreeProgressChanged?.Invoke(TreesGrown, treesTotal);
    }

    public bool IsTreeGrown(string plotId)
    => !string.IsNullOrEmpty(plotId) && grownTreeSet.Contains(plotId);

    public bool MarkTreeGrown(string plotId)
    {
        if (string.IsNullOrEmpty(plotId)) return false;
        if (!grownTreeSet.Add(plotId)) return false;

        if (data.grownTreePlotIds == null) data.grownTreePlotIds = new List<string>();
        data.grownTreePlotIds.Add(plotId);

        Save();
        NotifyTrees();
        return true;
    }

    public int GetTreeStage(string plotId)
    {
        if (string.IsNullOrEmpty(plotId)) return -1;

        if (data.treePlots == null) data.treePlots = new List<SaveData.TreePlotState>();
        var s = data.treePlots.Find(x => x.plotId == plotId);
        return s != null ? s.stage : -1;
    }

    public void SetTreeStage(string plotId, int stage)
    {
        if (string.IsNullOrEmpty(plotId)) return;

        if (data.treePlots == null) data.treePlots = new List<SaveData.TreePlotState>();

        var s = data.treePlots.Find(x => x.plotId == plotId);
        if (s == null)
        {
            s = new SaveData.TreePlotState { plotId = plotId, stage = stage };
            data.treePlots.Add(s);
        }
        else
        {
            s.stage = stage;
        }

        Save();
    }
}
