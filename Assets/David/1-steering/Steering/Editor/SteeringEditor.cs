using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Steering))]
public class SteeringEditor : Editor
{
    public void OnSceneGUI()
    {
        Steering steering = (Steering)target;

        float lineLength = 10f;

        Rigidbody rb = steering.GetComponent<Rigidbody>();
        Handles.color = Color.red;
        Handles.DrawDottedLine(rb.position, rb.position + rb.linearVelocity.normalized * lineLength, 1);
        Handles.DrawLine(steering.transform.position, steering.transform.position + steering.DesiredVelocity.normalized * lineLength);

        Handles.color = Color.green;
        Handles.DrawDottedLine(rb.position, rb.position + rb.transform.forward * lineLength, 1);
        float orientation = steering.DesiredOrientation;
        Vector3 going = Quaternion.AngleAxis(orientation, Vector3.up) * Vector3.forward;
        Handles.DrawLine(rb.position, rb.position + going.normalized * lineLength);
    }
}