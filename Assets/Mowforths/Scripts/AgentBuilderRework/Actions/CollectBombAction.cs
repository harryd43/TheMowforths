using UnityEngine;
using UnityEngine.Rendering;

// goes to bomb, collision enter will handle pickup 
public class CollectBombAction : ActionBehaviour
{
    [Header("CollectBomb Settings")]
    [SerializeField] private string bombTag = "Bomb";
    [SerializeField] private float detectionRadius = 11f;
    [SerializeField] private float decisionWeight = 7f;

    private GameObject nearestBomb;
    private float distanceToBomb = float.MaxValue;
    private ThrowBombAction throwAction;

    public GameObject CurrentTarget { get { return nearestBomb; } }

    public override void Initialize(AgentBuilder thisAgent)
    {
        base.Initialize(thisAgent);
        throwAction = GetComponent<ThrowBombAction>();
    }
    protected override void SetupDefaults()
    {
        actionName = "Collect Bomb";
        weight = decisionWeight;
        considerations.Add(MakeConsideration("Bomb proximity",
            ResponseCurveType.Linear, 1f, 1f, 0f, 0f));
    }

    public override void UpdateInputs()
    {
        nearestBomb = AgentHelper.FindNearestTagged(selfTransform.position, bombTag, detectionRadius, out distanceToBomb);
        float proximityScore = AgentHelper.ProximityScore(distanceToBomb, detectionRadius);
        FeedSingleConsideration(proximityScore);
    }

    public override void Execute()
    {
        if (nearestBomb != null)
        {
            navMeshAgent.SetDestination(nearestBomb.transform.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(bombTag))
        {
            return;
        }
        if (throwAction != null)
        {
            throwAction.bombCount++;
        }
        Destroy(collision.gameObject);
        nearestBomb = null;
        distanceToBomb = float.MaxValue;
    }

}
