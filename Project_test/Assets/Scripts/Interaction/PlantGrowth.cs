using System;
using System.Collections;
using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    [Header("0 = seed, 1 = sprout, 2 = young tree, 3 = adult")]
    public GameObject[] stages;

    [Tooltip("Durée (en secondes) pour passer à l’étape suivante APRÈS un arrosage.")]
    public float timeBetweenStages = 5f;

    private int currentStage = -1;
    private Coroutine growRoutine;
    private bool isGrowingStep = false;

    // Event pour sauvegarder à chaque étape (1,2,3...)
    public event Action<int> OnStageReached;

    // Optionnel : event quand adulte
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

    /// IMPORTANT : on affiche l'étape sans déclencher OnStageReached
    /// (sinon au chargement ça re-sauvegarde et ça fait des effets de bord)
    public void ShowStage(int stage)
    {
        if (stages == null || stages.Length == 0) return;

        stage = Mathf.Clamp(stage, 0, stages.Length - 1);

        // Stop une croissance en cours si on force un état (ex: load)
        if (growRoutine != null) StopCoroutine(growRoutine);
        growRoutine = null;
        isGrowingStep = false;

        currentStage = stage;

        HideAllStages();
        ShowStageInternal(currentStage);
    }

    /// Appelée quand on arrose : lance UN SEUL passage d’étape (après timeBetweenStages)
    public bool WaterOnce()
    {
        if (stages == null || stages.Length == 0) return false;
        if (currentStage < 0) return false;                   // rien planté
        if (currentStage >= stages.Length - 1) return false;  // déjà adulte
        if (isGrowingStep) return false;                      // déjà en train de grandir

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

        // ON SAUVEGARDE ICI (une seule fois)
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
