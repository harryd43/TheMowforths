using UnityEditor;

[CustomEditor(typeof(Teammate))]
public class TeammateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (DrawDefaultInspector())
        {
            Teammate teammate = (Teammate)target;
            teammate.ReAddToTeam();
        }
    }
}