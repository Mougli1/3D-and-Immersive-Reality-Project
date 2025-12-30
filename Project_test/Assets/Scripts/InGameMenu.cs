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

    [Header("World Placement")]
    [Tooltip("La caméra XR (Main Camera du XR Origin). Si vide, Camera.main sera utilisée.")]
    [SerializeField] private Transform playerHead;

    [Tooltip("Objet qu'on déplace dans le monde (idéalement le root du canvas/menu). Si vide, on utilise menuPanel.transform.")]
    [SerializeField] private Transform menuRoot;

    [SerializeField] private float spawnDistance = 1.4f;
    [SerializeField] private float heightOffset = -0.1f;

    [Tooltip("Si le joueur s'éloigne au-delà, on ferme le menu.")]
    [SerializeField] private float autoCloseDistance = 3.0f;

    [Tooltip("0 ou 180 si votre canvas est à l'envers.")]
    [SerializeField] private float facingOffsetY = 0f;

    bool isOpen = false;

    Coroutine bindRoutine;
    bool subscribed = false;

    void Awake()
    {
        if (!playerHead && Camera.main) playerHead = Camera.main.transform;
        if (!menuRoot && menuPanel) menuRoot = menuPanel.transform;
    }

    void OnEnable()
    {
        if (toggleMenuAction && toggleMenuAction.action != null)
        {
            toggleMenuAction.action.Enable();
            toggleMenuAction.action.performed += OnToggle;
        }

        bindRoutine = StartCoroutine(BindToProgressManager());
    }

    void OnDisable()
    {
        if (toggleMenuAction && toggleMenuAction.action != null)
            toggleMenuAction.action.performed -= OnToggle;

        if (bindRoutine != null) StopCoroutine(bindRoutine);
        bindRoutine = null;

        if (subscribed && ProgressManager.Instance != null)
        {
            ProgressManager.Instance.OnTrashProgressChanged -= OnTrashProgress;
            ProgressManager.Instance.OnSortProgressChanged -= OnSortProgress;
            ProgressManager.Instance.OnTreeProgressChanged -= OnTreeProgress;
        }

        subscribed = false;
    }

    void Update()
    {
        if (!isOpen) return;
        if (!playerHead || !menuRoot) return;

        if (autoCloseDistance > 0f)
        {
            float d = Vector3.Distance(playerHead.position, menuRoot.position);
            if (d > autoCloseDistance)
                CloseMenu();
        }
    }

    IEnumerator BindToProgressManager()
    {
        while (ProgressManager.Instance == null)
            yield return null;

        ProgressManager.Instance.OnTrashProgressChanged -= OnTrashProgress;
        ProgressManager.Instance.OnTrashProgressChanged += OnTrashProgress;

        ProgressManager.Instance.OnSortProgressChanged -= OnSortProgress;
        ProgressManager.Instance.OnSortProgressChanged += OnSortProgress;

        ProgressManager.Instance.OnTreeProgressChanged -= OnTreeProgress;
        ProgressManager.Instance.OnTreeProgressChanged += OnTreeProgress;

        subscribed = true;

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
        ToggleMenu();
    }

    void PlaceMenuInFront()
    {
        if (!playerHead || !menuRoot) return;

        Vector3 fwd = playerHead.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = playerHead.forward;
        fwd.Normalize();

        Vector3 pos = playerHead.position + fwd * spawnDistance;
        pos.y = playerHead.position.y + heightOffset;

        menuRoot.position = pos;

        // Face au joueur (yaw only)
        Vector3 dir = (playerHead.position - menuRoot.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            menuRoot.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, facingOffsetY, 0f);
        }
    }

    void SetOpen(bool open)
    {
        isOpen = open;

        if (open)
            PlaceMenuInFront();

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

    public void ToggleMenu() => SetOpen(!isOpen);
    public void OpenMenu() => SetOpen(true);
    public void CloseMenu() => SetOpen(false);
}
