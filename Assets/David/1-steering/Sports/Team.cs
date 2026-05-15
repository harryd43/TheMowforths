using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    [SerializeField] private TeamInfo team;
    public TeamInfo GetTeamInfo() { return team; }

    [SerializeField] private List<Teammate> members = new List<Teammate>();

    public void AddMember(Teammate teammate)
    {
        if (!this.members.Contains(teammate))
            this.members.Add(teammate);
    }

    public void RemoveMember(Teammate teammate)
    {
        this.members.Remove(teammate);
    }
}
