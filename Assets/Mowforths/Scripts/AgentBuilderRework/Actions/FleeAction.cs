using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

// flee action rework

public class FleeAction : ActionBehaviour
{
    [Header("Flee Settings")]
    [SerializeField] private string tagToFleeFrom = "Hostile";
    [SerializeField] private float detectionDistance = 7f;
    [SerializeField] private float fleeSampleDistance = 8f;
    [SerializeField] private int sampleCount = 8;
    [SerializeField] private float fleeSpeed = 6f;
    [SerializeField] private float decisionWeight = 9f;

    [Header("Extra Considerations")]
    [Tooltip("Second consideration to only flee when health is decreased")]
    [SerializeField] private bool useHealthConsideration = false;

    public override float? SpeedOverride => fleeSpeed;

    private MowforthHealth health;

    public override void Initialize(AgentBuilder thisAgent)
    {
        base.Initialize(thisAgent);
        health = GetComponent<MowforthHealth>();
    }

    protected override void SetupDefaults()
    {
        actionName = "Flee";
        weight = decisionWeight;
        considerations.Clear();
        considerations.Add(MakeConsideration("Hostile proximity",
            ResponseCurveType.Exponential, 3f, 2f, -1f, 0f));
        considerations.Add(MakeConsideration("Low health",
            ResponseCurveType.Linear, 1f, 1f, 0f, 0f, initialInput: 1f));
    }

    public override void UpdateInputs()
    {
        float nearestHostile;
        AgentHelper.FindNearestTagged(selfTransform.position, tagToFleeFrom, float.MaxValue, out nearestHostile);

        float fleeScore = 0f;
        if (nearestHostile <= detectionDistance) 
        {
            fleeScore = 2f - (nearestHostile / 10f);
        }

        if (considerations.Count > 0)
        {
            considerations[0].inputs[0].inputValue = fleeScore;
        }

        if (considerations.Count > 1)
        {
            if (useHealthConsideration && health != null && health.maxHealth > 0f)
            {
                considerations[1].inputs[0].inputValue = 1f - (health.currentHealth / health.maxHealth);
            }
            else
            {
                considerations[1].inputs[0].inputValue = 1f;
            }
        }
    }

    public override void Execute()
    {
        Vector3 bestFlee = selfTransform.position;
        float bestDistance = 0f;

        GameObject[] hostiles = GameObject.FindGameObjectsWithTag(tagToFleeFrom);
        if (hostiles.Length == 0)
        {
            return;
        }

        for (int i = 0; i < sampleCount; i++)
        {
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            Vector3 candidate = selfTransform.position + randomDirection * fleeSampleDistance;

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(candidate, out hit, 5f, NavMesh.AllAreas))
            {
                continue;
            }

            float totalDistanceFromHostiles = 0f;
            foreach (GameObject hostile in hostiles)
            {
                totalDistanceFromHostiles += Vector3.Distance(hit.position, hostile.transform.position);
            }
            if (totalDistanceFromHostiles > bestDistance)
            {
                bestDistance = totalDistanceFromHostiles;
                bestFlee = hit.position;
            }
        }
        navMeshAgent.SetDestination(bestFlee);
    }
}
