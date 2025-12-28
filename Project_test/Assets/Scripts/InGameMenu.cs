using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TextMeshProUGUI trashLabel;
    [SerializeField] private Slider trashProgressBar;

    [SerializeField] private TextMeshProUGUI sortLabel;
    [SerializeField] private Slider sortProgressBar;
    [SerializeField] private TextMeshProUGUI treeLabel;
    [SerializeField] private Slider treeProgressBar;


    [Header("Input")]
    [SerializeField] private InputActionReference toggleMenuAction;

    bool isOpen = false;

    Coroutine bindRoutine;
    bool subscribed = false;

    void OnEnable()
    {
        if (toggleMenuAction && toggleMenuAction.action != null)
        {
            toggleMenuAction.action.Enable();
            toggleMenuAction.action.performed += OnToggle;
        }

        // On s’abonne quand ProgressManager est prêt
        bindRoutine = StartCoroutine(BindToProgressManager());
    }

    void OnDisable()
    {
        if (toggleMenuAction && toggleMenuAction.action != null)
            toggleMenuAction.action.performed -= OnToggle;

        if (bindRoutine != null) StopCoroutine(bindRoutine);
        bindRoutine = null;

        if (subscribed && ProgressManager.Instance != null)
            ProgressManager.Instance.OnTrashProgressChanged -= OnTrashProgress;

        if (subscribed && ProgressManager.Instance != null)
            ProgressManager.Instance.OnSortProgressChanged -= OnSortProgress;

        if (subscribed && ProgressManager.Instance != null)
            ProgressManager.Instance.OnTreeProgressChanged -= OnTreeProgress;

        subscribed = false;
    }

    IEnumerator BindToProgressManager()
    {
        while (ProgressManager.Instance == null)
            yield return null;

        // sécurité anti-double abonnement
        ProgressManager.Instance.OnTrashProgressChanged -= OnTrashProgress;
        ProgressManager.Instance.OnTrashProgressChanged += OnTrashProgress;
        subscribed = true;

        ProgressManager.Instance.OnSortProgressChanged -= OnSortProgress;
        ProgressManager.Instance.OnSortProgressChanged += OnSortProgress;

        ProgressManager.Instance.OnTreeProgressChanged -= OnTreeProgress;
        ProgressManager.Instance.OnTreeProgressChanged += OnTreeProgress;

        // init
        OnTrashProgress(ProgressManager.Instance.TrashCollected, ProgressManager.Instance.TrashTotal);
        OnSortProgress(ProgressManager.Instance.SortedCount, ProgressManager.Instance.SortedTotal);
        OnTreeProgress(ProgressManager.Instance.TreesGrown, ProgressManager.Instance.TreesTotal);
    }

    void Start()
    {
        SetOpen(false);
    }

    void OnToggle(InputAction.CallbackContext ctx)
    {
        SetOpen(!isOpen);
    }

    void SetOpen(bool open)
    {
        isOpen = open;
        if (menuPanel) menuPanel.SetActive(open);
    }

    void OnTrashProgress(int collected, int total)
    {
        if (trashLabel) trashLabel.SetText($"Déchets collectés: {collected} / {total}");

        if (trashProgressBar)
        {
            trashProgressBar.minValue = 0;
            trashProgressBar.maxValue = total;
            trashProgressBar.value = collected;
        }
    }

    void OnSortProgress(int sorted, int total)
    {
        if (sortLabel) sortLabel.SetText($"Déchets triés: {sorted} / {total}");

        if (sortProgressBar)
        {
            sortProgressBar.minValue = 0;
            sortProgressBar.maxValue = total;
            sortProgressBar.value = sorted;
        }
    }

    void OnTreeProgress(int grown, int total)
    {
        if (treeLabel) treeLabel.SetText($"Arbres plantés : {grown} / {total}");

        if (treeProgressBar)
        {
            treeProgressBar.minValue = 0;
            treeProgressBar.maxValue = total;
            treeProgressBar.value = grown;
        }
    }

}
