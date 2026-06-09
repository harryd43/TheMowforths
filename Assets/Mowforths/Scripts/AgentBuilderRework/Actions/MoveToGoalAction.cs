using UnityEngine;

// Agent to goal
public class MoveToGoalAction : ActionBehaviour
{
    [Header("MoveToGoal Settings")]
    [SerializeField] private float arrivalThreshold = 0.5f;
    [Tooltip("Override speed when action runs, leave at 0 for default speed")]
    [SerializeField] private float moveSpeed = 0f;
    [SerializeField, Range(0f, 10f)] private float decisionWeight = 5f;

    public override float? SpeedOverride => moveSpeed > 0f ? (float?)moveSpeed : null;

    protected override void SetupDefaults()
    {
        actionName = "Move To Goal";
        weight = decisionWeight;
        considerations.Clear();
        considerations.Add(MakeConsideration("Has goal",
            ResponseCurveType.Linear, 1f, 1f, 0f, 0f));
    }

    public override void UpdateInputs()
    {
        FeedSingleConsideration(agent.HasGoal ? 1f : 0f);
    }

    public override void Execute()
    {
        if (!agent.HasGoal)
        {
            return;
        }

        navMeshAgent.SetDestination(agent.AssignedGoal);

        if (!navMeshAgent.pathPending && navMeshAgent.hasPath && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + arrivalThreshold)
        {
            agent.ClearGoal();
        }
    }
}
