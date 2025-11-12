using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class StatueAutoTrigger : MonoBehaviour {
    [SerializeField] private NarrationSystem narration;
    [SerializeField] private DialogueAsset dialogue;
    [SerializeField] private bool retrigger = true;   // autorise à relancer en re-rentrant
    [SerializeField] private bool debugLogs = false;

    private bool playedOnce = false;

    private void Reset() {
        var sc = GetComponent<SphereCollider>();
        sc.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        if (narration == null || dialogue == null) {
            if (debugLogs) Debug.LogWarning("[StatueAutoTrigger] Narration/Dialogue manquant.");
            return;
        }
        if (!retrigger && playedOnce) return;

        if (!narration.IsPlaying) {
            narration.StartDialogue(dialogue);
            playedOnce = true;
            if (debugLogs) Debug.Log("[StatueAutoTrigger] Dialogue lancé: " + dialogue.id);
        }
    }
}
