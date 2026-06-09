using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

// Scores all availiable actions and picks the one with the highest score to execute, extended by each agent type
public class UtilityAgent : MonoBehaviour
{
    [Header("Actions")]
    public List<UtilityAction> actions = new List<UtilityAction>();
    protected UtilityAction fleeAction;
    protected float nearestHostileDistance = float.MaxValue;

    [Header("Decision Making")]
    [SerializeField] private float decisionInterval = 0.2f; // how often to make a decision
    [SerializeField] private float minimumActionDuration = 2f;
    [SerializeField] private float fleeDetectionDistance = 7f;
    private float actionTimer = 0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugText = false;

    protected AgentController agentController;
    protected NavMeshAgent navMeshAgent;

    protected UtilityAction currentAction;

    private float decisionTimer = 0f; // decision interval

    protected Vector3 assignedGoal;
    protected bool hasGoal = false;

    private TextMesh debugText;

    protected virtual void Start()
    {
        agentController = GetComponent<AgentController>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (showDebugText)
        {
            CreateDebugText();
        }
    }

    protected virtual void Update()
    {
        if (hasGoal && !navMeshAgent.pathPending && navMeshAgent.hasPath && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.5f)
        {
            hasGoal = false;
        }
        decisionTimer -= Time.deltaTime;

        if (decisionTimer <= 0f)
        {
            decisionTimer = decisionInterval;
            MakeDecision();
        }

        if (currentAction != null)
        {
            ExecuteAction(currentAction);
        }
    }

    void MakeDecision()
    {
        UpdateInputValues();

        UtilityAction bestAction = null;
        float bestScore = -1f;

        foreach (UtilityAction action in actions)
        {
            float score = action.CalculateUtility();
            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        if (bestAction != null && (bestAction != currentAction))
        {
            currentAction = bestAction;
            

            if (showDebugText && debugText != null)
            {
                debugText.text = agentController.agentName + "\n" + currentAction.name + "\n(" + bestScore.ToString("F2") + ")";
            }
        }
    }

    protected virtual void UpdateInputValues()
    {
        // To be overridden by each agent type to update the input values for their considerations
    }

    protected virtual void ExecuteAction(UtilityAction action)
    {
        switch (action.actionType)
        {
            case ActionType.MoveToTarget:
                if (hasGoal)
                {
                    navMeshAgent.SetDestination(assignedGoal);
                }
                break;

            case ActionType.GetLostAndWander:
                if (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f)
                {
                    Vector3 randomPoint = transform.position + Random.insideUnitSphere * 5f;

                    UnityEngine.AI.NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
                    {
                        navMeshAgent.SetDestination(hit.position);
                    }
                }
                break;

            case ActionType.Idle:
                navMeshAgent.SetDestination(transform.position);
                break;

            default:
                break;
        }
    }

    protected void UpdateFleeInput()
    {
        GameObject[] hostiles = GameObject.FindGameObjectsWithTag("Hostile");
        nearestHostileDistance = float.MaxValue;

        foreach (GameObject hostile in hostiles)
        {
            float distance = Vector3.Distance(transform.position, hostile.transform.position);
            if (distance < nearestHostileDistance)
            {
                nearestHostileDistance = distance;
            }
        }
        float fleeScore = 0f;
        if (nearestHostileDistance <= fleeDetectionDistance)
        {
            fleeScore = 2f - (nearestHostileDistance / 10f);
        }

        if (fleeAction != null && fleeAction.considerations.Count > 0)
        {
            fleeAction.considerations[0].inputs[0].inputValue = fleeScore;
        }
    }
    public virtual void SetGoal(Vector3 goal)
    {
        assignedGoal = goal;
        hasGoal = true;
    }

    void CreateDebugText()
    {
        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 2.5f, 0);
        debugText = textObj.AddComponent<TextMesh>();
        debugText.text = agentController != null ? agentController.agentName : "Agent";
        debugText.fontSize = 14;
        debugText.color = Color.white;
        debugText.anchor = TextAnchor.MiddleCenter;
        debugText.alignment = TextAlignment.Center;
    }
    public virtual bool CanAcceptGoal()
    {
        return true;
    }
}
