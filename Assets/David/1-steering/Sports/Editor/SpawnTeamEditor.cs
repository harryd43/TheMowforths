using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpawnTeam))]
public class SpawnTeamEditor : Editor
{

private Tool LastTool = Tool.None;
    void Awake()
    {
        ((MonoBehaviour)target).transform.hideFlags = HideFlags.HideInInspector;
        ((MonoBehaviour)target).gameObject.isStatic = true;
    }
	void OnEnable()
	{
		LastTool = Tools.current;
		Tools.current = Tool.None;
	}

	void OnDisable()
	{
		Tools.current = LastTool;
	}

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Transform transform = ((MonoBehaviour)target).transform;
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }

    public void OnSceneGUI()
    {
        SpawnTeam spawner = (SpawnTeam)target;

        Vector3[] spawnPoints = spawner.GetSpawnPoints();
        for (int i = 0; i < spawnPoints.Length;i++)
        {
            Vector3 snap = Vector3.one * 0.5f;
            EditorGUI.BeginChangeCheck();
            Handles.color = Color.red;
            Vector3 newTargetPosition = Handles.PositionHandle(spawnPoints[i], Quaternion.LookRotation(Vector3.forward, Vector3.up));
            if (EditorGUI.EndChangeCheck())
            {
                spawner.SetSpawnPoint(i, new Vector3(newTargetPosition.x, spawnPoints[i].y, newTargetPosition.z));
            }
        }
    }
}