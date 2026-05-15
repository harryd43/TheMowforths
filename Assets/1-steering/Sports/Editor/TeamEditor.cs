using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Team))]
public class TeamEditor : Editor
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
        if (DrawDefaultInspector())
        {
            RePopulate();
        }
        Transform transform = ((MonoBehaviour)target).transform;
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }

    public void RePopulate()
    {
        Team team = (Team)target;
        if (team.GetTeamInfo() != null)
        {
            Teammate[] teammates = FindObjectsByType<Teammate>(FindObjectsSortMode.None);
            for (int i = 0; i < teammates.Length; i++)
            {
                team.RemoveMember(teammates[i]);
                if (teammates[i].GetTeamInfo() == team.GetTeamInfo())
                    team.AddMember(teammates[i]);
            }
        }
    }
}