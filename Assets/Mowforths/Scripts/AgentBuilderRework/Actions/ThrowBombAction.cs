using UnityEngine;

public class ThrowBombAction : ActionBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private string bombableTag = "Bombable";
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float throwRange = 5f;
    [SerializeField] private float bombForce = 8f;
    [SerializeField] private float bombCooldown = 3f;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private float decisionWeight = 8f;
    [SerializeField] public int bombCount = 0;

    [Header("Voice lines")]
    [SerializeField] private string[] bombLines;

    private GameObject nearestBombable;
    private float distanceToBombable = float.MaxValue;
    private float bombTimer = 0f;

    private TextSpawner textSpawner;

    public override void Initialize(AgentBuilder thisAgent)
    {
        base.Initialize(thisAgent);
        textSpawner = GetComponent<TextSpawner>();
    }

    protected override void SetupDefaults()
    {
        actionName = "Throw Bomb";
        weight = decisionWeight;
        considerations.Clear();
        considerations.Add(MakeConsideration("Prox",
            ResponseCurveType.Exponential, 3f, 2f, -1f, 0f));
        considerations.Add(MakeConsideration("Bomb available",
            ResponseCurveType.Linear, 1f, 1f, 0f, 0f, initialInput: 1f));
    }

    public override void UpdateInputs()
    {
        bombTimer -= Time.deltaTime;

        nearestBombable = null;
        distanceToBombable = float.MaxValue;
        GameObject[] bombables = GameObject.FindGameObjectsWithTag(bombableTag);
        foreach(GameObject bombable in bombables)
        {
            float dist = Vector3.Distance(selfTransform.position, AgentHelper.NearestSurfacePoint(bombable, selfTransform.position));
            if (dist < distanceToBombable)
            {
                distanceToBombable = dist;
                nearestBombable= bombable;
            }
        }

        float proximityScore = (nearestBombable != null && distanceToBombable <= detectionRadius) ? 1f - (distanceToBombable / detectionRadius) : 0f;
        float bombReadyScore = (bombTimer <= 0f && bombCount > 0) ? 1f : 0f;

        if (considerations.Count > 0) considerations[0].inputs[0].inputValue = proximityScore;
        if (considerations.Count > 1) considerations[1].inputs[0].inputValue = bombReadyScore;
    }

    public override void Execute()
    {
        if (nearestBombable == null || bombCount <= 0 || bombPrefab == null)
        {
            return;
        }

        Vector3 surfacePoint = AgentHelper.NearestSurfacePoint(nearestBombable, selfTransform.position);
        navMeshAgent.SetDestination(surfacePoint);

        if (distanceToBombable <= throwRange && bombTimer <= 0f)
        {
            GameObject bomb = Instantiate(bombPrefab, selfTransform.position + Vector3.up * 1f, Quaternion.identity);
            Rigidbody body = bomb.GetComponent<Rigidbody>();
            if (body != null)
            {
                Vector3 throwDirection = (surfacePoint - bomb.transform.position).normalized;
                body.AddForce(throwDirection * bombForce, ForceMode.Impulse);
            }

            BombBehaviour bombBehaviour = bomb.AddComponent<BombBehaviour>();
            bombBehaviour.Init(nearestBombable);

            bombCount--;
            bombTimer = bombCooldown;

            if (textSpawner != null && bombLines != null && bombLines.Length > 0)
            {
                textSpawner.ShowText(bombLines[Random.Range(0, bombLines.Length)]);
            }
        }
    }


}
