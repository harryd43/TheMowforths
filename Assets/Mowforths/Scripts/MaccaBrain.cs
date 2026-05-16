using UnityEngine;
using System.Collections.Generic;

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

    private GameObject nearestTarget;
    private float distanceToTarget = float.MaxValue;

    private UtilityAction moveToTargetAction;
    private UtilityAction smashObstacleAction;
    private UtilityAction getLostAndWanderAction;
    private UtilityAction idleAction;

    protected override void Start()
    {
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

        // Get lost and wander action
        getLostAndWanderAction = new UtilityAction();
        getLostAndWanderAction.name = "GetLostAndWander";
        getLostAndWanderAction.actionType = ActionType.GetLostAndWander;
        getLostAndWanderAction.weight = 2f; // Low weight to make it a fallback

        Consideration noGoalConsideration = new Consideration();
        noGoalConsideration.name = "No goal in sight";
        noGoalConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput()
            {
                inputValue = 0f, // Updated
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        getLostAndWanderAction.considerations.Add(noGoalConsideration);
        actions.Add(getLostAndWanderAction);

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
            float distance = Vector3.Distance(transform.position, smashableObject.transform.position);
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

        if (getLostAndWanderAction.considerations.Count > 0)
        {
            getLostAndWanderAction.considerations[0].inputs[0].inputValue = 1f - hasGoalScore; // Opposite of having a goal
        }
    }

    protected override void ExecuteAction(UtilityAction action)
    {
        if (action.actionType == ActionType.SmashObstacle)
        {
            if (nearestTarget != null)
            {
                navMeshAgent.SetDestination(nearestTarget.transform.position);


                if (distanceToTarget <= smashRange)
                {
                    Debug.Log("MACCA SMASH: " + nearestTarget.name);
                    Destroy(nearestTarget);
                    nearestTarget = null;
                }
            }
        }
        else
        {
            base.ExecuteAction(action);
        }
    }
    

}
