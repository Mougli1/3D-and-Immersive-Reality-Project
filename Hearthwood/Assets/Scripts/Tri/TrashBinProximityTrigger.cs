using UnityEngine;

public class TrashBinProximityTrigger : MonoBehaviour
{
    public TrashBinLidScript lidScript;

    private void OnTriggerEnter(Collider other)
    {
        var dechet = other.GetComponentInParent<Dechet>();
        if (dechet == null) return;

        if (lidScript != null)
            lidScript.OpenLid();
    }
}
