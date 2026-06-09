using UnityEngine;

public class PatrolWaypointsAction : ActionBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float arrivalThreshold = 0.5f;
    [SerializeField, Range(0f, 10f)] private float decisionWeight = 4f;

    [Header("Sight Cone (Optional)")]
    [Tooltip("If set, patrol backs off as alert rises")]
    [SerializeField] private SightCone sightCone;

    private int currentWaypoint;

    public override float? SpeedOverride => patrolSpeed > 0f ? (float?)patrolSpeed : null;

    public override void Initialize(AgentBuilder thisAgent)
    {
        base.Initialize(thisAgent);
        if (sightCone == null)
        {
            sightCone = GetComponent<SightCone>();
        }
    }

    protected override void SetupDefaults()
    {
        actionName = "Patrol";
        weight = decisionWeight;
        considerations.Clear();
        considerations.Add(MakeConsideration("Calm alert level",
            ResponseCurveType.Linear, 1f, 1f, 0f, 0f, initialInput: 1f));
    }

    public override void UpdateInputs()
    {
        float alert = sightCone != null ? sightCone.alertLevel : 0f;
        FeedSingleConsideration(1f -  alert);
    }

    public override void Execute()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        navMeshAgent.SetDestination(patrolPoints[currentWaypoint].position);

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= arrivalThreshold)
        {
            currentWaypoint = (currentWaypoint + 1) % patrolPoints.Length;
        }
    }
}
