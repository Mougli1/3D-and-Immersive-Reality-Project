using System.Collections;
using UnityEngine;

public class ForestAmbienceController : MonoBehaviour
{
    [System.Serializable]
    public class AmbiencePreset
    {
        public string name;

        [Header("Directional Light")]
        public Color lightColor = Color.white;
        public float lightIntensity = 1f;
        public Vector3 lightEuler = new Vector3(50f, -30f, 0f);

        [Header("Ambient")]
        public Color ambientLight = Color.gray;
        public float ambientIntensity = 1f;

        [Header("Fog")]
        public bool fogEnabled = false;
        public Color fogColor = Color.gray;
        public float fogDensity = 0.01f;
    }

    [Header("References")]
    [SerializeField] private Light directionalLight;

    [Header("Presets (0=Début, 1=Après mission 1, 2=Après mission 2, 3=Après mission 3)")]
    [SerializeField] private AmbiencePreset[] presets;

    int currentIndex = 0;
    bool sortCompletedFired = false;
    bool treeCompletedFired = false;

    void OnEnable()
    {
        StartCoroutine(BindAndApplyFromProgress());
    }

    void OnDisable()
    {
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.OnTrashObjectiveCompleted -= OnTrashDone;
            ProgressManager.Instance.OnSortProgressChanged -= OnSortProgress;
            ProgressManager.Instance.OnTreeProgressChanged -= OnTreeProgress;
        }
    }

    IEnumerator BindAndApplyFromProgress()
    {
        while (ProgressManager.Instance == null)
            yield return null;

        // Subscriptions
        ProgressManager.Instance.OnTrashObjectiveCompleted -= OnTrashDone;
        ProgressManager.Instance.OnTrashObjectiveCompleted += OnTrashDone;

        ProgressManager.Instance.OnSortProgressChanged -= OnSortProgress;
        ProgressManager.Instance.OnSortProgressChanged += OnSortProgress;

        ProgressManager.Instance.OnTreeProgressChanged -= OnTreeProgress;
        ProgressManager.Instance.OnTreeProgressChanged += OnTreeProgress;

        Debug.Log($"Sorted: {ProgressManager.Instance.SortedCount}/{ProgressManager.Instance.SortedTotal} | Trees: {ProgressManager.Instance.TreesGrown}/{ProgressManager.Instance.TreesTotal} | TrashDone: {ProgressManager.Instance.IsTrashObjectiveCompleted}");
        Debug.Log($"IsSortCompleted={IsSortCompleted()} IsTreeCompleted={IsTreeCompleted()} -> preset={currentIndex}");

        // Déterminer où on en est (continue/save)
        if (IsTreeCompleted()) currentIndex = 3;
        else if (IsSortCompleted()) currentIndex = 2;
        else if (ProgressManager.Instance.IsTrashObjectiveCompleted) currentIndex = 1;
        else currentIndex = 0;

        sortCompletedFired = IsSortCompleted();
        treeCompletedFired = IsTreeCompleted();

        //currentIndex = 4; // TESTING
        ApplyPreset(currentIndex);
    }

    bool IsSortCompleted()
    {
        if (ProgressManager.Instance == null) return false;
        return ProgressManager.Instance.SortedTotal > 0
               && ProgressManager.Instance.SortedCount >= ProgressManager.Instance.SortedTotal;
    }

    bool IsTreeCompleted()
    {
        if (ProgressManager.Instance == null) return false;
        return ProgressManager.Instance.TreesTotal > 0
               && ProgressManager.Instance.TreesGrown >= ProgressManager.Instance.TreesTotal;
    }

    void OnTrashDone()
    {
        currentIndex = Mathf.Max(currentIndex, 1);
        ApplyPreset(currentIndex);
    }

    void OnSortProgress(int sorted, int total)
    {
        if (sortCompletedFired) return;
        if (total <= 0) return;

        if (sorted >= total)
        {
            sortCompletedFired = true;
            currentIndex = Mathf.Max(currentIndex, 2);
            ApplyPreset(currentIndex);
        }
    }

    void OnTreeProgress(int grown, int total)
    {
        if (treeCompletedFired) return;
        if (total <= 0) return;

        if (grown >= total)
        {
            treeCompletedFired = true;
            currentIndex = Mathf.Max(currentIndex, 3);
            ApplyPreset(currentIndex);
        }
    }

    void ApplyPreset(int index)
    {
        if (presets == null || presets.Length == 0) return;
        if (index < 0 || index >= presets.Length) return;

        var p = presets[index];

        if (directionalLight)
        {
            directionalLight.color = p.lightColor;
            directionalLight.intensity = p.lightIntensity;
            directionalLight.transform.rotation = Quaternion.Euler(p.lightEuler);
        }

        RenderSettings.ambientLight = p.ambientLight;
        RenderSettings.ambientIntensity = p.ambientIntensity;

        RenderSettings.fog = p.fogEnabled;
        RenderSettings.fogColor = p.fogColor;
        RenderSettings.fogDensity = p.fogDensity;
    }
}
