using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class TrashItem : MonoBehaviour
{
    [Header("Infos")]
    public string trashName = "Déchet";

    [Header("Collect (tenir X secondes)")]
    public float holdToCollectSeconds = 3f;

    private XRGrabInteractable grab;
    private Coroutine holdRoutine;
    private bool collected = false;

    private TrashQuestManager manager;

    [SerializeField] private string trashId;
    public string TrashId => trashId;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(trashId))
            trashId = System.Guid.NewGuid().ToString();
    }

    [ContextMenu("Regenerate ID")]
    private void RegenerateId()
    {
        trashId = System.Guid.NewGuid().ToString();
    }
    #endif

    public void SetManager(TrashQuestManager m) => manager = m;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    IEnumerator Start()
    {
        while (ProgressManager.Instance == null)
            yield return null;

        if (!string.IsNullOrEmpty(trashId) &&
            ProgressManager.Instance.IsTrashCollected(trashId))
        {
            Destroy(gameObject);
            yield break;
        }
    }

    void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (collected) return;

        // Afficher tant qu'on tient
        ToastSystem.Instance?.ShowPersistent($"Ramassé : {trashName}. Vous le rangez dans votre sac.");

        // Démarre le timer de tenue
        if (holdRoutine != null) StopCoroutine(holdRoutine);
        holdRoutine = StartCoroutine(CoHoldCollect(args));
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (collected) return;

        // Si on lâche avant la fin : on annule
        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
            holdRoutine = null;
        }

        ToastSystem.Instance?.Hide();
    }

    IEnumerator CoHoldCollect(SelectEnterEventArgs args)
    {
        yield return new WaitForSeconds(holdToCollectSeconds);
        if (!grab.isSelected || collected)
        {
            holdRoutine = null;
            yield break;
        }

        // Collecte validée
        collected = true;

        Debug.Log($"[Trash] Collected: {trashName}");
        bool added = ProgressManager.Instance != null && ProgressManager.Instance.MarkTrashCollected(trashId);
        Debug.Log($"[Trash] MarkTrashCollected name={trashName} id='{trashId}' added={added} now={ProgressManager.Instance?.TrashCollected}");

        manager?.NotifyTrashCollected(this);

        ToastSystem.Instance?.Hide();

        // Release propre puis destruction
        if (grab.interactionManager != null)
        {
            var interactor = grab.firstInteractorSelecting;
            if (interactor != null)
                grab.interactionManager.SelectExit(interactor, grab);
        }

        Destroy(gameObject);
    }
}
