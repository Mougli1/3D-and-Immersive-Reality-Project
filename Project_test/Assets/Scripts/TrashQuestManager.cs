using UnityEngine;

public class TrashQuestManager : MonoBehaviour
{
    [Header("Dialogue de fin")]
    public NarrationSystem narration;
    public DialogueAsset dialogueOnAllCollected;

    private TrashItem[] trashItems;
    private int collectedCount = 0;
    private bool allDone = false;

    private void Awake()
    {
        // Récupère automatiquement tous les TrashItem sous ce GameObject
        trashItems = GetComponentsInChildren<TrashItem>();

        foreach (var item in trashItems)
        {
            if (item != null)
                item.SetManager(this);
        }

        Debug.Log($"[TrashQuest] {trashItems.Length} déchets à ramasser.");
    }

    public void NotifyTrashCollected(TrashItem item)
    {
        if (allDone) return;

        collectedCount++;
        Debug.Log($"[TrashQuest] Déchet livré ({collectedCount}/{trashItems.Length})");

        if (collectedCount >= trashItems.Length)
        {
            allDone = true;
            Debug.Log("[TrashQuest] Tous les déchets ont été livrés.");

            if (narration != null && dialogueOnAllCollected != null)
            {
                // Si rien ne parle, on lance tout de suite
                if (!narration.IsPlaying)
                {
                    narration.StartDialogue(dialogueOnAllCollected);
                }
                else
                {
                    // Sinon on attend la fin du dialogue en cours
                    narration.OnDialogueEnded += OnAnyDialogueEnded;
                }
            }
        }
    }

    private void OnAnyDialogueEnded(string id)
    {
        if (narration != null && dialogueOnAllCollected != null && !narration.IsPlaying)
        {
            narration.StartDialogue(dialogueOnAllCollected);
        }

        // On se désabonne pour ne pas relancer plusieurs fois
        if (narration != null)
            narration.OnDialogueEnded -= OnAnyDialogueEnded;
    }
}
