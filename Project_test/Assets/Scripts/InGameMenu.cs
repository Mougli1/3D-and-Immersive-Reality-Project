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

        // ✅ On s’abonne quand ProgressManager est prêt
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

        // ✅ Init UI immédiate
        OnTrashProgress(ProgressManager.Instance.TrashCollected, ProgressManager.Instance.TrashTotal);
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
        if (trashLabel) trashLabel.SetText($"Déchets : {collected} / {total}");

        if (trashProgressBar)
        {
            trashProgressBar.minValue = 0;
            trashProgressBar.maxValue = total;
            trashProgressBar.value = collected;
        }
    }
}
