using System;
using System.Collections;
using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    [Header("0 = seed, 1 = sprout, 2 = young tree, 3 = adult")]
    public GameObject[] stages;

    public float timeBetweenStages = 5f;

    private int currentStage = -1;
    private Coroutine growRoutine;
    private bool isGrowingStep = false;

    public event Action<int> OnStageReached;

    public event Action OnBecameAdult;

    public int CurrentStage => currentStage;
    public bool IsBusy => isGrowingStep;

    public int LastStageIndex =>
        (stages != null && stages.Length > 0) ? stages.Length - 1 : 3;

    private void Awake()
    {
        HideAllStages();
    }

    public void ShowSeed()
    {
        ShowStage(0);
    }

    public void ShowAdult()
    {
        ShowStage(LastStageIndex);
    }

    public void ShowStage(int stage)
    {
        if (stages == null || stages.Length == 0) return;

        stage = Mathf.Clamp(stage, 0, stages.Length - 1);

        if (growRoutine != null) StopCoroutine(growRoutine);
        growRoutine = null;
        isGrowingStep = false;

        currentStage = stage;

        HideAllStages();
        ShowStageInternal(currentStage);
    }

    // Appelée quand on arrose : lance un seul passage d’étape 
    public bool WaterOnce()
    {
        if (stages == null || stages.Length == 0) return false;
        if (currentStage < 0) return false; // rien planté
        if (currentStage >= stages.Length - 1) return false; // déjà adulte
        if (isGrowingStep) return false; // déjà en train de grandir

        growRoutine = StartCoroutine(CoGrowOneStep());
        return true;
    }

    private IEnumerator CoGrowOneStep()
    {
        isGrowingStep = true;

        yield return new WaitForSeconds(timeBetweenStages);

        currentStage++;
        HideAllStages();
        ShowStageInternal(currentStage);

        // Sauvegarde
        OnStageReached?.Invoke(currentStage);

        if (currentStage >= stages.Length - 1)
            OnBecameAdult?.Invoke();

        isGrowingStep = false;
        growRoutine = null;
    }

    private void ShowStageInternal(int stage)
    {
        if (stage >= 0 && stages != null && stage < stages.Length && stages[stage] != null)
            stages[stage].SetActive(true);
    }

    private void HideAllStages()
    {
        if (stages == null) return;
        foreach (var s in stages)
            if (s != null) s.SetActive(false);
    }
}
