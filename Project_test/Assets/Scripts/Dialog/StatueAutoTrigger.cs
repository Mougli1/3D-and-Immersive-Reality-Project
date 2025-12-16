using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class StatueAutoTrigger : MonoBehaviour
{
    [SerializeField] private NarrationSystem narration;

    [Header("Dialogues")]
    [SerializeField] private DialogueAsset introDialogue;
    [SerializeField] private DialogueAsset ramassageBriefDialogue;

    [Header("Options")]
    [SerializeField] private bool forceIntroFirst = true; // si true: on joue intro au moins 1 fois avant de passer au brief
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

        bool trashComplete = pm != null && pm.TrashTotal > 0 && pm.TrashCollected >= pm.TrashTotal;

        // Choix du dialogue à jouer
        DialogueAsset target = introDialogue;

        if (trashComplete && ramassageBriefDialogue != null)
        {
            if (!forceIntroFirst || (pm != null && pm.IsDialogueSeen(introDialogue.id)))
                target = ramassageBriefDialogue;
        }

        bool seen = pm != null && pm.IsDialogueSeen(target.id);

        if (seen && target.lines != null && target.lines.Length > 0)
            narration.StartDialogue(target, target.lines.Length - 1);  // dernière ligne
        else
            narration.StartDialogue(target);                           // dialogue complet

        if (debugLogs)
            Debug.Log($"[StatueAutoTrigger] Play={target.id} seen={seen} trashComplete={trashComplete}");
    }
}
