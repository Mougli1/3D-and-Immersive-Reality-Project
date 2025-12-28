using System.Collections;
using UnityEngine;

public class SoilPlot : MonoBehaviour
{
    [Header("Plante à créer quand une graine est plantée")]
    public PlantGrowth plantPrefab;

    [Header("Save ID (unique par parcelle)")]
    [SerializeField] private string plotId;
    public string PlotId => plotId;

    private PlantGrowth plantedPlant;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(plotId))
            plotId = System.Guid.NewGuid().ToString();
    }

    [ContextMenu("Regenerate Plot ID")]
    private void RegenerateId()
    {
        plotId = System.Guid.NewGuid().ToString();
    }
#endif

    private IEnumerator Start()
    {
        while (ProgressManager.Instance == null)
            yield return null;

        // Load de l'état au démarrage (0/1/2/3)
        int stage = ProgressManager.Instance.GetTreeStage(plotId);

        if (stage >= 0)
        {
            SpawnPlant();
            plantedPlant.ShowStage(stage);
            HookPlantEvents();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (plantedPlant != null) return;
        if (ProgressManager.Instance == null) return;

        // Si déjà planté dans la save, on ne replante pas
        if (ProgressManager.Instance.GetTreeStage(plotId) >= 0) return;

        if (other.CompareTag("Seed"))
            PlantSeed(other.gameObject);
    }

    private void PlantSeed(GameObject seed)
    {
        Destroy(seed);

        // IMPORTANT : ce bloc ne doit être présent qu'UNE fois
        SpawnPlant();
        plantedPlant.ShowStage(0);
        HookPlantEvents();
        ProgressManager.Instance.SetTreeStage(plotId, 0);

        WaterPromptUI.Instance.Show(this, transform.position + Vector3.up * 0.3f);
    }

    public void WaterPlant()
    {
        if (plantedPlant == null) return;
        plantedPlant.WaterOnce();
    }

    private void SpawnPlant()
    {
        Vector3 spawnPos = transform.position;
        spawnPos.y += 0.01f;

        plantedPlant = Instantiate(plantPrefab, spawnPos, Quaternion.identity);
    }

    private void HookPlantEvents()
    {
        if (plantedPlant == null) return;

        plantedPlant.OnStageReached -= OnPlantStageReached;
        plantedPlant.OnStageReached += OnPlantStageReached;
    }

    private void OnPlantStageReached(int stage)
    {
        if (ProgressManager.Instance == null) return;
        if (string.IsNullOrEmpty(plotId)) return;

        // Sauvegarder toutes les étapes (1,2,3...)
        ProgressManager.Instance.SetTreeStage(plotId, stage);

        // Si dernière étape -> compter pour l'objectif (1/3, 2/3…)
        if (stage >= plantedPlant.LastStageIndex)
            ProgressManager.Instance.MarkTreeGrown(plotId);
    }
}
