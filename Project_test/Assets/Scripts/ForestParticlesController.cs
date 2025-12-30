using System.Collections;
using UnityEngine;

public class ForestParticlesController : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;

    [Header("Colors (0=debut, 1=apres mission1, 2=apres mission2, 3=apres mission3)")]
    [SerializeField] private Color c0 = new Color(0.05f, 0.20f, 0.08f, 0.25f);
    [SerializeField] private Color c1 = new Color(0.10f, 0.35f, 0.12f, 0.28f);
    [SerializeField] private Color c2 = new Color(0.15f, 0.55f, 0.18f, 0.30f);
    [SerializeField] private Color c3 = new Color(0.20f, 0.75f, 0.25f, 0.33f);

    [Header("Optional density")]
    [SerializeField] private float rate0 = 6f;
    [SerializeField] private float rate1 = 8f;
    [SerializeField] private float rate2 = 10f;
    [SerializeField] private float rate3 = 12f;

    int currentIndex = -1;

    void Awake()
    {
        if (!ps) ps = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        StartCoroutine(Bind());
    }

    void OnDisable()
    {
        if (ProgressManager.Instance == null) return;

        ProgressManager.Instance.OnTrashProgressChanged -= OnAnyProgressChanged;
        ProgressManager.Instance.OnSortProgressChanged  -= OnAnyProgressChanged;
        ProgressManager.Instance.OnTreeProgressChanged  -= OnAnyProgressChanged;
    }

    IEnumerator Bind()
    {
        while (ProgressManager.Instance == null)
            yield return null;

        ProgressManager.Instance.OnTrashProgressChanged -= OnAnyProgressChanged;
        ProgressManager.Instance.OnSortProgressChanged  -= OnAnyProgressChanged;
        ProgressManager.Instance.OnTreeProgressChanged  -= OnAnyProgressChanged;

        ProgressManager.Instance.OnTrashProgressChanged += OnAnyProgressChanged;
        ProgressManager.Instance.OnSortProgressChanged  += OnAnyProgressChanged;
        ProgressManager.Instance.OnTreeProgressChanged  += OnAnyProgressChanged;

        ApplyFromProgress();
    }

    void OnAnyProgressChanged(int a, int b)
    {
        ApplyFromProgress();
    }

    void ApplyFromProgress()
    {
        int idx;

        if (ProgressManager.Instance.TreesTotal > 0 &&
            ProgressManager.Instance.TreesGrown >= ProgressManager.Instance.TreesTotal)
            idx = 3;
        else if (ProgressManager.Instance.SortedTotal > 0 &&
                 ProgressManager.Instance.SortedCount >= ProgressManager.Instance.SortedTotal)
            idx = 2;
        else if (ProgressManager.Instance.IsTrashObjectiveCompleted)
            idx = 1;
        else
            idx = 0;

        if (idx == currentIndex) return;
        currentIndex = idx;

        ApplyPreset(idx);
    }

    void ApplyPreset(int idx)
    {
        if (!ps) return; // ultra important

        // IMPORTANT : on récupère les modules depuis l'instance PS, à chaque fois
        var main = ps.main;
        var emission = ps.emission;

        Color col;
        float rate;

        if (idx == 0) { col = c0; rate = rate0; }
        else if (idx == 1) { col = c1; rate = rate1; }
        else if (idx == 2) { col = c2; rate = rate2; }
        else { col = c3; rate = rate3; }

        main.startColor = col;

        var roc = emission.rateOverTime;
        roc.mode = ParticleSystemCurveMode.Constant;
        roc.constant = rate;
        emission.rateOverTime = roc;
    }
}
