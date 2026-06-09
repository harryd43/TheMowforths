using UnityEngine;
using UnityEngine.Rendering;


//Idle rework, lines in inspector now
public class IdleAction : ActionBehaviour
{
    [Header("Idle Settings")]
    [Tooltip("Add voice lines to be displayed")]
    [SerializeField] private string[] idleLines;
    [SerializeField, Range(0f, 0.01f)] private float speakProbability = 0.0005f;
    [SerializeField] private float decisionWeight = 0.5f;

    private TextSpawner textSpawner;

    public override void Initialize(AgentBuilder thisAgent)
    {
        base.Initialize(thisAgent);
        textSpawner = GetComponent<TextSpawner>();
    }

    protected override void SetupDefaults()
    {
        actionName = "Idle";
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
        if (navMeshAgent != null) navMeshAgent.SetDestination(selfTransform.position);

        if (idleLines != null && idleLines.Length > 0 && textSpawner != null && Random.Range(0f, 1f) < speakProbability)
        {
            textSpawner.ShowText(idleLines[Random.Range(0, idleLines.Length)]);
        }
    }
}
