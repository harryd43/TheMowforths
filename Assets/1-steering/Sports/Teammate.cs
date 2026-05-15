using UnityEngine;

public enum Role {
    Goalkeeper,
    FieldPlayer
}

[ExecuteInEditMode]
public class Teammate : MonoBehaviour
{
    [SerializeField] private TeamInfo team;
    public TeamInfo GetTeamInfo() { return team; }

    [SerializeField] private Role role;
    public Role GetRole() { return role; }

    public void AssignToTeam(TeamInfo team, Role role)
    {
        this.team = team;
        this.role = role;
    }

    void Awake()
    {
        ReAddToTeam();
    }

    void OnDestroy()
    {
        RemoveFromTeam();
    }

    public void ReAddToTeam()
    {
        Team[] teams = FindObjectsByType<Team>(FindObjectsSortMode.None);
        for (int i = 0; i < teams.Length; i++)
        {
            teams[i].RemoveMember(this);
            if (teams[i].GetTeamInfo() == team)
                teams[i].AddMember(this);
        }
    }

    private void RemoveFromTeam()
    {
        Team[] teams = FindObjectsByType<Team>(FindObjectsSortMode.None);
        for (int i = 0; i < teams.Length;i++)
            if (teams[i].GetTeamInfo() == team)
                teams[i].RemoveMember(this);
    }

}
