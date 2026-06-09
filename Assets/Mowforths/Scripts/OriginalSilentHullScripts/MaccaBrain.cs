using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.AI;

// Extends utility agent to include Macca's specific actions and considerations, such as smashing obstacles
// Strong bias towards smashing
// Linear desire to reach goal
// Doesn't consider steal or considerations
// Buffoon - smash weight very high, sometimes smashes things he doesn't need to
public class MaccaBrain : UtilityAgent
{
    [Header("Macca Settings")]
    [SerializeField] private float smashDectectionRadius = 6.5f;
    [SerializeField] private float smashRange = 1.5f;
    [SerializeField] private float fleeSpeed;

    private GameObject nearestTarget;
    private float distanceToTarget = float.MaxValue;

    private UtilityAction moveToTargetAction;
    private UtilityAction smashObstacleAction;
    private UtilityAction idleAction;

    private TextSpawner textSpawner;

    private string[] idleLines = new string[]
    {
        "Macca like eat bricks!",
        "Macca bored Macca want SMASH!",
        "My name Macca!",
        "a...b...d...e...c....f......G!"
    };

    private string[] smashLines = new string[]
    {
        "MACCA SMASH!",
        "HAHA crate go bye bye!",
        "SMASH SMASH SMASH"
    };

    protected override void Start()
    {
        textSpawner = GetComponent<TextSpawner>();
        base.Start();
        SetupActions();
    }

    void SetupActions()
    {
        actions.Clear();

        // Move to goal action
        moveToTargetAction = new UtilityAction();
        moveToTargetAction.name = "MoveToTarget";
        moveToTargetAction.actionType = ActionType.MoveToTarget;
        moveToTargetAction.weight = 5f;

        Consideration hasTargetConsideration = new Consideration();
        hasTargetConsideration.name = "Has target to walk to";
        hasTargetConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput()
            {
                inputValue = 0f,// Updated
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        moveToTargetAction.considerations.Add(hasTargetConsideration);
        actions.Add(moveToTargetAction);

        // Smash obstacle action
        smashObstacleAction = new UtilityAction();
        smashObstacleAction.name = "SmashObstacle";
        smashObstacleAction.actionType = ActionType.SmashObstacle;
        smashObstacleAction.weight = 10f; // Very high weight to encourage smashing

        Consideration smashObstacleConsideration = new Consideration();
        smashObstacleConsideration.name = "Smash obstacle if nearby";
        smashObstacleConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput()
            {
                inputValue = 0f, // Updated
                responseCurve = new ResponseCurve(ResponseCurveType.Quadratic, 1f, 0.5f, 0f, 0f)
            }
        };
        smashObstacleAction.considerations.Add(smashObstacleConsideration);
        actions.Add(smashObstacleAction);

        // Idle action
        idleAction = new UtilityAction();
        idleAction.name = "Idle";
        idleAction.actionType = ActionType.Idle;
        idleAction.weight = 0.5f; // Lowest weight

        Consideration alwaysOpenConsideration = new Consideration();
        alwaysOpenConsideration.name = "Always open to be selected";
        alwaysOpenConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput()
            {
                inputValue = 1f, // Always available
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        idleAction.considerations.Add(alwaysOpenConsideration);
        actions.Add(idleAction);

        // Flee Action
        fleeAction = new UtilityAction();
        fleeAction.name = "Flee";
        fleeAction.actionType = ActionType.Flee;
        fleeAction.weight = 6f;

        Consideration maccaFlee = new Consideration();
        maccaFlee.name = "Flee Macca";
        maccaFlee.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Exponential, 3f, 2f, -1f, 0f)
            }
        };
        fleeAction.considerations.Add(maccaFlee);
        actions.Add(fleeAction);
    }

    // Update the input values based on world state per tick

    protected override void UpdateInputValues()
    {
        // Find nearest smashable object
        GameObject[] smashableObjects = GameObject.FindGameObjectsWithTag("Smashable");
        nearestTarget = null;
        distanceToTarget = float.MaxValue;

        foreach (GameObject smashableObject in smashableObjects)
        {
            float distance = Vector3.Distance(transform.position, NearestSurfacePoint(smashableObject));
            if (distance < distanceToTarget)
            {
                distanceToTarget = distance;
                nearestTarget = smashableObject;
            }
        }

        // Normalized score, 1 (next to it) to 0 (at max detection radius or beyond)
        float proximityScore = 0f;
        if (nearestTarget != null && distanceToTarget <= smashDectectionRadius)
        {
            proximityScore = 1f - (distanceToTarget / smashDectectionRadius);
        }

        float hasGoalScore = (hasGoal) ? 1f : 0f;

        if (moveToTargetAction.considerations.Count > 0)
        {
            moveToTargetAction.considerations[0].inputs[0].inputValue = hasGoalScore;
        }

        if (smashObstacleAction.considerations.Count > 0)
        {
            smashObstacleAction.considerations[0].inputs[0].inputValue = proximityScore;
        }

        UpdateFleeInput();
    }

    Vector3 NearestSurfacePoint(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider == null)
        {
            collider = obj.GetComponentInChildren<Collider>();
        }
        if (collider != null)
        {
            return collider.ClosestPointOnBounds(transform.position);
        }
        return obj.transform.position;
    }
    protected override void ExecuteAction(UtilityAction action)
    {
        if (action.actionType == ActionType.SmashObstacle)
        {
            if (nearestTarget != null)
            {
                Vector3 surfacePoint = NearestSurfacePoint(nearestTarget);
                navMeshAgent.SetDestination(surfacePoint);


                if (distanceToTarget <= smashRange)
                {
                    if (Random.Range(0f, 1f) < 0.0005f && textSpawner != null)
                    {
                        textSpawner.ShowText(smashLines[Random.Range(0, smashLines.Length)]);
                    }
                    Destroy(nearestTarget);
                    nearestTarget = null;
                }
            }
        }
        if (action.actionType == ActionType.Idle)
        {
            if (Random.Range(0f, 1f) < 0.0005f && textSpawner != null)
            {
                textSpawner.ShowText(idleLines[Random.Range(0, idleLines.Length)]);
            }
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
        else
        {
            base.ExecuteAction(action);
        }
    }
    

}
