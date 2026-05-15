using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Steering))]
public class ThrustToTarget : MonoBehaviour
{
    private Rigidbody rb;
    private Steering steering;

    public Vector3 target;

    public float stopWeight;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        steering = GetComponent<Steering>();
    }

    void FixedUpdate()
    {
        float face = steering.Face(target);
        float stop = steering.CounterSteer(target);

        float maxTorque = 1;
        float torque = Mathf.Clamp(face + stop * stopWeight, -maxTorque, maxTorque);

        rb.AddTorque(new Vector3(0, torque, 0), ForceMode.VelocityChange);


        // experiment to see how adding force only in facing direction works
        // result: orbits

        // the more we are facing the target, the greater the proportion of the steering force we apply
        Vector3 force = steering.Arrive(target);
        Vector3 alignedForce = ForceInFacing(force);

        Debug.DrawRay(transform.position, alignedForce * 100, Color.yellow, 0.01f);
        rb.AddForce(alignedForce, ForceMode.VelocityChange);
    }

    private Vector3 ForceInFacing(Vector3 force)
    {
        Vector3 direction = (target - rb.position).normalized;
        // only the component of force pointing forward is thrust
        float forceInFacingDirection = Vector3.Dot(rb.transform.forward, force);
        // float amountForceIsAlignedWithTarget = Vector3.Dot(direction, force.normalized);
        Vector3 alignedForce = rb.transform.forward * forceInFacingDirection;// * amountForceIsAlignedWithTarget;
        return alignedForce;
    }
}
