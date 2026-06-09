using UnityEditor;
using System.Collections.Generic;
[CustomEditor(typeof(AgentBuilder))]
public class AgentBuilderEditor : Editor
{
    private static readonly HashSet<string> hiddenInheritedFields = new HashSet<string>
    {
        "actions",                  
        "minimumActionDuration",    
        "fleeDetectionDistance",    
        "showDebugText",            
    };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (prop.propertyPath == "m_Script")
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(prop);
                }
                continue;
            }

            if (hiddenInheritedFields.Contains(prop.name)) continue;

            EditorGUILayout.PropertyField(prop, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
