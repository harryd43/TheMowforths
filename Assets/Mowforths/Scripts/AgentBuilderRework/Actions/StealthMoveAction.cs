using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

// Rework from SarahBrain behaviours
public class StealthMoveAction : ActionBehaviour, IGoalListener
{
    [Header("Speed Settings")]
    [SerializeField] private float stealthSpeed = 2f;
    [SerializeField] private float panicSpeed = 6f;
    [SerializeField] private float panicThreshold = 0.3f;

    [Header("Cost-Weighted Pathfinding")]
    [Tooltip("Toggle OFF to use simple unity navmesh movement")]
    [SerializeField, Range(0f, 10f)] private float decisionWeight = 7f;
    [SerializeField] private bool useDangerCostPathfinding = true;
    [SerializeField] private int waypointChecks = 24;
    [SerializeField] private float sampleRadius = 8f;
    [SerializeField] private int segmentSamples = 5;
    [SerializeField] private float blockedSegmentCost = 9999f;
    [SerializeField] private float dangerPathRecalcThreshold = 2f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float rayHeight = 0.5f;
    [SerializeField] private float waypointArrivalThreshold = 1.5f;

    [Header("Visualisation")]
    [SerializeField] private bool showPath = true;

    [Header("Speech")]
    [SerializeField] private string[] stealthLines;
    [SerializeField, Range(0f, 1f)] private float speechProbability = 0.2f;

    private List<Vector3> stealthPath = new List<Vector3>();
    private int currentPathIndex = 0;
    private float plannedPathDanger = 0f;

    private float detectionRisk = 0f;
    private bool isPanicking = false;

    private TextSpawner textSpawner;

    public override float? SpeedOverride => isPanicking ? panicSpeed : stealthSpeed;

    public override void Initialize(AgentBuilder thisAgent)
    {
        base.Initialize(thisAgent);
        textSpawner = GetComponent<TextSpawner>();
    }

    protected override void SetupDefaults()
    {
        actionName = "StealthMovement";
        weight = decisionWeight;
        considerations.Clear();
        considerations.Add(MakeConsideration("Has goal",
            ResponseCurveType.Linear, 1f, 1f, 0f, 0f));
        considerations.Add(MakeConsideration("Safety",
            ResponseCurveType.Logistic, -15f, 1f, 1f, 0.5f));
    }

    public void OnNewGoal(Vector3 newGoal)
    {
        stealthPath.Clear();
        currentPathIndex = 0;
        isPanicking = false;
    }

    public override void UpdateInputs()
    {
        detectionRisk = 0f;
        foreach (SightCone sightCone in SightCone.allCones)
        {
            float danger = sightCone.GetPositionDanger(selfTransform.position);
            if (danger > detectionRisk)
            {
                detectionRisk = danger;
            }
        }
        detectionRisk = Mathf.Clamp01(detectionRisk);

        if (detectionRisk > panicThreshold)
        {
            isPanicking = true;
        }

        // recalc
        if (useDangerCostPathfinding && stealthPath.Count > 0 && agent.HasGoal && currentPathIndex < stealthPath.Count)
        {
            float dangerLeft = RemainingPathDanger();
            if (dangerLeft > plannedPathDanger + dangerPathRecalcThreshold)
            {
                stealthPath.Clear();
                currentPathIndex = 0;
            }
        }

        if (considerations.Count > 0) considerations[0].inputs[0].inputValue = agent.HasGoal ? 1f : 0f;
        if (considerations.Count > 1) considerations[1].inputs[0].inputValue = 1f - detectionRisk;
    }

    public override void Execute()
    {
        if (!agent.HasGoal)
        {
            return;
        }

        //norm
        if (!useDangerCostPathfinding)
        {
            navMeshAgent.SetDestination(agent.AssignedGoal);
            if (!navMeshAgent.pathPending && navMeshAgent.hasPath && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.5f)
            {
                agent.ClearGoal();
                isPanicking = false;
            }
            return;
        }
        //cost

        if (stealthPath.Count == 0)
        {
            CalculateStealthPath(agent.AssignedGoal);
            currentPathIndex = 0;
            plannedPathDanger = RemainingPathDanger();
            return;
        }

        Vector3 nextWaypoint = stealthPath[currentPathIndex];
        navMeshAgent.SetDestination(nextWaypoint);

        Vector3 flattenedAgent = new Vector3(selfTransform.position.x, 0f, selfTransform.position.z);
        Vector3 flattenedWaypoint = new Vector3(nextWaypoint.x, 0f, nextWaypoint.z);

        if (Vector3.Distance(flattenedAgent, flattenedWaypoint) < waypointArrivalThreshold)
        {
            currentPathIndex++;

            if (currentPathIndex >= stealthPath.Count)
            {
                agent.ClearGoal();
                stealthPath.Clear();
                currentPathIndex = 0;
                isPanicking = false;
                return;
            }

            if (textSpawner != null && stealthLines != null && stealthLines.Length > 0 && Random.Range(0f, 1f) < speechProbability)
            {
                textSpawner.ShowText(stealthLines[Random.Range(0, stealthLines.Length)]);
            }
        }
    }

    private float EvaluateSegment(Vector3 from, Vector3 to)
    {
        Vector3 rayFrom = from + Vector3.up * rayHeight;
        Vector3 targetDir = (to - from);
        float segmentDistance = targetDir.magnitude;

        if (segmentDistance > 0.001f && Physics.Raycast(rayFrom, targetDir.normalized, segmentDistance, obstacleMask))
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

    private float EvaluatePath(NavMeshPath path)
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

    private float RemainingPathDanger()
    {
        if (stealthPath.Count == 0) return 0f;

        float total = 0f;
        Vector3 segmentStart = selfTransform.position;
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

    private void CalculateStealthPath(Vector3 goal)
    {
        stealthPath.Clear();

        Vector3 current = selfTransform.position;
        float totalDistance = Vector3.Distance(current, goal);
        int steps = Mathf.Max(3, Mathf.RoundToInt(totalDistance / 3f));

        Vector3 travelDirection = (goal - current).normalized;
        Vector3 perpendicularDirection = new Vector3(-travelDirection.z, 0f, travelDirection.x);

        for (int step = 0; step < steps; step++)
        {
            Vector3 pathOrigin = step == 0 ? selfTransform.position : stealthPath[stealthPath.Count - 1];
            float t = (float)(step + 1) / steps;
            Vector3 directPoint = Vector3.Lerp(current, goal, t);
            Vector3 bestPoint = directPoint;
            float bestScore = blockedSegmentCost;

            // Try direct route first
            NavMeshPath directPath = new NavMeshPath();
            if (NavMesh.CalculatePath(pathOrigin, directPoint, NavMesh.AllAreas, directPath)
                && directPath.status == NavMeshPathStatus.PathComplete)
            {
                bestScore = EvaluatePath(directPath);
            }

            // perp offsets
            float[] perpendicularOffsets = { 3f, 5f, 7f, -3f, -7f, 2f, -2f };
            foreach (float offset in perpendicularOffsets)
            {
                Vector3 candidate = directPoint + perpendicularDirection * offset;
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit perpHit, 2f, NavMesh.AllAreas))
                {
                    continue;
                }
                candidate = perpHit.position;

                NavMeshPath testPath = new NavMeshPath();
                if (!NavMesh.CalculatePath(pathOrigin, candidate, NavMesh.AllAreas, testPath)) 
                {
                    continue;
                }
                if (testPath.status != NavMeshPathStatus.PathComplete) 
                {
                    continue;
                } 

                float segmentCost = EvaluatePath(testPath);
                float distancePenalty = Vector3.Distance(directPoint, candidate) * 0.05f;
                float totalScore = segmentCost + distancePenalty;

                if (totalScore < bestScore)
                {
                    bestScore = totalScore;
                    bestPoint = candidate;
                }
            }

            // rand samples
            for (int i = 0; i < waypointChecks; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * sampleRadius;
                Vector3 candidate = directPoint + new Vector3(randomOffset.x, 0f, randomOffset.y);
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit randHit, 2f, NavMesh.AllAreas))
                {
                    continue;  
                }
                candidate = randHit.position;

                NavMeshPath testPath = new NavMeshPath();
                if (!NavMesh.CalculatePath(pathOrigin, candidate, NavMesh.AllAreas, testPath)) 
                {
                    continue;
                }
                if (testPath.status != NavMeshPathStatus.PathComplete)
                {
                    continue;
                }
                float segmentCost = EvaluatePath(testPath);
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

    private void OnDrawGizmos()
    {
        if (!showPath || stealthPath == null || stealthPath.Count == 0)
        {

            return;
        }
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
