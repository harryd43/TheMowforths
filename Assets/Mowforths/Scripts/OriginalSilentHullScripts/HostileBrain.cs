using UnityEngine;
using UnityEngine.AI;


public class HostileBrain : UtilityAgent
{
    [Header("HostileSettings")]
    [SerializeField] private float patrolRadius = 8f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float patrolSpeed = 2f;

    private Vector3 spawnPoint;
    private Transform target;
    private float attackTimer = 0f;
    private float proximityScore = 0f;
    private bool isReturning = false;

    private UtilityAction patrolAction;
    private UtilityAction chaseAction;
    private UtilityAction attackAction;

    private TextSpawner textSpawner;

    private string[] hostileLines =
    {
        "I'M GONNA DO YA!",
        "YOU NEED SLAPPIN DOWN!",
        "LET'S HAVE IT!",
        "YOU WANT SOME?"
    };

    protected override void Start()
    {
        base.Start();
        spawnPoint = transform.position;
        textSpawner = GetComponent<TextSpawner>();
        SetupActions();
    }

    void SetupActions()
    {
        actions.Clear();

        // Patrol always availiable
        patrolAction = new UtilityAction();
        patrolAction.name = "patrol";
        patrolAction.actionType = ActionType.GetLostAndWander;
        patrolAction.weight = 3f;

        Consideration alwaysPatrol = new Consideration();
        alwaysPatrol.name = "always patrrol";
        alwaysPatrol.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 1f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        patrolAction.considerations.Add(alwaysPatrol);
        actions.Add(patrolAction);

        // Chase, expo spike when close like shanes aggression
        chaseAction = new UtilityAction();
        chaseAction.name = "Chase";
        chaseAction.actionType = ActionType.MoveToTarget;
        chaseAction.weight = 8f;

        Consideration playerNearby = new Consideration();
        playerNearby.name = "Player nearby";
        playerNearby.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Exponential, 3f, 2f, -1f, 0f)
            }
        };
        chaseAction.considerations.Add(playerNearby);
        actions.Add(chaseAction);

        // Attack, highly weighted, only when in range
        attackAction = new UtilityAction();
        attackAction.name = "Attack";
        attackAction.actionType = ActionType.PickFight;
        attackAction.weight = 10f;

        Consideration inRange = new Consideration();
        inRange.name = "In attack range";
        inRange.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        attackAction.considerations.Add(inRange);
        actions.Add(attackAction);
    }

    protected override void UpdateInputValues()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float nearestPlayer = detectionRadius;
        target = null;

        foreach (GameObject player in players)
        {
            if (!player.gameObject.activeSelf)
            {
                continue;
            }
            float distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceFromPlayer < nearestPlayer)
            {
                nearestPlayer = distanceFromPlayer;
                target = player.transform;
            }
        }

        // Lost target
        if (target == null && isReturning)
        {
            float distanceFromSpawn = Vector3.Distance(transform.position, spawnPoint);
            if (distanceFromSpawn > patrolRadius)
            {
                isReturning = true;
            }
        }

        if (isReturning)
        {
            float distanceFromSpawn = Vector3.Distance(transform.position, spawnPoint);
            if (distanceFromSpawn <= patrolRadius)
            {
                isReturning = false;
            }
        }

        if (target != null)
        {
            isReturning = false;
        }

        // expo spike range
        proximityScore = target != null ? 1f - (nearestPlayer / detectionRadius) : 0f;

        // range and cooldown
        float attackScore = (target != null && Vector3.Distance(transform.position, target.position) <= attackRange) ? 1f : 0f;

        if (chaseAction.considerations.Count > 0)
        {
            chaseAction.considerations[0].inputs[0].inputValue = proximityScore;
        }
        if (attackAction.considerations.Count > 0)
        {
            attackAction.considerations[0].inputs[0].inputValue = attackScore;
        }
    }

    protected override void ExecuteAction(UtilityAction action)
    {
        if (isReturning)
        {
            navMeshAgent.speed = patrolSpeed;
            navMeshAgent.SetDestination(spawnPoint);
            return;
        }
        if (action.actionType == ActionType.GetLostAndWander)
        {
            navMeshAgent.speed = patrolSpeed;

            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= 0.5f)
            {
                Vector3 randomOffset = Random.insideUnitSphere * patrolRadius;
                randomOffset.y = 0;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(spawnPoint + randomOffset, out hit, patrolRadius, NavMesh.AllAreas))
                {
                    navMeshAgent.SetDestination(hit.position);
                }
            }
            return;
        }

        if (action.actionType == ActionType.MoveToTarget)
        {
            navMeshAgent.speed = chaseSpeed;

            if (target != null)
            {
                navMeshAgent.SetDestination(target.position);
            }
            return;
        }

        if (action.actionType == ActionType.PickFight)
        {
            if (target != null)
            {
                navMeshAgent.SetDestination(target.position);

                if (attackTimer <= 0f)
                {
                    attackTimer = attackCooldown;
                    MowforthHealth health = target.gameObject.GetComponent<MowforthHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(attackDamage);
                    }
                    if (textSpawner != null && Random.Range(0f, 1f) < 0.4f)
                    {
                        textSpawner.ShowText(hostileLines[Random.Range(0, hostileLines.Length)]);
                    }
                }
            }
            return;
        }
        base.ExecuteAction(action);
    }

    protected override void Update()
    {
        attackTimer -= Time.deltaTime;
        base.Update();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying ? spawnPoint : transform.position, patrolRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

}
