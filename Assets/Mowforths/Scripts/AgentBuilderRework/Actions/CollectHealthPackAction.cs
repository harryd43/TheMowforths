using UnityEngine;

// Prox and health considerations for health pack
public class CollectHealthPackAction : ActionBehaviour
{
    [Header("Pickup settings")]
    [SerializeField] private string healthTag = "Health";
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float decisionWeight = 8f;

    private GameObject nearestPack;
    private float distanceToPack = float.MaxValue;
    private MowforthHealth health;

    protected override void SetupDefaults()
    {
        actionName = "Collect health pack";
        weight = 8f;
        considerations.Clear();
        considerations.Add(MakeConsideration("Health pack proximity",
            ResponseCurveType.Linear, 1f, 1f, 0f, 0f));
        considerations.Add(MakeConsideration("Low Health",
            ResponseCurveType.Linear, 1f, 1f, 0f, 0f));
    }

    public override void Initialize(AgentBuilder thisAgent)
    {
        base.Initialize(thisAgent);
        health = GetComponent<MowforthHealth>();
    }

    public override void UpdateInputs()
    {
        nearestPack = AgentHelper.FindNearestTagged(selfTransform.position, healthTag, detectionRadius, out distanceToPack);
        float proximityScore = AgentHelper.ProximityScore(distanceToPack, detectionRadius);

        float healthScore = 0f;
        if (health !=  null&& health.maxHealth > 0f)
        {
            healthScore = 1f - (health.currentHealth / health.maxHealth);
        }

        if (considerations.Count > 0) considerations[0].inputs[0].inputValue = proximityScore;
        if (considerations.Count > 1) considerations[1].inputs[0].inputValue = healthScore;
    }

    public override void Execute()
    {
        if (nearestPack != null)
        {
            navMeshAgent.SetDestination(nearestPack.transform.position);
        }
    }
}
