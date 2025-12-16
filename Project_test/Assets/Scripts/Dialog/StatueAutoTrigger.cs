using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class StatueAutoTrigger : MonoBehaviour
{
    [SerializeField] private NarrationSystem narration;

    [Header("Dialogues")]
    [SerializeField] private DialogueAsset introDialogue;
    [SerializeField] private DialogueAsset ramassageBriefDialogue;
    [SerializeField] private DialogueAsset triBriefDialogue;

    [Header("Options")]
    [SerializeField] private bool forceIntroFirst = true;
    [SerializeField] private bool forceRamassageBriefFirst = true;
    [SerializeField] private bool debugLogs = false;

    private void Reset()
    {
        var sc = GetComponent<SphereCollider>();
        sc.isTrigger = true;
    }

    private void OnEnable()
    {
        if (narration != null)
            narration.OnDialogueEnded += OnDialogueEnded;
    }

    private void OnDisable()
    {
        if (narration != null)
            narration.OnDialogueEnded -= OnDialogueEnded;
    }

    private void OnDialogueEnded(string dialogueId)
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.MarkDialogueSeen(dialogueId);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (narration == null || introDialogue == null) return;
        if (narration.IsPlaying) return;

        var pm = ProgressManager.Instance;

        bool introSeen = pm != null && pm.IsDialogueSeen(introDialogue.id);

        bool trashComplete = pm != null && pm.TrashTotal > 0 && pm.TrashCollected >= pm.TrashTotal;
        bool sortComplete  = pm != null && pm.SortedTotal > 0 && pm.SortedCount >= pm.SortedTotal;

        DialogueAsset target = introDialogue;

        // 1) Si tri terminé -> triBrief
        if (sortComplete && triBriefDialogue != null)
        {
            // Optionnel : on force l’ordre "Intro -> RamassageBrief -> TriBrief"
            bool briefSeen = (ramassageBriefDialogue == null) || (pm != null && pm.IsDialogueSeen(ramassageBriefDialogue.id));
            if (!forceRamassageBriefFirst || briefSeen)
                target = triBriefDialogue;
            else if (ramassageBriefDialogue != null)
                target = ramassageBriefDialogue;
        }
        // 2) Sinon si ramassage terminé -> ramassageBrief
        else if (trashComplete && ramassageBriefDialogue != null)
        {
            target = ramassageBriefDialogue;
        }

        // Force à voir l’intro au moins 1 fois
        if (forceIntroFirst && !introSeen)
            target = introDialogue;

        bool seen = pm != null && pm.IsDialogueSeen(target.id);

        // si déjà vu -> dernière ligne seulement
        if (seen && target.lines != null && target.lines.Length > 0)
            narration.StartDialogue(target, target.lines.Length - 1);
        else
            narration.StartDialogue(target);

        if (debugLogs)
            Debug.Log($"[StatueAutoTrigger] Play={target.id} seen={seen} trashComplete={trashComplete} sortComplete={sortComplete}");
    }
}
