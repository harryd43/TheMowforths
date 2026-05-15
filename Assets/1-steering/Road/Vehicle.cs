using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Vehicle : MonoBehaviour
{
    private Rigidbody rb;
    private Driver driver;

    [SerializeField] private Vector3 target;

    [SerializeField, Min(0f)] private float maxThrust = 1.0f;
    [SerializeField, Min(0f)] private float maxBrake = 1.0f;
    [SerializeField, Min(0f)] private float maxReverse = 1.0f;
    [SerializeField, Min(0f)] private float maxTurn = 1.0f;
    [SerializeField, Min(0.01f)] private float momentOfInertia = 1.0f;
    [SerializeField, Min(0f)] private float turnAtSpeedCoefficient = 1.0f;
    [SerializeField, Min(0f)] private float sensitivity = 1.0f;

    public float thrust = 0;
    public float brakeForwards = 0;
    public float brakeReverse = 0;
    public float reverse = 0;
    public float turningForce = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        driver = GetComponent<Driver>();
    }

    void FixedUpdate()
    {
        Vector3 force = Vector3.zero;
        if (driver != null)
            force = driver.SteeringForce * sensitivity;

        float forwardComponent = Vector3.Dot(rb.transform.forward, force);
        // skidComponent is the amount we are already sliding in the desired direction
        float skidComponent = Vector3.Dot(rb.linearVelocity.normalized, force);

        thrust = 0;
        brakeForwards = 0;
        brakeReverse = 0;
        reverse = 0;
        turningForce = 0;

        if (forwardComponent > 0) // want to go forwards
        {
            if (skidComponent >= 0) // already going forwards
                thrust = Mathf.Min(forwardComponent, maxThrust * Time.fixedDeltaTime);
            else // currently reversing
                brakeReverse = Mathf.Min(forwardComponent, maxBrake * Time.fixedDeltaTime);
        }
        else if (forwardComponent < 0) // want to go backwards
        {
            if (skidComponent >= 0) // already reversing
                reverse = Mathf.Min(-forwardComponent, maxReverse * Time.fixedDeltaTime);
            else // currently going forwards
                brakeForwards = Mathf.Min(-forwardComponent, maxBrake * Time.fixedDeltaTime);
        }

        float vehicleSpeed = Vector3.Dot(rb.transform.forward, rb.linearVelocity);
        float maxTurnAtSpeed = Mathf.Min(maxTurn, Mathf.Abs(vehicleSpeed) * turnAtSpeedCoefficient);
        float maxTurnPerFrame = maxTurnAtSpeed * Time.fixedDeltaTime;
        
        turningForce = Mathf.Clamp(Vector3.Dot(rb.transform.right, force), -maxTurnPerFrame, maxTurnPerFrame);
        turningForce /= momentOfInertia;

        Debug.DrawRay(transform.position, rb.transform.forward * thrust, Color.green, 0.01f);
        Debug.DrawRay(transform.position, rb.transform.right * turningForce, Color.yellow, 0.01f);
        Debug.DrawRay(transform.position, -rb.transform.forward * brakeForwards, Color.red, 0.01f);
        Debug.DrawRay(transform.position, rb.transform.forward * brakeReverse, Color.red, 0.01f);
        Debug.DrawRay(transform.position, -rb.transform.forward * reverse, Color.magenta, 0.01f);

        if (thrust > 0)
            rb.AddForce(thrust * rb.transform.forward, ForceMode.VelocityChange);
        if (brakeForwards > 0)
            rb.AddForce(brakeForwards * -rb.transform.forward, ForceMode.VelocityChange);
        if (brakeReverse > 0)
            rb.AddForce(brakeReverse * rb.transform.forward, ForceMode.VelocityChange);
        if (reverse > 0)
            rb.AddForce(reverse * -rb.transform.forward, ForceMode.VelocityChange);
        if (turningForce != 0)
            rb.AddTorque(new Vector3(0, turningForce, 0), ForceMode.VelocityChange);
    }

}
