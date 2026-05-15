using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Arrive))]
public class ArriveEditor : Editor
{
    public void OnSceneGUI()
    {
        Arrive steering = (Arrive) target;
        Vector3 snap = Vector3.one * 0.5f;
        EditorGUI.BeginChangeCheck();
        Handles.color = Color.red;
        Vector3 newTargetPosition = Handles.PositionHandle(steering.GetTarget(), Quaternion.LookRotation(Vector3.forward, Vector3.up));
        if (EditorGUI.EndChangeCheck())
        {
            steering.SetTarget(new Vector3(newTargetPosition.x, steering.GetTarget().y, newTargetPosition.z));
        }
    }
}