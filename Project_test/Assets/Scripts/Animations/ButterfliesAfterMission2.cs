using System.Collections;
using UnityEngine;

public class ButterfliesAfterMission2 : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject butterflyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int count = 10;

    [SerializeField] private float minHeight = 0.2f;
    [SerializeField] private float maxHeight = 2.5f;


    bool spawned = false;

    void OnEnable()
    {
        StartCoroutine(BindAndMaybeSpawn());
    }

    void OnDisable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnSortProgressChanged -= OnSortProgress;
    }

    IEnumerator BindAndMaybeSpawn()
    {
        while (ProgressManager.Instance == null)
            yield return null;

        ProgressManager.Instance.OnSortProgressChanged -= OnSortProgress;
        ProgressManager.Instance.OnSortProgressChanged += OnSortProgress;

        if (IsSortCompleted())
            SpawnOnce();
    }

    bool IsSortCompleted()
    {
        return ProgressManager.Instance.SortedTotal > 0
            && ProgressManager.Instance.SortedCount >= ProgressManager.Instance.SortedTotal;
    }

    void OnSortProgress(int sorted, int total)
    {
        if (spawned) return;
        if (total > 0 && sorted >= total)
            SpawnOnce();
    }

    void SpawnOnce()
    {
        if (spawned) return;
        if (!butterflyPrefab || spawnPoints == null || spawnPoints.Length == 0) return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform p = spawnPoints[i];
            if (!p) continue;

            float y = Random.Range(minHeight, maxHeight);
            Vector3 jitter = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            Vector3 pos = p.position + jitter + Vector3.up * y;


            GameObject anchor = new GameObject($"ButterflyAnchor_{i}");
            anchor.transform.position = pos;
            anchor.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject b = Instantiate(butterflyPrefab, anchor.transform);
            b.transform.localPosition = Vector3.zero;
            b.transform.localRotation = Quaternion.identity;
        }


        spawned = true;
    }


}
