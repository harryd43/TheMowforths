using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;


// IMplements 'sight' for guards
public class SightCone : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] public float viewAngle = 90f;
    [SerializeField] public float viewDistance = 8f;
    [SerializeField] public LayerMask obstacleMask;

    [Header("Alert Settings")]
    [SerializeField] private float alertRiseRate = 1.5f;
    [SerializeField] private float alertDecayRate = 0.4f;
    [HideInInspector] public float alertLevel = 0f;
    [HideInInspector] public Vector3 lastSeenPosition;
    [HideInInspector] public bool hasSeenTarget = false;
    [HideInInspector] public GameObject detectedTarget = null;

    [Header("Danger Settings")]
    [SerializeField] private float baseConeDanger = 0.4f;
    

    public static List<SightCone> allCones = new List<SightCone>();

    void OnEnable()
    {
        allCones.Add(this);
    }
    void OnDisable()
    {
        allCones.Remove(this);
    }
    void OnDestroy()
    {
        allCones.Remove(this);
    }

    void Update()
    {
        bool canSeeTarget = false;
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject agent in agents)
        {
            if (IsPositionVisible(agent.transform.position))
            {
                canSeeTarget = true;
                lastSeenPosition = agent.transform.position;
                hasSeenTarget = true;
                detectedTarget = agent;
                break;
            }
        }
        
        if (!canSeeTarget)
        {
            detectedTarget = null;
        }
        if (canSeeTarget)
        {
            alertLevel += alertRiseRate * Time.deltaTime;
        }
        else
        {
            alertLevel -= alertDecayRate * Time.deltaTime;
        }
        alertLevel = Mathf.Clamp01(alertLevel);

        if (alertLevel <= 0f)
        {
            hasSeenTarget = false;
        }

    }

    public bool IsPositionInCone(Vector3 position)
    {
        Vector3 directionToPosition = (position - transform.position);
        directionToPosition.y = 0;

        if (directionToPosition.magnitude > viewDistance)
        {
            return false;
        }
        float angle = Vector3.Angle(transform.forward, directionToPosition.normalized);
        return angle <= viewAngle * 0.5f;

    }

    public bool IsPositionVisible(Vector3 position)
    {
        if (!IsPositionInCone(position))
        {
            return false;
        }

        Vector3 directionToTarget = (position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, position);

        return !Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToTarget, distanceToTarget, obstacleMask);
    }

    public float GetPositionDanger(Vector3 position) // Cost multiplier for Sarahs A*
    {
        if (!IsPositionInCone(position))
        {
            return 0f;
        }

        // Scale with closeness to center of cone
        Vector3 directionToPosition = (position - transform.position);
        directionToPosition.y = 0;
        float distanceScore = 1f - (directionToPosition.magnitude / viewDistance);
        float angle = Vector3.Angle(transform.forward, directionToPosition.normalized);
        float angleScore = 1f - (angle / (viewAngle * 0.5f));

        float middleDanger = distanceScore * angleScore * 5f + distanceScore * angleScore * alertLevel * 5f;

        float alwaysDanger = baseConeDanger * (1f + alertLevel);

        return Mathf.Clamp01(Mathf.Max(middleDanger, alwaysDanger));
    }
}
