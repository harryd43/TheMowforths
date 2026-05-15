using System.Collections.Generic;
using System.Text;
using Goap;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GoapPlanner))]
public class GoapPlannerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GoapPlanner planner = (GoapPlanner)target;

        EditorGUILayout.TextArea(DictToString(planner.getGoalStates()));
    }

    string DictToString<K, V>(Dictionary<K, V> dict)
    {
        StringBuilder sb = new StringBuilder();
        foreach(KeyValuePair<K, V> entry in dict)
        {
            sb.Append(entry.Key);
            sb.Append(", ");
            sb.Append(entry.Value);
            sb.Append("\n");
        }
        if (sb.Length >= 1)
            sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }

}
