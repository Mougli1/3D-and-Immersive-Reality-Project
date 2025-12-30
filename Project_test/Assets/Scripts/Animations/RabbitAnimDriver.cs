using UnityEngine;

public class RabbitAnimDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string boolName = "isMoving";
    [SerializeField] private float moveThreshold = 0.02f; // sensibilité

    Vector3 lastPos;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        lastPos = transform.position;
    }

    void Update()
    {
        Vector3 delta = transform.position - lastPos;
        bool moving = delta.sqrMagnitude > (moveThreshold * moveThreshold);

        if (animator) animator.SetBool(boolName, moving);

        lastPos = transform.position;
    }
}
