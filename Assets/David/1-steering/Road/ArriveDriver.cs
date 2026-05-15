using UnityEngine;

public class ArriveDriver : Driver
{
    [SerializeField] private Vector3 target;

    void FixedUpdate()
    {
        SteeringForce = steering.Arrive(target);
    }
    
}