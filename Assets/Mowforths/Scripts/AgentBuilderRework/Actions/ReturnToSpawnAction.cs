using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class ReturnToSpawnAction : ActionBehaviour
{
    [Header("Return Settings")]
    [SerializeField] private float patrolRadius = 8f;
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private float arrivalThreshold = 0.5f;
    [SerializeField] private float decisionWeight = 9f;

    private Vector3 spawnPoint;
    private ChaseTargetAction chaseTargetAction;

    public override float? SpeedOverride => returnSpeed;

    public override void Initialize(AgentBuilder thisAgent)
    {
        base.Initialize(thisAgent);
        spawnPoint = selfTransform.position;
        chaseTargetAction = GetComponent<ChaseTargetAction>();
    }

    protected override void SetupDefaults()
    {
        actionName = "Return";
        weight = decisionWeight;
        considerations.Clear();
        considerations.Add(MakeConsideration("Spawn far away",
            ResponseCurveType.Linear, 1f, 1f, 0f, 0f));
        considerations.Add(MakeConsideration("No target",
            ResponseCurveType.Linear, 1f, 1f, 0f, 0f, initialInput: 1f));
    }

    public override void UpdateInputs()
    {
        float distanceFromSpawn = Vector3.Distance(selfTransform.position, spawnPoint);
        float farScore = distanceFromSpawn > patrolRadius ? 1f : 0f;

        bool hasTarget = chaseTargetAction != null && chaseTargetAction.CurrentTarget != null;
        float noTargetScore = hasTarget ? 0f : 1f;

        if (considerations.Count > 0) considerations[0].inputs[0].inputValue = farScore;
        if (considerations.Count > 1) considerations[1].inputs[0].inputValue = noTargetScore;
    }

    public override void Execute()
    {
        navMeshAgent.SetDestination(spawnPoint);
    }
}
