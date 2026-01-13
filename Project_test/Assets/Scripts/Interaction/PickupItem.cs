using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PickupItem : MonoBehaviour {
    [SerializeField] private float dropOffset = 0.35f;
    private Rigidbody rb;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    public void SendTo(Transform entryPoint) {
        if (!entryPoint) return;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = entryPoint.position + Vector3.up * dropOffset;
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}
