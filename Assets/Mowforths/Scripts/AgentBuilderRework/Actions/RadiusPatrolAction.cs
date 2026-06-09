using UnityEngine;
using UnityEngine.AI;

// rework of hostiles radius wander
public class RadiusPatrolAction : ActionBehaviour
{
    [Header("Radius Patrol Settings")]
    [SerializeField] private float patrolRadius = 8f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float arrivalThreshold = 0.5f;
    [SerializeField, Range(0f, 10f)] private float decisionWeight = 10f;

    private Vector3 spawnPoint;

    public override float? SpeedOverride => patrolSpeed;

    public override void Initialize(AgentBuilder thisAgent)
    {
        base.Initialize(thisAgent);
        spawnPoint = selfTransform.position;
    }

    protected override void SetupDefaults()
    {
        actionName = "Random Patrol";
        weight = decisionWeight;
        considerations.Clear();
        considerations.Add(MakeConsideration("Always Available",
            ResponseCurveType.Linear, 1f, 1f, 0f, 0f, initialInput: 1f));
    }

    public override void UpdateInputs()
    {
        FeedSingleConsideration(1f);
    }

    public override void Execute()
    {
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= arrivalThreshold)
        {
            Vector3 randomOffset = Random.insideUnitSphere * patrolRadius;
            randomOffset.y = 0f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPoint + randomOffset, out hit, patrolRadius, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(Application.isPlaying ? spawnPoint : transform.position, patrolRadius);
    }
}
