using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Steering))]
public class PursueTarget : MonoBehaviour
{
    enum FacingType
    {
        None,
        Kinematic,
        Dynamic
    }

    [SerializeField] private Rigidbody target;
    [SerializeField] private FacingType facing = FacingType.Dynamic;
    
    
    private Rigidbody rb;
    private Steering steering;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        steering = GetComponent<Steering>();
    }

    void FixedUpdate()
    {
        rb.AddForce(steering.Pursuit(target), ForceMode.VelocityChange);

        if (facing == FacingType.Kinematic)
            rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity, Vector3.up));
        else if (facing == FacingType.Dynamic)
            rb.AddTorque(0, steering.LookAlong(rb.linearVelocity), 0, ForceMode.VelocityChange);
    }
}
