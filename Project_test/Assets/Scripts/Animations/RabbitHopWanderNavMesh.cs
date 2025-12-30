using UnityEngine;
using UnityEngine.AI;

public class RabbitHopWanderNavMesh : MonoBehaviour
{
    [Header("Wander")]
    [SerializeField] float wanderRadius = 10f;

    [Header("Hop Movement")]
    [SerializeField] float hopMoveTime = 0.35f;   // temps où il avance
    [SerializeField] float hopPauseTime = 0.15f;  // petite pause (glisse = 0)
    [SerializeField] float waitMin = 0.8f;        // pause entre 2 destinations
    [SerializeField] float waitMax = 2.0f;

    NavMeshAgent agent;

    float waitTimer;
    float hopTimer;
    bool hopping;     // on est en train d’aller vers une destination
    bool movePhase;   // true = avance, false = pause

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        waitTimer = Random.Range(waitMin, waitMax);
        hopTimer = hopMoveTime;
        movePhase = true;
    }

    void Update()
    {
        if (!agent) return;

        // si pas de destination ou arrivé : on attend un peu puis on choisit une nouvelle destination
        if (!agent.hasPath || agent.remainingDistance <= 0.6f)
        {
            agent.isStopped = true;
            hopping = false;

            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                agent.SetDestination(RandomPointOnNavMesh(transform.position, wanderRadius));
                waitTimer = Random.Range(waitMin, waitMax);

                // reset cycle de bonds
                hopping = true;
                movePhase = true;
                hopTimer = hopMoveTime;
                agent.isStopped = false;
            }
            return;
        }

        // cycle hop : avance / pause / avance / pause...
        if (!hopping) hopping = true;

        hopTimer -= Time.deltaTime;
        if (hopTimer <= 0f)
        {
            movePhase = !movePhase;
            hopTimer = movePhase ? hopMoveTime : hopPauseTime;
        }

        agent.isStopped = !movePhase;
    }

    static Vector3 RandomPointOnNavMesh(Vector3 center, float radius)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 random = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(random, out var hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }
        return center;
    }
}
