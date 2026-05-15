using UnityEngine;

[RequireComponent(typeof(Steering))]
public class CrowdDriver : Driver
{
    [SerializeField] private float decayCoefficient = 10;
    [SerializeField] private float maxDistance = 10;

    private CrowdDriver[] targets;

    private Vector3 target;

    public Vector3 wander;
    public Vector3 separation;
    public Vector3 wallAvoidance;



    [SerializeField] private float radius = 1f;
    [SerializeField] private float distance = 1f;
    [SerializeField] private float jitter = 0.2f;

    [SerializeField] private float maxAcceleration = 1f;
    [SerializeField] private float wallAvoidanceDistance = 1f;


    [SerializeField] private float wallAvoidWeight = 1f;
    [SerializeField] private float separationWeight = 1f;
    [SerializeField] private float wanderWeight = 1f;
    

    void Start()
    {
        targets = FindObjectsByType<CrowdDriver>(FindObjectsSortMode.None);
    }

    void FixedUpdate()
    {
        SteeringForce = TotalForce();
    }

    private Vector3 TotalForce()
    {
        Vector3 steeringForce = Vector3.zero;
        wallAvoidance = steering.WallAvoidance(wallAvoidanceDistance) * wallAvoidWeight;
        steeringForce = AccumulateForce(steeringForce, wallAvoidance, out bool forceAdded);
        separation = steering.Separation(targets, maxDistance, decayCoefficient) * separationWeight;
        steeringForce = AccumulateForce(steeringForce, separation, out forceAdded);
        wander = steering.Wander(ref target, radius, distance, jitter);
        steeringForce = AccumulateForce(steeringForce, wander, out forceAdded);

        return steeringForce;
    }

    private Vector3 AccumulateForce(Vector3 steeringForce, Vector3 forceToAdd, out bool forceWasAdded)
    {
        float magnitudeRemaining = maxAcceleration * Time.fixedDeltaTime - steeringForce.magnitude;

        if (magnitudeRemaining < 0f)
        {
            forceWasAdded = false;
            return steeringForce;
        }

        float magnitudeToAdd = forceToAdd.magnitude;
        if(magnitudeToAdd < magnitudeRemaining)
            steeringForce += forceToAdd;
        else
            steeringForce += forceToAdd.normalized * magnitudeRemaining;
        forceWasAdded = true;
        return steeringForce;
    }
}
