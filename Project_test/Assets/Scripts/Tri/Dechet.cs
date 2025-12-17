using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Dechet : MonoBehaviour
{
    public enum TypeDechet { Papier, Emballages, Verre, Organique }
    public TypeDechet type;

    [Header("UI")]
    public string dechetName = "Déchet";

    [Header("Save")]
    [SerializeField] private string dechetId;
    public string DechetId => dechetId;

    [Tooltip("Si coché, l'objet se détruit au chargement s'il a déjà été trié (Continue).")]
    [SerializeField] private bool despawnIfAlreadySorted = true;

    // --- cache / état ---
    private XRGrabInteractable grab;
    private Rigidbody rb;
    private Collider[] cols;
    private Renderer[] rends;

    private Vector3 spawnPos;
    private Quaternion spawnRot;

    private bool toastLocked = false;
    private bool resetting = false;
    private bool sorted = false;
    private Coroutine resetRoutine;
    private Coroutine lockRoutine;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(dechetId))
            dechetId = System.Guid.NewGuid().ToString();
    }

    [ContextMenu("Regenerate ID")]
    private void RegenerateId()
    {
        dechetId = System.Guid.NewGuid().ToString();
    }
#endif

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        cols = GetComponentsInChildren<Collider>(true);
        rends = GetComponentsInChildren<Renderer>(true);

        spawnPos = transform.position;
        spawnRot = transform.rotation;
    }

    private void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    private IEnumerator Start()
    {
        if (!despawnIfAlreadySorted) yield break;

        while (ProgressManager.Instance == null)
            yield return null;

        if (!string.IsNullOrEmpty(dechetId) && ProgressManager.Instance.IsTrashSorted(dechetId))
            Destroy(gameObject);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        // Ne pas couper un toast "résultat" (mauvais/bon tri) en relâchant
        if (toastLocked) return;
        ToastSystem.Instance?.Hide();
    }

    private void ForceReleaseIfHeld()
    {
        if (grab == null) return;
        if (!grab.isSelected) return;

        if (grab.interactionManager != null)
        {
            var interactor = grab.firstInteractorSelecting;
            if (interactor != null)
                grab.interactionManager.SelectExit(interactor, grab);
        }
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in rends)
            if (r) r.enabled = visible;

        foreach (var c in cols)
            if (c) c.enabled = visible;
    }

    private void LockToast(float seconds)
    {
        if (!gameObject.activeInHierarchy) return;

        toastLocked = true;

        if (lockRoutine != null) StopCoroutine(lockRoutine);
        lockRoutine = StartCoroutine(CoUnlockToast(seconds));
    }

    private IEnumerator CoUnlockToast(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        toastLocked = false;
        lockRoutine = null;
    }

    public bool IsResetting => resetting;
    public bool IsSorted => sorted;

    // Appelé par la poubelle quand le tri est réussi
    public void OnSorted(float toastSeconds)
    {
        if (sorted) return;
        sorted = true;

        LockToast(toastSeconds);
        ForceReleaseIfHeld();
    }

    public void OnWrongBin(float toastSeconds, float respawnDelay)
    {
        if (resetting || sorted) return;

        LockToast(toastSeconds);
        ForceReleaseIfHeld();

        if (resetRoutine != null) StopCoroutine(resetRoutine);
        resetRoutine = StartCoroutine(CoRespawn(respawnDelay));
    }

    private IEnumerator CoRespawn(float respawnDelay)
    {
        resetting = true;

        // "Disparaît" très brièvement
        SetVisible(false);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }

        yield return new WaitForSeconds(respawnDelay);

        // "Réapparaît"
        transform.SetPositionAndRotation(spawnPos, spawnRot);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp();
        }

        SetVisible(true);
        resetting = false;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (sorted) return;
        ToastSystem.Instance?.ShowPersistent($"À trier : {dechetName}");
    }
}
