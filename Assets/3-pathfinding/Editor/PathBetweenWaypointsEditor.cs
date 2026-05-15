using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathBetweenWaypoints))]
public class PathBetweenWaypointsEditor : Editor
{
    public void OnSceneGUI()
    {
        PathBetweenWaypoints agent = (PathBetweenWaypoints) target;

        Vector3 agentTarget = agent.GetTarget();

        if (agent.graph != null)
        {
            Handles.color = Color.blue;
            Handles.DrawSolidDisc(agent.graph.waypoints[agent.first].position, Vector3.up, 0.5f);
            Handles.DrawSolidDisc(agent.graph.waypoints[agent.last].position, Vector3.up, 0.5f);

            if (agent.route != null)
            {
                for (int i = 1; i < agent.route.Length; i++)
                {
                    Handles.DrawLine(agent.route[i - 1].position, agent.route[i].position, 4);
                }
            }
            Handles.DrawDottedLine(agent.transform.position, agent.graph.waypoints[agent.first].position, 4);
            Handles.DrawDottedLine(agent.graph.waypoints[agent.last].position, agentTarget, 4);
        }

        EditorGUI.BeginChangeCheck();
        Handles.color = Color.green;
        Handles.DrawSolidDisc(agentTarget, Vector3.up, 0.5f);
        Vector3 newTargetPosition = Handles.PositionHandle(agentTarget, Quaternion.LookRotation(Vector3.forward, Vector3.up));
        Handles.Label(agentTarget + Vector3.up, "Target");
        if (EditorGUI.EndChangeCheck())
            agent.SetTarget(newTargetPosition);

    }
}