using UnityEngine;

// rework
public class SmashObstacleAction : ActionBehaviour
{
    [Header("Smash Settings")]
    [SerializeField] private string smashableTag = "Smashable";
    [SerializeField] private float detectionRadius = 6.5f;
    [SerializeField] private float smashRange = 1.5f;
    [SerializeField, Range(0f, 10f)] private float decisionWeight = 10f;

    [Header("Voice")]
    [SerializeField] private string[] smashLines;
    [SerializeField, Range(0f, 0.01f)] private float speechProbability = 0.0005f;

    private GameObject nearestSmashable;
    private float distanceToTarget = float.MaxValue;

    private TextSpawner textSpawner;

    public override void Initialize(AgentBuilder thisAgent)
    {
        base.Initialize(thisAgent);
        textSpawner = GetComponent<TextSpawner>();
    }

    protected override void SetupDefaults()
    {
        actionName = "Smash";
        weight = decisionWeight;
        considerations.Clear();
        considerations.Add(MakeConsideration("Prox",
            ResponseCurveType.Quadratic, 1f,0.5f, 0f, 0f));
    }

    public override void UpdateInputs()
    {
        nearestSmashable = null;
        distanceToTarget = float.MaxValue;

        GameObject[] smashables = GameObject.FindGameObjectsWithTag(smashableTag);
        foreach (GameObject smashable in smashables)
        {
            float dist = Vector3.Distance(selfTransform.position, AgentHelper.NearestSurfacePoint(smashable, selfTransform.position));
            if (dist < distanceToTarget)
            {
                distanceToTarget = dist;
                nearestSmashable = smashable;
            }
        }

        float proximityScore = (nearestSmashable != null && distanceToTarget <= detectionRadius) ? 1f - (distanceToTarget / detectionRadius) : 0f;
        FeedSingleConsideration(proximityScore);
    }

    public override void Execute()
    {
        if (nearestSmashable == null)
        {
            return;
        }

        Vector3 surfacePoint = AgentHelper.NearestSurfacePoint(nearestSmashable, selfTransform.position);
        navMeshAgent.SetDestination(surfacePoint);

        if (distanceToTarget <= smashRange)
        {
            if (textSpawner != null && smashLines != null && smashLines.Length > 0 && Random.Range(0f,1f) < speechProbability)
            {
                textSpawner.ShowText(smashLines[Random.Range(0, smashLines.Length)]);
            }
            Destroy(nearestSmashable);
            nearestSmashable = null;
        }
    }

}
