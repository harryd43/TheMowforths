using UnityEngine;

// Reworking guard behaviour
public class Investigate : ActionBehaviour
{
    [Header("Investigation Settings")]
    [SerializeField] private SightCone sightCone;
    [SerializeField] private float investigationSpeed = 2f;
    [SerializeField] private float decisionWeight = 6f;

    public override float? SpeedOverride => investigationSpeed;

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
        actionName = "Investigate";
        weight = decisionWeight;
        considerations.Clear();
        considerations.Add(MakeConsideration("Suspicious Alert Level",
            ResponseCurveType.Quadratic, 1f, 0.5f, 0f, 0f));
    }

    public override void UpdateInputs()
    {
        float alert = sightCone!= null ? sightCone.alertLevel : 0f;
        FeedSingleConsideration(alert);
    }

    public override void Execute()
    {
        if (sightCone != null && sightCone.hasSeenTarget)
        {
            navMeshAgent.SetDestination(sightCone.lastSeenPosition);
        }
        else
        {
            navMeshAgent.SetDestination(selfTransform.position);
        }
    }
}
