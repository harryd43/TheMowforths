using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Steering))]
public class Crowd : MonoBehaviour
{
    [SerializeField] private float decayCoefficient = 10;
    [SerializeField] private float maxDistance = 10;

    private Crowd[] targets;
    private Rigidbody rb;
    private Steering steering;

    private Vector3 target;

    public Vector3 wander;
    public Vector3 separation;
    public Vector3 wallAvoidance;

    [SerializeField] private float radius = 1f;
    [SerializeField] private float distance = 1f;
    [SerializeField] private float jitter = 0.2f;

    [SerializeField] private float maxAcceleration = 1f;
    [SerializeField] private float wallAvoidanceDistance = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        steering = GetComponent<Steering>();   
        targets = FindObjectsByType<Crowd>(FindObjectsSortMode.None);
    }

    void FixedUpdate()
    {
        Vector3 totalForce = TotalForce();
        rb.AddForce(totalForce, ForceMode.VelocityChange);

        rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity, Vector3.up));
    }

    private Vector3 TotalForce()
    {
        Vector3 steeringForce = Vector3.zero;
        separation = steering.Separation(targets, maxDistance, decayCoefficient);
        steeringForce = AccumulateForce(steeringForce, separation, out bool forceAdded);
        wallAvoidance = steering.WallAvoidance(wallAvoidanceDistance);
        steeringForce = AccumulateForce(steeringForce, wallAvoidance, out forceAdded);
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
