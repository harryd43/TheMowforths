using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Steering))]
public class Wander : MonoBehaviour
{
    enum FacingType
    {
        None,
        Kinematic,
        Dynamic
    }

    [SerializeField] private FacingType facing = FacingType.Dynamic;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float distance = 1f;
    [SerializeField] private float jitter = 0.2f;


    private Vector3 target;
    private Rigidbody rb;
    private Steering steering;

    public Vector3 GetTarget() { return this.target;  }
    public float GetRadius() { return this.radius;  }
    public float GetDistance() { return this.distance; }
    public float GetJitter() { return this.jitter; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        steering = GetComponent<Steering>();
    }

    void Start()
    {
        rb.MoveRotation(Quaternion.AngleAxis(Random.value * 360, Vector3.up));
    }

    void FixedUpdate()
    {
        Vector3 steeringForce = steering.Wander(ref target, radius, distance, jitter);
        rb.AddForce(steeringForce, ForceMode.VelocityChange);

        // For a kinematic behaviour (facing is determined by velocity)
        if (facing == FacingType.Kinematic)
            rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity, Vector3.up));

        // For a steering behaviour (facing is independent of velocity)
        else if (facing == FacingType.Dynamic)
        {
            float changeInYRotation = steering.LookAlong(rb.linearVelocity);
            rb.AddTorque(new Vector3(0, changeInYRotation, 0), ForceMode.VelocityChange);
        }
    }
}
