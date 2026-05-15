using UnityEngine;

[RequireComponent(typeof(Steering))]
public class Driver : MonoBehaviour
{
    protected Steering steering;

    public Vector3 SteeringForce { get; protected set; }

    void Awake()
    {
        steering = GetComponent<Steering>();
    }
}
