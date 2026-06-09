using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.Rendering;

// Cost weighted pathfinding to navigate round guard sightlines, A* cost implementation

public class SarahBrain : UtilityAgent
{
    [Header("Sarah Settings")]
    [SerializeField] private float detectionRisk = 0f;
    [SerializeField] private float stealthMoveSpeed = 2f;
    [SerializeField] private float normalMoveSpeed = 4f;
    [SerializeField] private float policeCallCooldown = 500f;
    [SerializeField] private float dangerThreshold = 0.3f;
    [SerializeField] private float fleeSpeed = 8f;

    [Header("Stealth Pathfinding")]
    [SerializeField] private int waypointChecks = 24;
    [SerializeField] private float sampleRadius = 8f;
    [SerializeField] private bool showPathGizmos = true;
    private bool isPanicking = false;

    [Header("CheckPathfindSegments")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private int segmentSamples = 5;
    [SerializeField] private float rayHeight = 0.5f;
    [SerializeField] private float blockedSegmentCost = 9999f;

    [Header("Evaluation")]
    [SerializeField] public bool useDangerCost = true;

    private float policeCooldownTimer = 0f;
    private bool policeActive = false;
    private List<Vector3> stealthPath = new List<Vector3>();
    private int currentPathIndex = 0;
    private float plannedPathDanger = 0f;

    [SerializeField] private float replanMargin = 2f;

    private UtilityAction stealthMoveAction;
    private UtilityAction idleAction;

    private TextSpawner textSpawner;

    private string[] stealthLines = new string[]
    {
        "Sneaky, like a lady of the night",
        "Bravo 6, goin dark"
    };

    private string[] policeLines = new string[]
    {
        "POLICE!",
        "I'M CALLING THE POLICE!"
    };

    protected override void Start()
    {
        base.Start();
        textSpawner = GetComponent<TextSpawner>();
        navMeshAgent.speed = stealthMoveSpeed;
        SetupActions();
    }

    void SetupActions()
    {
        actions.Clear();

        // Stealth Move
        stealthMoveAction = new UtilityAction();
        stealthMoveAction.name = ("Sneaking");
        stealthMoveAction.actionType = ActionType.StealthMode;
        stealthMoveAction.weight = 7f;

        Consideration hasGoalConsideration = new Consideration();
        hasGoalConsideration.name = "Has Goal";
        hasGoalConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        stealthMoveAction.considerations.Add(hasGoalConsideration);

        Consideration detectionRiskConsideration = new Consideration();
        detectionRiskConsideration.name = "detectionRisk";
        detectionRiskConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Logistic,  -15f, 1f, 1f, 0.5f)
            }
        };
        stealthMoveAction.considerations.Add(detectionRiskConsideration);
        actions.Add(stealthMoveAction);

        // Idle
        idleAction = new UtilityAction();
        idleAction.name = "Idle";
        idleAction.actionType = ActionType.Idle;
        idleAction.weight = 1f;

        Consideration alwaysOpenConsideration = new Consideration();
        alwaysOpenConsideration.name = "Always availiable";
        alwaysOpenConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 1f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        idleAction.considerations.Add(alwaysOpenConsideration);
        actions.Add(idleAction);

        // Flee
        fleeAction = new UtilityAction();
        fleeAction.name = "flee";
        fleeAction.actionType= ActionType.Flee;
        fleeAction.weight = 9f;

        Consideration sarahFlee = new Consideration();
        sarahFlee.name = "Flee Sarah";
        sarahFlee.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Exponential, 3f, 2f, -1f, 0f)
            }
        };
        fleeAction.considerations.Add(sarahFlee);
        actions.Add(fleeAction);
    }

    protected override void UpdateInputValues()
    {
        policeCooldownTimer -= Time.deltaTime;

        detectionRisk = 0f;
        foreach (SightCone cone in SightCone.allCones)
        {
            float danger = cone.GetPositionDanger(transform.position);
            detectionRisk = Mathf.Max(detectionRisk, danger);
        }
        detectionRisk = Mathf.Clamp01(detectionRisk);

        if (detectionRisk > 0.3f)
        {
            isPanicking = true;
        }
        navMeshAgent.speed = isPanicking ? normalMoveSpeed * 1.5f : stealthMoveSpeed;

        bool needsRecalculation = false;

        if (stealthPath.Count > 0 && hasGoal && currentPathIndex < stealthPath.Count)
        {
            float dangerous = RemainingPathDanger();
            if (dangerous > plannedPathDanger + replanMargin)
            {
                needsRecalculation = true;
            }
        }

        if (needsRecalculation)
        {
            stealthPath.Clear();
            currentPathIndex = 0;
        }

        // Feed them considerationssss
        if (stealthMoveAction.considerations.Count > 1)
        {
            stealthMoveAction.considerations[0].inputs[0].inputValue = hasGoal ? 1f : 0f;
            stealthMoveAction.considerations[1].inputs[0].inputValue = 1f - detectionRisk;
        }

        // Police call not action anymore so can do one off when moving to goal
        if (detectionRisk > 0.6f && policeCooldownTimer <= 0f && !policeActive)
        {
            CallPolice();
        }

        if (policeCooldownTimer <= 0f)
        {
            policeActive = false;
        }
        UpdateFleeInput();
    }

    protected override void ExecuteAction(UtilityAction action)
    {
        if (action.actionType == ActionType.StealthMode)
        {
            if (hasGoal)
            {
                if (stealthPath.Count == 0)
                {
                    CalculateStealthPath(assignedGoal);
                    currentPathIndex = 0;
                    return;
                }
               
                Vector3 nextWaypoint = stealthPath[currentPathIndex];
                navMeshAgent.SetDestination(nextWaypoint);

                float distanceToWaypoint = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(nextWaypoint.x, 0, nextWaypoint.z));

                if (distanceToWaypoint < 1.5f)
                {
                    currentPathIndex++;

                    if (currentPathIndex >= stealthPath.Count)
                    {
                        hasGoal = false;
                        stealthPath.Clear();
                        currentPathIndex = 0;
                        isPanicking = false;
                        return;
                    }

                    if (Random.Range(0f, 1f) < 0.2f && textSpawner != null)
                    {
                        textSpawner.ShowText(stealthLines[Random.Range(0, stealthLines.Length)]);
                    }
                }
            }
            return;
        }
        if (action.actionType == ActionType.Flee)
        {
            Vector3 bestFlee = transform.position;
            float bestDistance = 0f;

            GameObject[] hostiles = GameObject.FindGameObjectsWithTag("Hostile");

            for (int i = 0; i < 8; i++)
            {
                Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                Vector3 candidate = transform.position + randomDirection * fleeSpeed;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(candidate, out hit, 5f, NavMesh.AllAreas))
                {
                    float distanceFromHostiles = 0f;
                    foreach (GameObject hostile in hostiles)
                    {
                        distanceFromHostiles += Vector3.Distance(hit.position, hostile.transform.position);
                    }
                    if (distanceFromHostiles > bestDistance)
                    {
                        bestDistance = distanceFromHostiles;
                        bestFlee = hit.position;
                    }
                }
            }
            navMeshAgent.SetDestination(bestFlee);
            return;
        }
        base.ExecuteAction(action);
    }

    // For stealth pathfind dont plot trhoguh obj
    LayerMask ObstacleMask()
    {
        if (obstacleMask.value != 0)
        {
            return obstacleMask;
        }
        foreach (SightCone cone in SightCone.allCones)
        {
            if (cone.obstacleMask.value != 0)
            {
                return cone.obstacleMask;
            }
        }
        return ~0;
    }

    // Ray the points
    float EvaluateSegment(Vector3 from, Vector3 to)
    {
        Vector3 rayFrom = from + Vector3.up * rayHeight;
        Vector3 rayTo = to + Vector3.up * rayHeight;
        Vector3 targetDir = rayTo - rayFrom;
        float segmentDistance = targetDir.magnitude;

        if (segmentDistance > 0.001f && Physics.Raycast(rayFrom, targetDir.normalized, segmentDistance, ObstacleMask()))
        {
            return blockedSegmentCost;
        }

        int samples = Mathf.Max(2, segmentSamples);
        float danger = 0f;
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / (samples - 1);
            Vector3 samplePoint = Vector3.Lerp(from, to, t);
            foreach (SightCone cone in SightCone.allCones)
            {
                danger += cone.GetPositionDanger(samplePoint);
            }
        }
        return danger / samples;
    }

    float EvaluatePath(NavMeshPath path)
    {
        if (path == null || path.corners == null || path.corners.Length < 2)
        {
            return blockedSegmentCost;
        }

        float total = 0f;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            total += EvaluateSegment(path.corners[i], path.corners[i + 1]);
        }
        return total;
    }

    float RemainingPathDanger()
    {
        if (stealthPath.Count == 0)
        {
            return 0f;
        }

        float total = 0f;
        Vector3 segmentStart = transform.position;
        for (int i = currentPathIndex; i < stealthPath.Count; i++)
        {
            int samples = Mathf.Max(2, segmentSamples);
            for (int sample = 0; sample < samples; sample++)
            {
                float t = (float)sample / (samples - 1);
                Vector3 p = Vector3.Lerp(segmentStart, stealthPath[i], t);
                foreach (SightCone cone in SightCone.allCones)
                {
                    total += cone.GetPositionDanger(p);
                }
            }
            segmentStart = stealthPath[i];
        }
        return total;
    }



    // Cost weighted pathfinding

    void CalculateStealthPath(Vector3 goal)
    {
        if (!useDangerCost)
        {
            stealthPath.Clear();
            stealthPath.Add(goal);
            return;
        }


        stealthPath.Clear();

        Vector3 current = transform.position;
        float totalDistance = Vector3.Distance(current, goal);
        int steps = Mathf.Max(3, Mathf.RoundToInt(totalDistance / 3f));

        Vector3 travelDirection = (goal - current).normalized;
        Vector3 perpendicularDirection = new Vector3(-travelDirection.z, 0, travelDirection.x);

        for (int step = 0; step < steps; step++)
        {
            Vector3 pathOrigin = step == 0 ? transform.position : stealthPath[stealthPath.Count - 1];
            float t = (float)(step + 1) / steps;
            Vector3 directPoint = Vector3.Lerp(current, goal, t);
            Vector3 bestPoint = directPoint;
            float bestScore = blockedSegmentCost;

            NavMeshPath directPath = new NavMeshPath();
            if (NavMesh.CalculatePath(pathOrigin, directPoint, NavMesh.AllAreas, directPath) && directPath.status == NavMeshPathStatus.PathComplete)
            {
                bestScore = EvaluatePath(directPath);
            }

            float[] perpendicularOffsets = { 3f, 5f, 7f, -3f, -7f, 2f, -2f };
            foreach (float offset in perpendicularOffsets)
            {
                Vector3 candidate = directPoint + perpendicularDirection * offset;

                NavMeshHit hitted;
                if (!NavMesh.SamplePosition(candidate, out hitted, 2f, NavMesh.AllAreas))
                {
                    continue;
                }
                candidate = hitted.position;

                // Account for obj
                NavMeshPath testThePath = new NavMeshPath();
                if (!NavMesh.CalculatePath(pathOrigin, candidate, NavMesh.AllAreas, testThePath))
                {
                    continue;
                }
                if (testThePath.status != NavMeshPathStatus.PathComplete)
                {
                    continue;
                }

                float segmentCost = EvaluatePath(testThePath);
                float distancePenalty = Vector3.Distance(directPoint, candidate) * 0.05f;
                float totalScore = segmentCost + distancePenalty;

                if (totalScore < bestScore)
                {
                    bestScore = totalScore;
                    bestPoint = candidate;
                }
            }

            // Trying random samples incase non perp detours are better to avoid cone
            for (int i = 0; i < waypointChecks; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * sampleRadius;
                Vector3 candidate = directPoint + new Vector3(randomOffset.x, 0, randomOffset.y);

                NavMeshHit hit;
                if (!NavMesh.SamplePosition(candidate,out hit, 2f, NavMesh.AllAreas))
                {
                    continue;
                }
                candidate = hit.position;

                // Account for obj
                NavMeshPath testThePath = new NavMeshPath();
                if (!NavMesh.CalculatePath(pathOrigin, candidate, NavMesh.AllAreas, testThePath))
                {
                    continue;
                }
                if (testThePath.status != NavMeshPathStatus.PathComplete)
                {
                    continue;
                }

                float segmentCost = EvaluatePath(testThePath);
                float distancePenalty = Vector3.Distance(directPoint, candidate) * 0.05f;
                float totalScore = segmentCost + distancePenalty;

                if (totalScore < bestScore)
                {
                    bestScore = totalScore;
                    bestPoint = candidate;
                }
            }
            stealthPath.Add(bestPoint);
        }
        stealthPath.Add(goal);
    }

    void CallPolice()
    {
        policeActive = true;
        policeCooldownTimer = policeCallCooldown;

        if (textSpawner != null)
        {
            textSpawner.ShowText(policeLines[Random.Range(0, policeLines.Length)]);
        }

        // Stun Guards
        GameObject[] guards = GameObject.FindGameObjectsWithTag("Guard");
        foreach(GameObject guard in guards)
        {
            GuardBrain brain = guard.GetComponent<GuardBrain>();
            SightCone cone = guard.GetComponent<SightCone>();
            if (brain != null)
            {
                brain.StartCoroutine(StunGuard(brain, cone));
            }
        }
    }

    System.Collections.IEnumerator StunGuard(GuardBrain brain, SightCone cone)
    {
        NavMeshAgent agent = brain.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
        }
        if (cone != null)
        {
            cone.alertLevel = 0f;
        }

        if (brain.GetComponent<TextSpawner>() != null)
        {
            brain.GetComponent<TextSpawner>().ShowText("AHHHH");
        }

        yield return new WaitForSeconds(5f);

        if (agent != null) agent.isStopped = false;

    }

    public override void SetGoal(Vector3 goal)
    {
        stealthPath.Clear();
        currentPathIndex = 0;
        isPanicking = false;
        base.SetGoal(goal);
    }

    private void OnDrawGizmos()
    {
        if (!showPathGizmos || stealthPath == null || stealthPath.Count == 0)
            return;

        Gizmos.color = Color.magenta;
        Vector3 prev = transform.position;
        foreach (Vector3 point in stealthPath)
        {
            Gizmos.DrawLine(prev, point);
            Gizmos.DrawSphere(point, 0.2f);
            prev = point;
        }
    }
}
