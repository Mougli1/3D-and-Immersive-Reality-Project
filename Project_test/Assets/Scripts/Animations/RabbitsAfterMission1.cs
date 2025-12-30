using System.Collections;
using UnityEngine;

public class RabbitsAfterMission1 : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject rabbitPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int count = 6;

    private bool spawned = false;

    void OnEnable()
    {
        StartCoroutine(BindAndMaybeSpawn());
    }

    void OnDisable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnTrashObjectiveCompleted -= SpawnOnce;
    }

    IEnumerator BindAndMaybeSpawn()
    {
        while (ProgressManager.Instance == null)
            yield return null;

        // écouter l'event (si le joueur termine pendant la session)
        ProgressManager.Instance.OnTrashObjectiveCompleted -= SpawnOnce;
        ProgressManager.Instance.OnTrashObjectiveCompleted += SpawnOnce;

        // si déjà terminé (Continue / chargement) -> spawn tout de suite
        if (ProgressManager.Instance.IsTrashObjectiveCompleted)
            SpawnOnce();
    }

    void SpawnOnce()
    {
        if (spawned) return;
        if (!rabbitPrefab || spawnPoints == null || spawnPoints.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(rabbitPrefab, p.position, p.rotation);
        }

        spawned = true;
    }
}
