using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    [Header("0 = seed, 1 = sprout, 2 = young tree, 3 = adult")]
    public GameObject[] stages;

    public float timeBetweenStages = 5f;

    private int currentStage = -1;   // -1 = rien affiché
    private float timer = 0f;
    private bool isGrowing = false;
    private bool hasStartedGrowing = false; // 🔒 empêche double arrosage

    private void Awake()
    {
        HideAllStages();   // au début, aucune étape visible
    }

    private void Update()
    {
        if (!isGrowing) return;
        if (currentStage < 0) return;

        timer += Time.deltaTime;

        if (timer >= timeBetweenStages)
        {
            timer = 0f;
            GoToNextStage();
        }
    }

    /// Affiche seulement la graine (appelé quand on plante)
    public void ShowSeed()
    {
        currentStage = 0;      // graine
        timer = 0f;
        isGrowing = false;
        hasStartedGrowing = false;

        UpdateVisuals();
    }

    /// Lance la croissance (appelé UNE FOIS quand on arrose)
    public void StartGrowth()
    {
        if (hasStartedGrowing) return; // ⛔ déjà arrosée

        hasStartedGrowing = true;
        isGrowing = true;
        timer = 0f;

        if (currentStage < 0)
            currentStage = 0;

        UpdateVisuals();
    }

    private void GoToNextStage()
    {
        if (currentStage >= stages.Length - 1)
        {
            isGrowing = false; // arbre adulte
            return;
        }

        currentStage++;
        UpdateVisuals();

        if (currentStage >= stages.Length - 1)
        {
            isGrowing = false; // fin croissance
        }
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i] != null)
                stages[i].SetActive(i == currentStage);
        }
    }

    private void HideAllStages()
    {
        if (stages == null) return;

        foreach (var s in stages)
            if (s != null) s.SetActive(false);
    }
}
