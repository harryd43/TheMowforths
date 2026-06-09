using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine.SocialPlatforms.Impl;


// Script to add to the agent, used to build and selected components to enable 
// Reworked from original system used to create unique agents for demo

[RequireComponent(typeof(NavMeshAgent))]
public class AgentBuilder : UtilityAgent
{
    [Header("Agent Builder")]
    [Tooltip("Default speed before an action overrides")]
    [SerializeField] private float defaultSpeed = 5f;

    [Tooltip("Action re-evaluation interval (seconds)")]
    [SerializeField] private float decisionRate = 0.2f;

    [Tooltip("Debugging tool for displaying actions above agent")]
    [SerializeField] private bool showActionLabel = false;

    // List of actions replaces previous utility action list from old system
    private List<ActionBehaviour> selectedActions = new List<ActionBehaviour>();
    private ActionBehaviour currentActionBehaviour;

    private float decisionTimer = 0f;
    private TextMesh labelMesh;

    // goal & action state readers
    public bool HasGoal { get { return hasGoal; } set { hasGoal = value; } }
    public Vector3 AssignedGoal { get { return assignedGoal; } }
    public NavMeshAgent NavMeshAgent { get { return navMeshAgent; } }
    public float DefaultSpeed { get { return defaultSpeed; } }

    public ActionBehaviour CurrentAction { get { return currentActionBehaviour; } }

    protected override void Start()
    {
        base.Start();

        // Get actions sorted
        selectedActions.Clear();
        selectedActions.AddRange(GetComponents<ActionBehaviour>());

        foreach (ActionBehaviour action in selectedActions)
        {
            action.Initialize(this);
        }

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = DefaultSpeed;
        }

        if (showActionLabel)
        {
            CreateLabel();
        }
    }

    protected override void Update()
    {
        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0f)
        {
            decisionTimer = decisionRate;
            MakeDecision();
        }

        if (currentActionBehaviour != null)
        {
            currentActionBehaviour.Execute();
        }
    }

    private void MakeDecision()
    {
        ActionBehaviour bestAction = null;
        float bestScore = 0f;

        foreach (ActionBehaviour action in selectedActions)
        {
            if (action == null || !action.enabled)
            {
                continue;
            }

            action.UpdateInputs();
            float actionScore = action.CalculateUtility();

            if (actionScore > bestScore)
            {
                bestScore = actionScore;
                bestAction = action;
            }
        }

        if (bestAction == null)
        {
            return;
        }

        if (bestAction != currentActionBehaviour)
        {
            if (currentActionBehaviour != null)
            {
                currentActionBehaviour.OnDeactivated();
            }
            currentActionBehaviour = bestAction;

            if (navMeshAgent != null)
            {
                navMeshAgent.speed = currentActionBehaviour.SpeedOverride ?? defaultSpeed;
            }

            if (labelMesh != null)
            {
                string agentName = agentController != null ? agentController.agentName : "Agent";
                labelMesh.text = agentName + "\n" + currentActionBehaviour.actionName + "\n(" + bestScore.ToString("F2") + ")";
            }
        }
        else
        {
            if (navMeshAgent != null)
            {
                navMeshAgent.speed = currentActionBehaviour.SpeedOverride ?? defaultSpeed;
            }
        }
    }

    public override void SetGoal(Vector3 goal)
    {
        base.SetGoal(goal);

        foreach (ActionBehaviour action in selectedActions)
        {
            if (action is IGoalListener listener)
            {
                listener.OnNewGoal(goal);
            }
        }
    }

    public override bool CanAcceptGoal()
    {
        return true;
    }

    public void ClearGoal()
    {
        hasGoal = false;
    }

    private void CreateLabel()
    {
        GameObject text = new GameObject("ActionLabel");
        text.transform.SetParent(transform);
        text.transform.localPosition = new Vector3(0, 2.5f, 0);
        labelMesh = text.AddComponent<TextMesh>();
        labelMesh.text = agentController != null ? agentController.agentName : "Agent";
        labelMesh.fontSize = 14;
        labelMesh.color = Color.white;
        labelMesh.anchor = TextAnchor.MiddleCenter;
        labelMesh.alignment = TextAlignment.Center;
    }
}
public interface IGoalListener // using for moving stealth path into this system as a toggle
{
    void OnNewGoal(Vector3 goal);
}

