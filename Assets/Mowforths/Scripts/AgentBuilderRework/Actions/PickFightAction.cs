using Genies.Sdk;
using System.Runtime.CompilerServices;
using UnityEngine;

// rework
public class PickFightAction : ActionBehaviour
{
    [Header("Fight Settings")]
    [SerializeField] private string targetTag = "Hostile";
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 50f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField, Range(0f, 10f)] private float pickFightWeight = 10f;

    [Header("Voice Lines")]
    [SerializeField] private string[] attackLines;
    [SerializeField, Range(0f, 1f)] private float lineProbability = 0.4f;

    [Header("Action sorting")]
    [Tooltip("If chase target is activated, use the chased target to attack instead of running multiple searches")]
    [SerializeField] private bool useChaseTarget = true;

    private float attackTimer = 0f;
    private GameObject currentTarget;
    private float distanceToTarget = float.MaxValue;

    private ChaseTargetAction chaseTargetAction;
    private TextSpawner textSpawner;

    public override float? SpeedOverride => chaseSpeed;

    public override void Initialize(AgentBuilder thisAgent)
    {
        base.Initialize(thisAgent);
        chaseTargetAction = GetComponent<ChaseTargetAction>();
        textSpawner = GetComponent<TextSpawner>();
    }

    protected override void SetupDefaults()
    {
        actionName = "PickFight";
        weight = pickFightWeight;
        considerations.Clear();
        considerations.Add(MakeConsideration("Target close",
            ResponseCurveType.Exponential, 3f, 2f, -1f, 0f));
    }

    public override void UpdateInputs()
    {
        if (useChaseTarget && chaseTargetAction != null && chaseTargetAction.CurrentTarget != null)
        {
            currentTarget = chaseTargetAction.CurrentTarget;
            distanceToTarget = chaseTargetAction.DistanceToTarget;
        }
        else
        {
            currentTarget = AgentHelper.FindNearestTagged(selfTransform.position, targetTag, detectionRadius, out distanceToTarget);
        }

        float proximityScore = AgentHelper.ProximityScore(distanceToTarget, detectionRadius);
        FeedSingleConsideration(proximityScore);
    }

    public override void Execute()
    {
        attackTimer -= Time.deltaTime;
        if (currentTarget == null)
        {
            return;
        }
        navMeshAgent.SetDestination(currentTarget.transform.position);

        if (distanceToTarget <= attackRange && attackTimer <= 0f)
        {
            attackTimer = attackCooldown;

            MowforthHealth health = currentTarget.GetComponent<MowforthHealth>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);
            }

            if (textSpawner != null && attackLines != null && attackLines.Length > 0 && Random.Range(0f,1f) < lineProbability)
            {
                textSpawner.ShowText(attackLines[Random.Range(0, attackLines.Length)]);
            }
        }
    }
}
