using UnityEngine;

public class TrashBin : MonoBehaviour
{
    public Dechet.TypeDechet typeAccepte;
    public TrashBinLidScript lidScript;

    [Header("Toasts")]
    [SerializeField] private float successToastSeconds = 2.5f;
    [SerializeField] private float wrongToastSeconds = 3.5f;

    [Header("Wrong bin respawn")]
    [SerializeField] private float wrongRespawnDelay = 0.05f;

    [Header("Destroy delay (good bin)")]
    [SerializeField] private float destroyDelay = 0.2f;

    private void OnTriggerEnter(Collider other)
    {
        // robuste si collider enfant
        Dechet dechet = other.GetComponentInParent<Dechet>();
        if (dechet == null) return;
        if (dechet.IsResetting || dechet.IsSorted) return;

        if (dechet.type != typeAccepte)
        {
            ToastSystem.Instance?.Show($"Mauvais tri ! Recommencez.", wrongToastSeconds);
            dechet.OnWrongBin(wrongToastSeconds, wrongRespawnDelay);
            return;
        }

        // Bon tri : on incrémente la progression
        bool added = ProgressManager.Instance != null && ProgressManager.Instance.MarkTrashSorted(dechet.DechetId);
        if (!added)
            return;

        ToastSystem.Instance?.Show($"Bien joué ! Trié : {dechet.dechetName} ({dechet.type})", successToastSeconds);
        dechet.OnSorted(successToastSeconds);

        if (lidScript != null)
            lidScript.OpenLid();

        Destroy(dechet.gameObject, destroyDelay);
    }
}
