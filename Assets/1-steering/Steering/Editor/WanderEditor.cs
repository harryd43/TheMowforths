using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Wander))]
public class WanderEditor : Editor
{
    public void OnSceneGUI()
    {
        Wander wander = (Wander)target;

        Vector3 worldSpaceCenter = wander.transform.position + wander.transform.forward * wander.GetDistance();

        Handles.color = Color.cyan;
        Handles.DrawWireDisc(worldSpaceCenter, Vector3.up, wander.GetRadius());
        Handles.DrawSolidDisc(worldSpaceCenter + wander.GetTarget(), Vector3.up, 0.1f);
    }
}