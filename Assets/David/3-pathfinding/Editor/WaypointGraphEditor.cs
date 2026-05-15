using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointGraph))]
public class WaypointGraphEditor : Editor
{
    public void OnSceneGUI()
    {
        WaypointGraph waypointGraph = (WaypointGraph) target;

        Vector3 snap = Vector3.one * 0.5f;
        for (int i = 0; i < waypointGraph.waypoints.Length; i++)
        {
            Waypoint w = waypointGraph.waypoints[i];
            EditorGUI.BeginChangeCheck();
            Handles.color = Color.red;
            Vector3 newTargetPosition = Handles.PositionHandle(w.position, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            Handles.DrawSolidDisc(w.position, Vector3.up, 0.5f);
            Handles.Label(w.position + Vector3.up, i.ToString());
            if (EditorGUI.EndChangeCheck())
                waypointGraph.waypoints[i].position = newTargetPosition;
        }

        Handles.color = Color.red;
        for (int i = 0; i < waypointGraph.edges.Length; i++)
        {
            Edge e = waypointGraph.edges[i];
            Handles.DrawLine(waypointGraph.waypoints[e.start].position, waypointGraph.waypoints[e.end].position, 2);
            
        }

    }
}