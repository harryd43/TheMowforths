using UnityEngine;

// Chases tag, pick fight reads to avoid duplication of similar logic
public class ChaseTargetAction : ActionBehaviour
{
    [Header("ChaseTarget Settings")]
    [SerializeField] private string tagToChase = "Player";
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float decisionWeight = 8f;

    private GameObject currentTarget;
    private float distanceToTarget = float.MaxValue;

    public GameObject CurrentTarget { get { return currentTarget; } } 
    public float DistanceToTarget { get { return distanceToTarget; } }

    public override float? SpeedOverride => chaseSpeed;

    protected override void SetupDefaults()
    {
        actionName = "Chase";
        weight = decisionWeight;
        considerations.Clear();
        considerations.Add(MakeConsideration("Target proximity",
            ResponseCurveType.Exponential, 3f, 2f, -1f, 0f));
    }

    public override void UpdateInputs()
    {
        currentTarget = AgentHelper.FindNearestTagged(selfTransform.position, tagToChase, detectionRadius, out distanceToTarget);
        float proximityScore = AgentHelper.ProximityScore(distanceToTarget, detectionRadius);
        FeedSingleConsideration(proximityScore);
    }

    public override void Execute()
    {
        if (currentTarget != null)
        {
            navMeshAgent.SetDestination(currentTarget.transform.position);
        }
    }
}
