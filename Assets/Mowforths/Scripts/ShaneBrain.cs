using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.InputSystem;

// Shanes brain, has fights, bombs walls. Sudden exponential aggression when near a target to fight or bomb
// Drops goals to pick a fight 
public class ShaneBrain : UtilityAgent
{
    [Header("Shane Settings")]
    [SerializeField] private float fightDetectionRadius = 8f;
    [SerializeField] private float fightRange = 1.5f;
    [SerializeField] private float bombDetectionRadius = 10f;
    [SerializeField] private float bombThrowRange = 5f;
    [SerializeField] private float bombCooldown = 3f;
    [SerializeField] private float bombForce = 8f;
    [SerializeField] private GameObject bombPrefab;

    private float bombTimer = 0f;

    private GameObject nearestHostile;
    private float distanceToHostile = float.MaxValue;

    private GameObject nearestBombable;
    private float distanceToBombable = float.MaxValue;

    private UtilityAction moveToTargetAction;
    private UtilityAction pickFightAction;
    private UtilityAction throwBombAction;
    private UtilityAction getLostAndWanderAction;
    private UtilityAction idleAction;

    private TextSpawner textSpawner;

    private string[] shaneLines = new string[]
    {
        "COME ON THEN",
        "LETS BE HAVIN YA",
        "I LOOOVE TRIFLE",
        "GET OUT MY WAY",
        "WHO WANTS SOME",
        "YOU THINK DARKNESS IS YOUR ALLY?",
        "I WAS BORN IN THE DARKNESS",
        "THE SHADOWS BETRAY YOU",
        "NO ONE CARED WHO I WAS UNTIL I SMEARED TRIFLE ON MY FACE",
        "I AM NECESSARY EVIL",
        "CITIZENS OF EARTH! I AM SHANE"
    };

    private string[] shaneIdleLines = new string[]
    {
        "COME ON THEN",
        "WHO WANTS SOME!",
        "MMM I WANT TRILE",
        "WHERE IS EVERYONE?",
        "I WANNA FIGHT!",
        "I WAS BORN IN THE DARKNESS"
    };

    protected override void Start()
    {
        base.Start();
        textSpawner = GetComponent<TextSpawner>();
        SetupActions();

    }

    void SetupActions()
    {
        actions.Clear();

        // Move to target
        moveToTargetAction = new UtilityAction();
        moveToTargetAction.name = "Pick a fight";
        moveToTargetAction.actionType = ActionType.MoveToTarget;
        moveToTargetAction.weight = 4f; // Lower than bomb / fight

        Consideration hasGoalConsideration = new Consideration();
        hasGoalConsideration.name = "Has goal";
        hasGoalConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f,1f,0f,0f)
            }
        };
        moveToTargetAction.considerations.Add(hasGoalConsideration);
        actions.Add(moveToTargetAction);

        // Pick Fight
        pickFightAction = new UtilityAction();
        pickFightAction.name = "FIGHT";
        pickFightAction.actionType = ActionType.PickFight;
        pickFightAction.weight = 9f;

        Consideration fightProximityConsideration = new Consideration();
        fightProximityConsideration.name = "Hostile nearby";
        fightProximityConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Exponential, 3f, 2f, -1f, 0f)
            }
        };
        pickFightAction.considerations.Add(fightProximityConsideration);
        actions.Add(pickFightAction);

        // Throw bomb action
        throwBombAction = new UtilityAction();
        throwBombAction.name = "It's bomb o'clock";
        throwBombAction.actionType = ActionType.ThrowBomb;
        throwBombAction.weight = 8f;

        Consideration bombableProximityConsideration = new Consideration();
        bombableProximityConsideration.name = "Bombable wall close";
        bombableProximityConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Exponential, 3f, 2f, -1f, 0f)
            }
        };
        throwBombAction.considerations.Add(bombableProximityConsideration);

        Consideration bombCooldownConsideration = new Consideration();
        bombCooldownConsideration.name = "Bomb ready";
        bombCooldownConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 1f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        throwBombAction.considerations.Add(bombCooldownConsideration);
        actions.Add(throwBombAction);

        /* Wander Action
        getLostAndWanderAction = new UtilityAction();
        getLostAndWanderAction.name = "Wander";
        getLostAndWanderAction.actionType = ActionType.GetLostAndWander;
        getLostAndWanderAction.weight = 3f;

        Consideration wanderConsideration = new Consideration();
        wanderConsideration.name = "Go for walk";
        wanderConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        getLostAndWanderAction.considerations.Add(wanderConsideration);
        actions.Add(getLostAndWanderAction);*/

        // idle
        idleAction = new UtilityAction();
        idleAction.name = "Idle";
        idleAction.actionType = ActionType.Idle;
        idleAction.weight = 2f;

        Consideration alwaysOnConsideration = new Consideration();
        alwaysOnConsideration.name = "always an option";
        alwaysOnConsideration.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 1f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        idleAction.considerations.Add(alwaysOnConsideration);
        actions.Add(idleAction);
    }

    protected override void UpdateInputValues()
    {
        bombTimer -= Time.deltaTime;

        // Find nearest enemy
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Hostile");
        nearestHostile = null;
        distanceToHostile = float.MaxValue;

        foreach (GameObject enemy in enemies) 
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < distanceToHostile)
            { 
                distanceToHostile = distance;
                nearestHostile = enemy;
            }

        }

        // Find nearest bombable surface
        GameObject[] bombables = GameObject.FindGameObjectsWithTag("Bombable");
        nearestBombable = null;
        distanceToBombable = float.MaxValue;

        foreach (GameObject bombable in bombables)
        {
            float distance = Vector3.Distance(transform.position, bombable.transform.position);
            if (distance < distanceToBombable)
            {
                distanceToBombable = distance;
                nearestBombable = bombable;
            }
        }

        // Normalise prox scores
        float fightScore = 0f;
        if (nearestHostile != null && distanceToHostile <= fightDetectionRadius)
        {
            fightScore = 1f - (distanceToHostile / fightDetectionRadius);
        }

        float bombScore = 0f;
        if (nearestBombable != null &&  distanceToBombable <= bombDetectionRadius)
        {
            bombScore = 1f - (distanceToBombable / bombDetectionRadius);
        }

        float bombReady = bombTimer <= 0f ? 1f : 0f;

        // Feed considerations
        if (moveToTargetAction.considerations.Count > 0)
        {
            moveToTargetAction.considerations[0].inputs[0].inputValue = hasGoal ? 1f : 0f;
        }
        if (pickFightAction.considerations.Count > 0)
        {
            pickFightAction.considerations[0].inputs[0].inputValue = fightScore;
        }
        if (throwBombAction.considerations.Count > 1)
        {
            throwBombAction.considerations[0].inputs[0].inputValue = bombScore;
            throwBombAction.considerations[1].inputs[0].inputValue = bombReady;
        }
    }

    protected override void ExecuteAction(UtilityAction action)
    {
        if (action.actionType == ActionType.PickFight)
        {
            if (nearestHostile != null)
            {
                navMeshAgent.SetDestination(nearestHostile.transform.position);

                if (distanceToHostile <= fightRange)
                {
                    if (textSpawner != null)
                    {
                        textSpawner.ShowText(shaneLines[Random.Range(0, shaneLines.Length)]);
                    }
                    Destroy(nearestHostile);
                    nearestHostile = null;
                }
            }
            return;
        }


        if (action.actionType == ActionType.ThrowBomb)
        {
            if (nearestBombable != null && bombTimer <= 0f)
            {
                Vector3 wallDirection = (nearestBombable.transform.position - transform.position).normalized;
                Vector3 throwPosition = nearestBombable.transform.position - wallDirection * bombThrowRange;

                navMeshAgent.SetDestination(throwPosition);

                float distanceToThrow = Vector3.Distance(transform.position, throwPosition);

                if (distanceToThrow <= 1.5f && bombPrefab != null)
                {
                    GameObject bomb = Instantiate(bombPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
                    Rigidbody body = bomb.GetComponent<Rigidbody>();

                    if (body != null)
                    {
                        Vector3 throwDirection = (nearestBombable.transform.position - bomb.transform.position).normalized + Vector3.up * 0.3f;
                        body.AddForce(throwDirection * bombForce, ForceMode.Impulse);
                    }

                    BombBehaviour bombBehaviour = bomb.AddComponent<BombBehaviour>();
                    bombBehaviour.Init(nearestBombable);

                    bombTimer = bombCooldown;

                    if (textSpawner != null)
                    {
                        textSpawner.ShowText(shaneLines[Random.Range(0, shaneLines.Length)]);
                    }
                }
                return;
            }
        }
        if (action.actionType == ActionType.Idle)
        {
            if (Random.Range(0f, 1f) < 0.0005f && textSpawner != null)
            {
                textSpawner.ShowText(shaneIdleLines[Random.Range(0, shaneIdleLines.Length)]);
            }
            base.ExecuteAction(action);
        }
        else
        {
            base.ExecuteAction(action);
        }
    }
}
