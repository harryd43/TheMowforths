using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Steering))]
public class Arrive : MonoBehaviour
{
    enum FacingType
    {
        None,
        Kinematic,
        Dynamic,
    }

    [SerializeField] private Vector3 target;
    [SerializeField] private FacingType facing = FacingType.Dynamic;
    
    private Rigidbody rb;
    private Steering steering;

    public void SetTarget(Vector3 target)
    {
        this.target = target;
    }
    
    public Vector3 GetTarget()
    {
        return target;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        steering = GetComponent<Steering>();
    }

    void FixedUpdate()
    {
        rb.AddForce(steering.Arrive(target), ForceMode.VelocityChange);

        if (facing == FacingType.Kinematic)
            rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity, Vector3.up));
        else if (facing == FacingType.Dynamic)
            rb.AddTorque(new Vector3(0, steering.LookAlong(rb.linearVelocity), 0), ForceMode.VelocityChange);
    }
}
