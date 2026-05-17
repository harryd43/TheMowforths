using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.UIElements;

// Ashley rides around on his bike and grabs things, getting lost constantly
public class AshleyBrain : UtilityAgent
{
    [Header("Ashley Settings")]
    [SerializeField] private float grabDetectionRadius = 4f;
    [SerializeField] private float grabRange = 1.5f;
    [SerializeField] private float boredomBuildRate = 0.3f;
    [SerializeField] private float boredomDecayRate = 0.15f;
    [SerializeField] private float wanderStartThreshold = 0.65f;
    [SerializeField] private float wanderStopThreshold = 0.2f;

    private bool isWandering = false;  
    private float boredom = 0f;

    private float complaintCooldown = 0f;
    [SerializeField] private float complaintInterval = 4f;

    private GameObject nearestGrabbable;
    private float distanceToGrabbable = float.MaxValue;
    private bool carryingObject = false;
    private GameObject carriedObject;
    private bool wanderPathCleared = false;
    private bool hadGoal = false;

    private TextSpawner textSpawner;

    private string[] complaints = new string[]
    {
        "Where am I?",
        "I'm sick of this!",
        "I need pasta!",
        "I don't really care!",
        "I don't want to apply for a job!",
        "I need a bike pump!",
        "I hate this!",
        "Let me tell you something!",
        "You're obssessed with me!",
        "Why am I like this?",
        "I love dinosaurs!",
        "My name Ashley!",
        "Somebody said....",
        "I guess I'll just grieve and move on",
        "It's untrue!",
        "Well I'm not, am I?",
        "Well I don't know do I?",
        "I'll tell you the same answer I said before!",
        "I've got nothing better to do!",
        "I forgot",
        "You what?",
        "Same answer as always!",
        "Because I said so",
        "I cant be bothered!"
    };

    private UtilityAction moveToTargetAction;
    private UtilityAction getLostAndWanderAction;
    private UtilityAction grabObjectAction;
    private UtilityAction idleAction;

    protected override void Start()
    {
        base.Start();
        textSpawner = GetComponent<TextSpawner>();
        SetupActions();
    }

    void SetupActions()
    {
        actions.Clear();

        // Move to target action
        moveToTargetAction = new UtilityAction();
        moveToTargetAction.name = "Try cycle to target";
        moveToTargetAction.actionType = ActionType.MoveToTarget;
        moveToTargetAction.weight = 7f;

        // Has goal consideration
        Consideration hasTargetConsideration = new Consideration();
        hasTargetConsideration.name = "Has Target";
        hasTargetConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f,1f,0f,0f)
            }
        };
        moveToTargetAction.considerations.Add(hasTargetConsideration);

        // Boredom consideration
        Consideration nonboredomConsideration = new Consideration();
        nonboredomConsideration.name = "Not Bored";
        nonboredomConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Quadratic, 1f, 0.5f, 0f, 0f)
            }
        };
        moveToTargetAction.considerations.Add(nonboredomConsideration);
        actions.Add(moveToTargetAction);

        // Wander action
        getLostAndWanderAction = new UtilityAction();
        getLostAndWanderAction.name = "Get lost and wander";
        getLostAndWanderAction.actionType = ActionType.GetLostAndWander;
        getLostAndWanderAction.weight = 5f;

        // Wander consideration
        Consideration wanderConsideration = new Consideration();
        wanderConsideration.name = "Bored or lost";
        wanderConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        getLostAndWanderAction.considerations.Add(wanderConsideration);
        actions.Add(getLostAndWanderAction);

        // Grab object action
        grabObjectAction = new UtilityAction();
        grabObjectAction.name = "I'M GONNA GRAB YA";
        grabObjectAction.actionType = ActionType.GrabTarget;
        grabObjectAction.weight = 8f;

        Consideration grabProximity = new Consideration();
        grabProximity.name = "Grabbable close";
        grabProximity.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        grabObjectAction.considerations.Add(grabProximity);

        // Consideration for if NOT carrying
        Consideration notCarryingConsideration = new Consideration();
        notCarryingConsideration.name = "Not carrying";
        notCarryingConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 1f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        grabObjectAction.considerations.Add(notCarryingConsideration);
        actions.Add(grabObjectAction);

        // Idle action
        idleAction = new UtilityAction();
        idleAction.name = "Idle";
        idleAction.actionType = ActionType.Idle;
        idleAction.weight = 0.5f;

        Consideration alwaysOpenConsideration = new Consideration();
        alwaysOpenConsideration.name = "Always Open";
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
    }

    protected override void UpdateInputValues()
    {
        // Boredom update
        float agentSpeed = navMeshAgent.velocity.magnitude;
        bool agentMoving = agentSpeed > 0.1f;

        if (isWandering)
        {
            boredom -= boredomDecayRate * Time.deltaTime;
        }
        else if (hasGoal && agentMoving)
        {
            boredom += boredomBuildRate * Time.deltaTime;
        }
        else if (!agentMoving)
        {
            boredom += boredomBuildRate * Time.deltaTime * 1.5f;
        }

        boredom = Mathf.Clamp01(boredom);

        if (!isWandering && boredom >= wanderStartThreshold)
        {
            isWandering = true;
        }
        else if (isWandering && boredom <= wanderStopThreshold)
        {
            isWandering = false;
        }


        // Nearest Grabbable
        GameObject[] grabbables = GameObject.FindGameObjectsWithTag("Grabbable");
        nearestGrabbable = null;
        distanceToGrabbable = float.MaxValue;

        foreach (GameObject grabbable in grabbables)
        {
            float distance = Vector3.Distance(transform.position, grabbable.transform.position);
            if (distance < distanceToGrabbable)
            {
                nearestGrabbable = grabbable;
                distanceToGrabbable = distance;
            }
        }

        // Normalise grab prox
        float grabProximityScore = 0f;
        if (nearestGrabbable != null && distanceToGrabbable <= grabDetectionRadius)
        {
            grabProximityScore = 1f - (distanceToGrabbable / grabDetectionRadius);
        }

        // Move to target
        if (moveToTargetAction.considerations.Count > 1)
        {
            moveToTargetAction.considerations[0].inputs[0].inputValue = isWandering ? 0f : (hasGoal ? 1f : 0f);
            moveToTargetAction.considerations[1].inputs[0].inputValue = 1f - boredom;
        }

        // Wander
        if (getLostAndWanderAction.considerations.Count > 0)
        {
            getLostAndWanderAction.considerations[0].inputs[0].inputValue = isWandering ? 1f : 0f;
        }

        // Grab 
        if (grabObjectAction.considerations.Count > 1)
        {
            grabObjectAction.considerations[0].inputs[0].inputValue = grabProximityScore;
            grabObjectAction.considerations[1].inputs[0].inputValue = carryingObject ? 0f : 1f;
        }

        Debug.Log("boredom: " + boredom.ToString("F2")
    + " | hasGoal: " + hasGoal
    + " | isWandering: " + isWandering
    + " | hadGoal: " + hadGoal
    + " | WANDER: " + getLostAndWanderAction.CalculateUtility().ToString("F2")
    + " | MOVE: " + moveToTargetAction.CalculateUtility().ToString("F2"));

    }

    protected override void ExecuteAction(UtilityAction action)
    {
        // Update carried obj pos
        if (carryingObject && carriedObject != null)
        {
            carriedObject.transform.position = transform.position + transform.forward * 1f + Vector3.up * 1f;
        }

        if (action.actionType == ActionType.GrabTarget)
        {
            if (nearestGrabbable != null && !carryingObject)
            {
                navMeshAgent.SetDestination(nearestGrabbable.transform.position);
                if (distanceToGrabbable <= grabRange)
                {
                    carryingObject = true;
                    carriedObject = nearestGrabbable;
                    nearestGrabbable = null;

                    Collider collider = carriedObject.GetComponent<Collider>();
                    if (collider != null)
                    {
                        collider.enabled = false;
                    }

                    Debug.Log("Ashley grabbed: " + carriedObject.name);
                }
            }
            return;
            
        }

        if (isWandering)
        {
            complaintCooldown -= Time.deltaTime;

            if (complaintCooldown <= 0f)
            {
                string complaint = complaints[Random.Range(0, complaints.Length)];
                if (textSpawner != null)
                {
                    textSpawner.ShowText(complaint);
                }

                complaintCooldown = complaintInterval + Random.Range(-1f, 2f);
            }
            if (!wanderPathCleared)
            {
                if (navMeshAgent.hasPath && navMeshAgent.remainingDistance > 0.5f)
                {
                    hadGoal = true;
                }
                navMeshAgent.ResetPath();
                wanderPathCleared = true;
            }
            if (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f)
            {
                navMeshAgent.ResetPath();
                Vector3 randomPoint = transform.position + Random.insideUnitSphere * 5f;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
                {
                    navMeshAgent.SetDestination(hit.position);
                }
            }
            
            return;
        }
        wanderPathCleared = false;
        base.ExecuteAction(action);
        
    }

    public override bool CanAcceptGoal()
    {
        return !(isWandering && hadGoal);
    }

    public override void SetGoal (Vector3 goal)
    {
        if (isWandering && hadGoal)
        {
            return;
        }
        if (!hasGoal)
        {
            isWandering = false;
            complaintCooldown = 2f;
            boredom = 0.1f;
            hadGoal = false;
            base.SetGoal(goal);
        }
        else if (hasGoal && !hadGoal)
        {
            hadGoal = false;
            base.SetGoal(goal);
        }
        else
        {
            base.SetGoal(goal);
        }
    }
}
