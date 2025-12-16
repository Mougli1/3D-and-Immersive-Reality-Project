using UnityEngine;

public class TrashBin : MonoBehaviour
{
    public Dechet.TypeDechet typeAccepte;
    public TrashBinLidScript lidScript;

    private void OnTriggerEnter(Collider other)
    {
        Dechet dechet = other.GetComponent<Dechet>();

        if (dechet == null) return;

        if (dechet.type == typeAccepte)
        {
            Debug.Log("Bon tri ! " + dechet.type + " dans la bonne poubelle.");

            //  Ouvre le couvercle
            if (lidScript != null)
                lidScript.OpenLid();

            Destroy(other.gameObject, 2f);

        }
        else
        {
            Debug.Log("Mauvaise poubelle ! " + dechet.type + " ne va pas ici.");
        }
    }
}

