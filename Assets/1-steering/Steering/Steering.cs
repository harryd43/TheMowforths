using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Steering : MonoBehaviour
{
    public Vector3 DesiredVelocity { get; private set; }
    public float DesiredOrientation { get; private set; }

    [SerializeField] private float maxSpeed = 10;
    [SerializeField] private float maxAcceleration = Mathf.Infinity;
    private float maxAccelerationPerFrame;
    [SerializeField, Min(0.02f)] private float arriveTimeToTarget = 1;
    [SerializeField] private float maxAngularVelocity = Mathf.Infinity;
    [SerializeField] private float maxAngularAcceleration = Mathf.Infinity;
    [SerializeField, Min(0.02f)] private float alignTimeToTarget = 0.1f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        maxAccelerationPerFrame = maxAcceleration / Time.fixedDeltaTime;
    }

    public enum FacingMode {
        Forward,
        None
    }

    public void ApplySteeringForce(Vector3 force, FacingMode facingMode)
    {
        rb.AddForce(force, ForceMode.VelocityChange);
        if (facingMode == FacingMode.Forward)
        {
            if (rb.linearVelocity.sqrMagnitude > 0)
                rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity, Vector3.up));
        }
    }

    /// <summary>Move toward target</summary>
    /// <param name="target">Target in world space</param>
    /// <returns>Velocity change required in meters per second per frame</returns>
    public Vector3 Seek(Vector3 target)
    {
        Vector3 toTarget = target - rb.position;
        if (toTarget.magnitude < 0.01f)
            return Vector3.zero;

        // Move towards target at max speed
        DesiredVelocity = toTarget.normalized * maxSpeed;
        // Acceleration is required change in velocity
        Vector3 acceleration = DesiredVelocity - rb.linearVelocity;
        acceleration = Vector3.ClampMagnitude(acceleration, maxAccelerationPerFrame);
        return acceleration;
    }


    /// <summary>Decelerate to a stop</summary>
    /// <returns>Velocity change required in meters per second per frame</returns>
    public Vector3 Stop()
    {
        DesiredVelocity = Vector3.zero;
        Vector3 acceleration = DesiredVelocity - rb.linearVelocity;
        acceleration = Vector3.ClampMagnitude(acceleration, maxAccelerationPerFrame);
        return acceleration;
    }

    /// <summary>Move away from target</summary>
    /// <param name="target">Target in world space</param>
    /// <returns>Velocity change required in meters per second per frame</returns>
    public Vector3 Flee(Vector3 target)
    {
        DesiredVelocity = (rb.position - target).normalized * maxSpeed;
        Vector3 acceleration =  DesiredVelocity - rb.linearVelocity;
        acceleration = Vector3.ClampMagnitude(acceleration, maxAccelerationPerFrame);
        return acceleration;
    }

    /// <summary>Approach target and slow to stop</summary>
    /// <param name="target">Target in world space</param>
    /// <returns>Velocity change required in meters per second per frame</returns>
    public Vector3 Arrive(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        float distance = toTarget.magnitude;
        if (distance < 0.01f)
            return Vector3.zero;
        
        float speed = distance / arriveTimeToTarget;
        speed = Mathf.Min(speed, maxSpeed);
        DesiredVelocity = toTarget.normalized * speed;
        Vector3 acceleration = DesiredVelocity - rb.linearVelocity;
        acceleration = Vector3.ClampMagnitude(acceleration, maxAccelerationPerFrame);
        return acceleration;
    }

    /// <summary>Seek predicted position of target</summary>
    /// <param name="target">Target Rigidbody</param>
    /// <returns>Velocity change required in meters per second per frame</returns>
    public Vector3 Pursuit(Rigidbody target)
    {
        Vector3 toTarget = target.position - rb.position;

        // Calculation of relative headings is only needed if we want to skip calculating
        // look ahead.
        // Vector3 targetHeading = target.linearVelocity.normalized;
        // Vector3 heading = rb.linearVelocity.normalized;
        // float cosRelativeHeading = Vector3.Dot(heading, targetHeading);
        // // If relative heading is less than ~18 degrees cos(18 degrees) ~= 0.95, skip look ahead
        // if (cosRelativeHeading < 0.95f)
        //     return Seek(target.position);

        // look ahead more for a longer distance to target
        // look ahead less if the agents are going faster
        float lookAheadTime = toTarget.magnitude / (maxSpeed + target.linearVelocity.magnitude);
        return Seek(target.position + target.linearVelocity * lookAheadTime);
    }

    /// <summary>Flee predicted position of target</summary>
    /// <param name="target">Target Rigidbody</param>
    /// <returns>Velocity change required in meters per second per frame</returns>
    public Vector3 Evade(Rigidbody target)
    {
        Vector3 toTarget = target.position - rb.position;

        // look ahead more for a longer distance to target
        // look ahead less if the agents are going faster
        float lookAheadTime = toTarget.magnitude / (maxSpeed + target.linearVelocity.magnitude);
        return Flee(target.position + target.linearVelocity * lookAheadTime);
    }

    /// <summary>Seek randomly moving point on a circle ahead of agent</summary>
    /// <param name="target">Point in local space</param>
    /// <param name="radius">Radius of circle to constrain target</param>
    /// <param name="distance">Distance to centre of circle constraining target</param>
    /// <param name="jitter">Amount to scale randomness</param>
    /// <returns>Velocity change required in meters per second per frame</returns>

    public Vector3 Wander(ref Vector3 target, float radius, float distance, float jitter)
    {       
        // Add random jitter to target position
        target += new Vector3(((Random.value * 2) - 1) * jitter, 0, ((Random.value * 2) - 1) * jitter);
        // Constrain new target to circle
        target = target.normalized * radius;
        // Seek position in world space
        Vector3 worldSpace = rb.position + transform.forward * distance + target;    
        return Seek(worldSpace);
    }

    /// <summary>Align to target orientation (radians)</summary>
    /// <param name="targetOrientation">The target orientation in radians</param>
    /// <returns>Velocity change required in radians per frame around the y axis</returns>
    public float Align(float targetOrientation)
    {
        targetOrientation = ClampOrientationRange(targetOrientation);
        DesiredOrientation = targetOrientation * Mathf.Rad2Deg;

        // orientationDelta is difference between current orientation and goal
        float orientationDelta = targetOrientation - rb.rotation.eulerAngles.y * Mathf.Deg2Rad;
        orientationDelta = ClampOrientationRange(orientationDelta);

        // velocity is change in radians per second
        float desiredAngularVelocity = orientationDelta / alignTimeToTarget;
        desiredAngularVelocity = Mathf.Clamp(desiredAngularVelocity, -maxAngularVelocity, maxAngularVelocity);

        // acceleration is change in velocity per second
        float angularAcceleration = desiredAngularVelocity - rb.angularVelocity.y;
        angularAcceleration = Mathf.Clamp(angularAcceleration, -maxAngularAcceleration, maxAngularAcceleration);

        // return as change in velocity per frame
        return angularAcceleration * Time.fixedDeltaTime;
    }

    /// <summary>Turn to face the given target</summary>
    /// <param name="target">Point to face world space</param>
    /// <returns>Velocity change required in radians per frame around the y axis</returns>
    public float Face(Vector3 target)
    {
        Vector3 displacement = target - rb.position;
        return LookAlong(displacement);
    }

    /// <summary>Turn to face along heading</summary>
    /// <param name="heading">Direction to look</param>
    /// <returns>Velocity change required in radians per frame around the y axis</returns>
    public float LookAlong(Vector3 heading)
    {
        heading = new Vector3(heading.x, 0, heading.z);
        if (heading.sqrMagnitude <= 0.0001f)
            return 0;
        heading = heading.normalized;
        float targetOrientation = Mathf.Atan2(heading.x, heading.z);
        return Align(targetOrientation);
    }

    /// <summary>Turn to face the direction of movement of the Rigidbody</summary>
    /// <returns>Velocity change required in radians per frame around the y axis</returns>
    public float LookWhereYoureGoing()
    {
        return LookAlong(rb.linearVelocity);
    }

    public Vector3 MoveAtAngle(float orientation)
    {
        DesiredVelocity = HeadingFor(orientation) * maxSpeed;
        Vector3 acceleration = DesiredVelocity - rb.linearVelocity;
        acceleration = Vector3.ClampMagnitude(acceleration, maxAccelerationPerFrame);
        return acceleration;
    }

    public Vector3 HeadingFor(float orientation)
    {
        return new Vector3(Mathf.Sin(orientation), 0, Mathf.Cos(orientation));
    }

    /// <summary>Turn to face opposite movement perpendicular to target. 
    /// As if a rocket needs to slow down to prevent drifting sideways</summary>
    /// <param name="target">Point in world space</param>
    /// <returns>Velocity change required in radians per frame around the y axis</returns>
    public float CounterSteer(Vector3 target)
    {
        Vector3 direction = (target - rb.position).normalized;
        Vector3 stoppingForce = Stop();

        Vector3 perpendicular = new Vector3(direction.z, 0, -direction.x);
        float stopPerpendicularToDirection = Vector3.Dot(stoppingForce, perpendicular);
        Vector3 desiredStoppingForce = perpendicular * stopPerpendicularToDirection;
        return LookAlong(desiredStoppingForce);
    }

    /// <summary>Clamp value within range (-pi, pi)</summary>
    /// <param name="orientation">Orientation in radians</param>
    private float ClampOrientationRange(float orientation)
    {
        orientation %= Mathf.PI * 2;
        if (orientation < 0)
            orientation += Mathf.PI * 2;

        if (orientation > Mathf.PI)
            orientation -= Mathf.PI * 2;
        return orientation;
    }

    public Vector3 Separation(MonoBehaviour[] targets, float maxDistance, float decayCoefficient)
    {
        Vector3 accumulate = Vector3.zero;
        for (int i = 0;i < targets.Length;i++)
        {
            Vector3 toTarget = targets[i].transform.position - rb.position;
            float sqrDistance = toTarget.sqrMagnitude;
            if (sqrDistance > 0 && sqrDistance < maxDistance * maxDistance)
            {
                float strength = Mathf.Min(decayCoefficient / sqrDistance, maxAccelerationPerFrame);
                accumulate -= toTarget.normalized * strength;
            }
        }
        accumulate = Vector3.ClampMagnitude(accumulate, maxAccelerationPerFrame);
        return accumulate * Time.fixedDeltaTime;
    }


    public Vector3 WallAvoidance(float feelerLength)
    {
        RaycastHit hit;

        if (Physics.Raycast(rb.position, rb.linearVelocity, out hit, feelerLength))
        {
            float penetrationDepth = feelerLength - hit.distance;
            return new Vector3(hit.normal.x, 0, hit.normal.z) * penetrationDepth * penetrationDepth;
        }

        // Feeler 30 degrees right of velocity direction
        if (Physics.Raycast(rb.position, Quaternion.AngleAxis(30, Vector3.up) * rb.linearVelocity, out hit, feelerLength))
        {
            float penetrationDepth = feelerLength - hit.distance;
            return new Vector3(hit.normal.x, 0, hit.normal.z) * penetrationDepth * penetrationDepth;
        }

        // Feeler 30 degrees left of velocity direction
        if (Physics.Raycast(rb.position, Quaternion.AngleAxis(-30, Vector3.up) * rb.linearVelocity, out hit, feelerLength))
        {
            float penetrationDepth = feelerLength - hit.distance;
            return new Vector3(hit.normal.x, 0, hit.normal.z) * penetrationDepth * penetrationDepth;
        }
        return Vector3.zero;
    }
}
