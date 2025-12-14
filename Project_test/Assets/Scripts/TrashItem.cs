using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
// Si XRGrabInteractable est introuvable chez vous, ajoutez aussi (XRI 3.x) :
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
    public void SetManager(TrashQuestManager m) => manager = m;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
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

        // Démarre le timer de tenue (si déjà lancé, on évite les doublons)
        if (holdRoutine != null) StopCoroutine(holdRoutine);
        holdRoutine = StartCoroutine(CoHoldCollect(args));
    }

    void OnRelease(SelectExitEventArgs args)
    {
        // Si déjà collecté, on ne fait rien (la release peut être déclenchée par notre SelectExit forcé)
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

        // Si entre temps on a lâché, grab.isSelected sera faux → on annule
        if (!grab.isSelected || collected)
        {
            holdRoutine = null;
            yield break;
        }

        // ✅ Collecte validée
        collected = true;

        Debug.Log($"[Trash] Collected: {trashName}");
        ProgressManager.Instance?.AddTrash(1);
        manager?.NotifyTrashCollected(this);

        // On cache le texte au moment où ça "rentre dans le sac"
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
