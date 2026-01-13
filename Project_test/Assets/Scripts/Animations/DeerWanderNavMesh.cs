using UnityEngine;
using UnityEngine.AI;

public class DeerWanderNavMesh : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    [Header("Animator Params")]
    [SerializeField] private string vertParam = "Vert";

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 20f;
    [SerializeField] private float minMoveTime = 3f;
    [SerializeField] private float maxMoveTime = 7f;
    [SerializeField] private float minIdleTime = 1.5f;
    [SerializeField] private float maxIdleTime = 4f;

    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 1.2f;
    [SerializeField] private float runSpeed = 3.5f;
    [SerializeField, Range(0f, 1f)] private float runChance = 0.2f;

    float timer;
    bool moving;
    bool running;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        EnterIdle();
    }

    void Update()
    {
        if (!agent || !agent.isOnNavMesh) return;

        timer -= Time.deltaTime;

        if (moving)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            {
                EnterIdle();
            }
            else if (timer <= 0f)
            {
                PickDestination();
                timer = Random.Range(minMoveTime, maxMoveTime);
            }
        }
        else
        {
            if (timer <= 0f)
                EnterMove();
        }

        // Piloter l'anim
        if (animator)
        {
            float vert;

            if (!moving) vert = 0f;
            else if (running) vert = 1f;
            else vert = 0.5f; // marche = valeur fixe

            animator.SetFloat(vertParam, vert);
        }

    }


    void EnterIdle()
    {
        moving = false;
        running = false;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        timer = Random.Range(minIdleTime, maxIdleTime);

        if (animator)
            animator.SetFloat(vertParam, 0f);
    }


    void EnterMove()
    {
        moving = true;
        running = Random.value < runChance;

        agent.isStopped = false;
        agent.speed = running ? runSpeed : walkSpeed;

        PickDestination();
        timer = Random.Range(minMoveTime, maxMoveTime);
    }

    void PickDestination()
    {
        Vector3 random = Random.insideUnitSphere * wanderRadius;
        random.y = 0f;
        Vector3 target = transform.position + random;

        if (NavMesh.SamplePosition(target, out var hit, wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }
}
