using System.Collections;
using UnityEngine;

public class MissionPlantationGate : MonoBehaviour
{
    [SerializeField] private GameObject plantationRoot;

    Coroutine bindRoutine;

    void Awake()
    {
        if (plantationRoot)
            plantationRoot.SetActive(false);
    }

    void OnEnable()
    {
        bindRoutine = StartCoroutine(Bind());
    }

    void OnDisable()
    {
        if (bindRoutine != null) StopCoroutine(bindRoutine);

        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnSortProgressChanged -= OnSortProgress;
    }

    IEnumerator Bind()
    {
        while (ProgressManager.Instance == null)
            yield return null;

        ProgressManager.Instance.OnSortProgressChanged -= OnSortProgress;
        ProgressManager.Instance.OnSortProgressChanged += OnSortProgress;

        Apply();
    }

    void OnSortProgress(int sorted, int total)
    {
        Apply();
    }

    bool IsSortCompleted()
    {
        if (ProgressManager.Instance == null) return false;

        int total = ProgressManager.Instance.SortedTotal;
        int done  = ProgressManager.Instance.SortedCount;

        return total > 0 && done >= total;
    }

    void Apply()
    {
        if (!plantationRoot) return;

        // Plantation visible seulement quand mission 2 (tri) est terminée
        plantationRoot.SetActive(IsSortCompleted());
    }
}
