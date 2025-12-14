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

    [Header("Input (manette droite)")]
    [Tooltip("Ex: XRI RightHand Interaction / Primary Button (A)")]
    [SerializeField] private InputActionReference toggleMenuAction;

    bool isOpen = false;

    void OnEnable()
    {
        if (toggleMenuAction && toggleMenuAction.action != null)
        {
            toggleMenuAction.action.Enable();
            toggleMenuAction.action.performed += OnToggle;
        }

        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnTrashProgressChanged += OnTrashProgress;
    }

    void OnDisable()
    {
        if (toggleMenuAction && toggleMenuAction.action != null)
            toggleMenuAction.action.performed -= OnToggle;

        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnTrashProgressChanged -= OnTrashProgress;
    }

    void Start()
    {
        SetOpen(false);

        // Init visuel au démarrage
        if (ProgressManager.Instance != null)
            OnTrashProgress(ProgressManager.Instance.TrashCollected, ProgressManager.Instance.TrashTotal);
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
