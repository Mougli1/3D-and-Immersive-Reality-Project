using UnityEngine;

public class TrashBin : MonoBehaviour
{
    public Dechet.TypeDechet typeAccepte;
    public TrashBinLidScript lidScript;

    private void OnTriggerEnter(Collider other)
    {
        // plus robuste si le collider est sur un enfant
        Dechet dechet = other.GetComponentInParent<Dechet>();
        if (dechet == null) return;

        if (dechet.type != typeAccepte)
        {
            Debug.Log("Mauvaise poubelle ! " + dechet.type + " ne va pas ici.");
            return;
        }

        // Marquer comme trié (sauvegarde + UI)
        bool added = ProgressManager.Instance != null && ProgressManager.Instance.MarkTrashSorted(dechet.DechetId);

        if (!added)
        {
            Debug.Log($"[Sort] Déjà trié ou ID dupliqué: {dechet.DechetId}");
            return;
        }

        Debug.Log("Bon tri ! " + dechet.type + " dans la bonne poubelle.");
        dechet.NotifySorted();

        ToastSystem.Instance?.Show($"Trié : {dechet.dechetName} ({dechet.type})", 1.5f);

        if (lidScript != null) lidScript.OpenLid();
        Destroy(dechet.gameObject, 2f);
    }
}
