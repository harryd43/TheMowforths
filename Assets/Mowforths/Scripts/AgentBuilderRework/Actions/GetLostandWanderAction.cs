using Cysharp.Threading.Tasks.Triggers;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

// Rework from previous system
public class GetLostandWanderAction : ActionBehaviour
{
    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderSpeed = 0f;
    [SerializeField] private float decisionWeight = 2f;

    public override float? SpeedOverride => wanderSpeed > 0f ? (float?)wanderSpeed : null;

    protected override void SetupDefaults()
    {
        actionName = "Wander";
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
        if (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = selfTransform.position + Random.insideUnitSphere * wanderRadius;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, wanderRadius, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
        }
    }
}
