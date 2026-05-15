using UnityEngine;

public class PursuitDriver : Driver
{
    [SerializeField] private Rigidbody target;

    void FixedUpdate()
    {
        SteeringForce = steering.Pursuit(target);
    }
    
}