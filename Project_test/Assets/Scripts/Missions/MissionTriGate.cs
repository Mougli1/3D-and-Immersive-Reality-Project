using System.Collections;
using UnityEngine;

public class MissionTriGate : MonoBehaviour
{
    [SerializeField] private GameObject triRoot; // le parent "Tri" à cacher/montrer

    Coroutine bindRoutine;

    void Awake()
    {
        // Cacher immédiatement au lancement (avant même ProgressManager)
        if (triRoot)
            triRoot.SetActive(false);
    }

    void OnEnable()
    {
        bindRoutine = StartCoroutine(Bind());
    }

    void OnDisable()
    {
        if (bindRoutine != null) StopCoroutine(bindRoutine);

        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.OnTrashProgressChanged -= OnTrashProgress;
            ProgressManager.Instance.OnTrashObjectiveCompleted -= Apply;
        }
    }

    IEnumerator Bind()
    {
        while (ProgressManager.Instance == null)
            yield return null;

        ProgressManager.Instance.OnTrashProgressChanged -= OnTrashProgress;
        ProgressManager.Instance.OnTrashProgressChanged += OnTrashProgress;

        ProgressManager.Instance.OnTrashObjectiveCompleted -= Apply;
        ProgressManager.Instance.OnTrashObjectiveCompleted += Apply;

        Apply(); // état initial (important pour Continue)
    }

    void OnTrashProgress(int collected, int total)
    {
        Apply();
    }

    void Apply()
    {
        if (!triRoot || ProgressManager.Instance == null) return;

        // Tri visible seulement quand mission 1 (ramassage) est terminée
        bool unlocked = ProgressManager.Instance.IsTrashObjectiveCompleted;
        triRoot.SetActive(unlocked);
    }
}
