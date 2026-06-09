using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Experimental.AI;
using NUnit.Framework.Interfaces;

// Utility AI extending agent
// Low alert - patrol
// Mid alert - investigate last known position
// High alert - chase and attack
// Return - Lost target
public class GuardBrain : UtilityAgent
{
    [Header("Guard Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;

    private float attackTimer = 0f;
    private int currentWaypoint = 0;
    private SightCone sightCone;

    private UtilityAction patrolAction;
    private UtilityAction investigationAction;
    private UtilityAction attackAction;
    private UtilityAction idleAction;

    private bool spoken = false;

    private TextSpawner textSpawner;

    private string[] guardLines = new string[]
    {
        "OI!",
        "STOP RIGHT THERE",
        "CRIMINAL!",
        "COME HERE!"
    };

    protected override void Start()
    {
        base.Start();
        sightCone = GetComponent<SightCone>();
        textSpawner = GetComponent<TextSpawner>();
        navMeshAgent.speed = patrolSpeed;
        SetupActions();
    }

    void SetupActions()
    {
        actions.Clear();

        // Patrol
        patrolAction = new UtilityAction();
        patrolAction.name = "Patrol";
        patrolAction.actionType = ActionType.MoveToTarget;
        patrolAction.weight = 4f;

        Consideration lowAlertConsideration = new Consideration();
        lowAlertConsideration.name = "Calm";
        lowAlertConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        patrolAction.considerations.Add(lowAlertConsideration);
        actions.Add(patrolAction);

        // Investigate
        investigationAction = new UtilityAction();
        investigationAction.name = "Investigate";
        investigationAction.actionType = ActionType.GetLostAndWander;
        investigationAction.weight = 6f;

        Consideration suspiciousConsideration = new Consideration();
        suspiciousConsideration.name = "Suspicious";
        suspiciousConsideration.inputs = new ConsiderationInput[]
        {

            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Quadratic, 1f, 0.5f, 0f, 0f)
            }
        };
        investigationAction.considerations.Add(suspiciousConsideration);
        actions.Add(investigationAction);

        // Attack
        attackAction = new UtilityAction();
        attackAction.name = "Attack";
        attackAction.actionType = ActionType.PickFight;
        attackAction.weight = 10f; //override

        Consideration confirmedTargetConsideration = new Consideration();
        confirmedTargetConsideration.name = "Got target";
        confirmedTargetConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Exponential, 3f, 2f, -1f, 0f)
            }
        };
        attackAction.considerations.Add(confirmedTargetConsideration);
        actions.Add(attackAction);

        // Idle
        UtilityAction idleAction = new UtilityAction();
        idleAction.name = "Idle";
        idleAction.actionType = ActionType.Idle;
        idleAction.weight = 0.5f;

        Consideration alwaysOptionConsideration = new Consideration();
        alwaysOptionConsideration.name = "Always availiable";
        alwaysOptionConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 1f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        idleAction.considerations.Add(alwaysOptionConsideration);
        actions.Add(idleAction);

    }

    protected override void UpdateInputValues()
    {
        float alert = sightCone != null ? sightCone.alertLevel : 0f;
        bool hasTarget = sightCone != null && sightCone.detectedTarget != null;

        if (!hasTarget)
        {
            spoken = false;
        }

        // Feed considerations
        if (patrolAction.considerations.Count > 0)
        {
            patrolAction.considerations[0].inputs[0].inputValue = 1f - alert;
        }
        if (investigationAction.considerations.Count > 0)
        {
            investigationAction.considerations[0].inputs[0].inputValue = alert;
        }
        if (attackAction.considerations.Count > 0)
        { 
            attackAction.considerations[0].inputs[0].inputValue = hasTarget ? alert : 0f;
        }
    }

    protected override void ExecuteAction(UtilityAction action)
    {
        if (action.actionType == ActionType.MoveToTarget)
        {
            navMeshAgent.speed = patrolSpeed;
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                navMeshAgent.SetDestination(patrolPoints[currentWaypoint].position);
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= 0.5f)
                {
                    currentWaypoint = (currentWaypoint + 1) % patrolPoints.Length;
                }
            }
            return;
        }
        if (action.actionType == ActionType.GetLostAndWander)
        {
            navMeshAgent.speed = patrolSpeed;
            if (sightCone != null && sightCone.hasSeenTarget)
            {
                navMeshAgent.SetDestination(sightCone.lastSeenPosition);
            }
            else
            {
                navMeshAgent.SetDestination(transform.position);
            }
            return;
        }
        if (action.actionType == ActionType.PickFight)
        {
            navMeshAgent.speed = chaseSpeed;
            if (sightCone != null && sightCone.detectedTarget != null)
            {
                navMeshAgent.SetDestination(sightCone.detectedTarget.transform.position);
            
                float distanceToTarget = Vector3.Distance(transform.position, sightCone.detectedTarget.transform.position);

                if (distanceToTarget <= attackRange && attackTimer <= 0f)
                {
                    if (textSpawner != null && !spoken)
                    {
                        textSpawner.ShowText(guardLines[Random.Range(0, guardLines.Length)]);
                    }
                    attackTimer = attackCooldown;

                    MowforthHealth health = sightCone.detectedTarget.GetComponent<MowforthHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(999f);
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
    
}
