using UnityEngine;

public class PickupInteractor : MonoBehaviour {
    [Header("Ramassage simple")]
    [SerializeField] private float pickupRadius = 2.0f;
    [SerializeField] private LayerMask itemMask;
    [SerializeField] private KeyCode pickupKey = KeyCode.F;
    [SerializeField] private Transform binEntryPoint;
    [SerializeField] private NarrationSystem narration;

    [Header("Objectif")]
    [SerializeField] private int targetCount = 5; // nb d’objets à ramasser
    [SerializeField] private DialogueAsset debriefDialogue;   // RamassageDebrief.asset

    private int currentCount = 0;

    void Update() {
        if (narration && narration.IsPlaying) return;
        if (Input.GetKeyDown(pickupKey)) TryPickupNearest();
    }

    void TryPickupNearest() {
        if (!binEntryPoint) { Debug.LogWarning("[Pickup] EntryPoint non assigné."); return; }

        int mask = (itemMask.value == 0) ? Physics.AllLayers : itemMask.value;
        var hits = Physics.OverlapSphere(transform.position, pickupRadius, mask, QueryTriggerInteraction.Ignore);

        PickupItem closest = null;
        float best = float.MaxValue;
        foreach (var h in hits) {
            var item = h.GetComponentInParent<PickupItem>();
            if (!item) continue;
            float d = (item.transform.position - transform.position).sqrMagnitude;
            if (d < best) { best = d; closest = item; }
        }

        if (!closest) { Debug.Log("[Pickup] Aucun objet à portée."); return; }

        closest.SendTo(binEntryPoint);   // TP au-dessus du bac
        currentCount++;

        if (currentCount >= targetCount) {
            if (debriefDialogue && narration && !narration.IsPlaying) {
                narration.StartDialogue(debriefDialogue);
            }
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
