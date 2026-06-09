using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Unity.VisualScripting;

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
    [SerializeField] private float fleeSpeed = 8f;
    [SerializeField] private float attackCooldown = 1f;
    private float attackTimer = 0f;
    private float bombCount = 0;
    [SerializeField] private GameObject bombPrefab;

    private float bombTimer = 0f;

    private GameObject nearestHostile;
    private float distanceToHostile = float.MaxValue;

    private GameObject nearestBombable;
    private float distanceToBombable = float.MaxValue;

    private GameObject nearestBombPickup;
    private float distanceToBomb = float.MaxValue;

    private GameObject nearestHealthPack;
    private float distanceToHealthPack = float.MaxValue;
    private UtilityAction collectHealthPackAction;
    [SerializeField] private float healthPackDetectionRadius;


    private UtilityAction collectBombAction;
    [SerializeField] private float bombPickupDetectionRadius = 11f;

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
        "CITIZENS OF EARTH! I AM SHANE",
        "YOU WANT SOME?"
    };

    private string[] shaneIdleLines = new string[]
    {
        "COME ON THEN",
        "WHO WANTS SOME!",
        "MMM I WANT TRIFLE",
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

        // flee
        fleeAction = new UtilityAction();
        fleeAction.name = "Flee";
        fleeAction.actionType = ActionType.Flee;
        fleeAction.weight = 9f;

        Consideration shaneFlee = new Consideration();
        shaneFlee.name = "Flee Shane";
        shaneFlee.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Exponential, 3f, 2f, -1f, 0f)
            }
        };
        fleeAction.considerations.Add(shaneFlee);

        Consideration shaneHealth = new Consideration();
        shaneHealth.name = "Health Consideration";
        shaneHealth.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        fleeAction.considerations.Add(shaneHealth);
        actions.Add(fleeAction);

        // Pickup bomb
        collectBombAction = new UtilityAction();
        collectBombAction.name = "Collect bomb";
        collectBombAction.actionType = ActionType.CollectBomb;
        collectBombAction.weight = 7f;

        Consideration bombProximity = new Consideration();
        bombProximity.name = "Bomb close";
        bombProximity.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        collectBombAction.considerations.Add(bombProximity);
        actions.Add (collectBombAction);

        // pickup health
        collectHealthPackAction = new UtilityAction();
        collectHealthPackAction.name = "Collect Health";
        collectHealthPackAction.actionType = ActionType.CollectHealthPack;
        collectHealthPackAction.weight = 8f;

        Consideration healthPackProximity = new Consideration();
        healthPackProximity.name = "Health pack near";
        healthPackProximity.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        collectHealthPackAction.considerations.Add(healthPackProximity);

        Consideration needsHealing = new Consideration();
        needsHealing.name = "Low health";
        needsHealing.inputs = new ConsiderationInput[]
        {
            new ConsiderationInput
            {
                inputValue = 0f,
                responseCurve = new ResponseCurve(ResponseCurveType.Linear, 1f, 1f, 0f, 0f)
            }
        };
        collectHealthPackAction.considerations.Add(needsHealing);
        actions.Add(collectHealthPackAction);
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
            float distance = DistanceToSurface(bombable);
            if (distance < distanceToBombable)
            {
                distanceToBombable = distance;
                nearestBombable = bombable;
            }
        }

        // Bomb pickup
        GameObject[] bombPickups = GameObject.FindGameObjectsWithTag("Bomb");
        nearestBombPickup = null;
        distanceToBomb = float.MaxValue;

        foreach (GameObject bomb in bombPickups)
        {
            float distance = Vector3.Distance(transform.position, bomb.transform.position);
            if (distance < distanceToBomb)
            {
                distanceToBomb= distance;
                nearestBombPickup = bomb;
            }
        }
        float bombPickupScore = 0f;
        if (nearestBombPickup != null && distanceToBomb <= bombPickupDetectionRadius)
        {
            bombPickupScore = 1f - (distanceToBomb / bombPickupDetectionRadius);
        }

        // Health pack
        GameObject[] healthPacks = GameObject.FindGameObjectsWithTag("Health");
        nearestHealthPack = null;
        distanceToHealthPack = float.MaxValue;

        foreach (GameObject healthPack in healthPacks)
        {
            float distance = Vector3.Distance(transform.position, healthPack.transform.position);
            if (distance < distanceToHealthPack)
            {
                distanceToHealthPack = distance;
                nearestHealthPack = healthPack;
            }
        }

        float healthPackScore = 0f;
        if (nearestHealthPack != null && distanceToHealthPack <= healthPackDetectionRadius)
        {
            healthPackScore = 1f - (distanceToHealthPack / healthPackDetectionRadius);
        }
        MowforthHealth health = GetComponent<MowforthHealth>();
        float lowHealthScore = health != null ? 1f - (health.currentHealth / health.maxHealth) : 0f;
     
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

        float healthScore = health != null ? 1f - (health.currentHealth / health.maxHealth) : 0f;

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
        if (fleeAction.considerations.Count > 1)
        {
            fleeAction.considerations[1].inputs[0].inputValue = healthScore;
        }
        if (collectBombAction.considerations.Count > 0)
        {
            collectBombAction.considerations[0].inputs[0].inputValue = bombPickupScore;
        }
        if (collectHealthPackAction.considerations.Count > 1)
        {
            collectHealthPackAction.considerations[0].inputs[0].inputValue = healthPackScore;
            collectHealthPackAction.considerations[1].inputs[0].inputValue = lowHealthScore;
        }
    }

    Vector3 NearestSurfacePoint(GameObject wall)
    {
        Collider collider = wall.GetComponent<Collider>();
        if (collider == null)
        {
            collider = wall.GetComponentInChildren<Collider>();
        }
        if (collider != null)
        {
            return collider.ClosestPointOnBounds(transform.position);
        }
        return wall.transform.position;
    }

    float DistanceToSurface(GameObject wall)
    {
        return Vector3.Distance(transform.position, NearestSurfacePoint(wall));
    }

    protected override void ExecuteAction(UtilityAction action)
    {
        if (action.actionType == ActionType.PickFight)
        {
            if (nearestHostile != null)
            {
                navMeshAgent.SetDestination(nearestHostile.transform.position);

                if (distanceToHostile <= fightRange && attackTimer <= 0f)
                {
                    attackTimer = attackCooldown;
                    if (textSpawner != null)
                    {
                        textSpawner.ShowText(shaneLines[Random.Range(0, shaneLines.Length)]);
                    }
                    MowforthHealth hostileHealth = nearestHostile.GetComponent<MowforthHealth>();
                    if (hostileHealth!=null)
                    {
                        hostileHealth.TakeDamage(51f);
                    }
                    nearestHostile = null;
                }
            }
            return;
        }


        if (action.actionType == ActionType.ThrowBomb)
        {
            if (nearestBombable != null && bombCount >= 1)
            {
                Vector3 surfacePoint = NearestSurfacePoint(nearestBombable);

                navMeshAgent.SetDestination(surfacePoint);

                if (distanceToBombable <= bombThrowRange && bombPrefab != null)
                {
                    GameObject bomb = Instantiate(bombPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
                    Rigidbody body = bomb.GetComponent<Rigidbody>();

                    if (body != null)
                    {
                        Vector3 throwDirection = (surfacePoint - bomb.transform.position).normalized;
                        body.AddForce(throwDirection * bombForce, ForceMode.Impulse);
                    }

                    BombBehaviour bombBehaviour = bomb.AddComponent<BombBehaviour>();
                    bombBehaviour.Init(nearestBombable);

                    bombCount -= 1;

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
        if (action.actionType == ActionType.CollectBomb)
        {
            if (nearestBombPickup != null)
            {
                navMeshAgent.SetDestination(nearestBombPickup.transform.position);
            }
            return;
        }
        if (action.actionType == ActionType.CollectHealthPack)
        {
            if (nearestHealthPack != null)
            {
                navMeshAgent.SetDestination(nearestHealthPack.transform.position);
            }
            return;
        }
        else
        {
            base.ExecuteAction(action);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Shane collided with: " + collision.gameObject.name + " tagged: " + collision.gameObject.tag); 
        if (collision.gameObject.CompareTag("Bomb"))
        {
            bombCount += 1;
            Destroy(collision.gameObject);
            Debug.Log("Shane bombCount now: " + bombCount);
        }
    }

    protected override void Update()
    {
        attackTimer -= Time.deltaTime;
        base.Update();
    }
}
