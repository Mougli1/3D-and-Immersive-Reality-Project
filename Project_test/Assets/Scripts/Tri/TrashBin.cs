using UnityEngine;

public class TrashBin : MonoBehaviour
{
    public Dechet.TypeDechet typeAccepte;
    public TrashBinLidScript lidScript;

    private void OnTriggerEnter(Collider other)
    {
        // plus robuste que GetComponent<Dechet>() si le collider est sur un enfant
        Dechet dechet = other.GetComponentInParent<Dechet>();
        if (dechet == null) return;

        if (dechet.type == typeAccepte)
        {
            Debug.Log("Bon tri ! " + dechet.type + " dans la bonne poubelle.");

            // Compte “trié”
            bool added = false;
            if (ProgressManager.Instance != null)
                added = ProgressManager.Instance.MarkTrashSorted(dechet.DechetId);

            Debug.Log($"[Sort] id='{dechet.DechetId}' added={added} now={ProgressManager.Instance?.SortedCount}");

            // Ouvre le couvercle
            if (lidScript != null)
                lidScript.OpenLid();

            Destroy(dechet.gameObject, 2f);
        }
        else
        {
            Debug.Log("Mauvaise poubelle ! " + dechet.type + " ne va pas ici.");
        }
    }
}
