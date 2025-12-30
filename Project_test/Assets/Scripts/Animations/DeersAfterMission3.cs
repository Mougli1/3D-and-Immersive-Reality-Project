using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DeersAfterMission3 : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject deerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Tooltip("Si vrai: 1 cerf par point. Sinon: 'count' cerfs au hasard.")]
    [SerializeField] private bool onePerPoint = true;

    [SerializeField] private int count = 5;

    [Tooltip("On snap la position sur le NavMesh (évite les erreurs).")]
    [SerializeField] private float navMeshSnapRadius = 5f;

    bool spawned = false;

    void OnEnable() => StartCoroutine(BindAndMaybeSpawn());

    void OnDisable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnTreeProgressChanged -= OnTreeProgress;
    }

    IEnumerator BindAndMaybeSpawn()
    {
        while (ProgressManager.Instance == null)
            yield return null;

        ProgressManager.Instance.OnTreeProgressChanged -= OnTreeProgress;
        ProgressManager.Instance.OnTreeProgressChanged += OnTreeProgress;

        if (IsTreeCompleted())
            SpawnOnce();
    }

    bool IsTreeCompleted()
    {
        return ProgressManager.Instance.TreesTotal > 0 &&
               ProgressManager.Instance.TreesGrown >= ProgressManager.Instance.TreesTotal;
    }

    void OnTreeProgress(int grown, int total)
    {
        if (spawned) return;
        if (total > 0 && grown >= total)
            SpawnOnce();
    }

    void SpawnOnce()
    {
        if (spawned) return;
        if (!deerPrefab || spawnPoints == null || spawnPoints.Length == 0) return;

        if (onePerPoint)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
                SpawnAt(spawnPoints[i]);
        }
        else
        {
            for (int i = 0; i < count; i++)
                SpawnAt(spawnPoints[Random.Range(0, spawnPoints.Length)]);
        }

        spawned = true;
    }

    void SpawnAt(Transform p)
    {
        if (!p) return;

        Vector3 pos = p.position;

        // Snap au NavMesh
        if (NavMesh.SamplePosition(pos, out var hit, navMeshSnapRadius, NavMesh.AllAreas))
            pos = hit.position;

        Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        Instantiate(deerPrefab, pos, rot);
    }
}
