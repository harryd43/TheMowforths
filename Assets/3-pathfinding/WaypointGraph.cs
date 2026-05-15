using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct Waypoint {
    public Vector3 position;
}

[Serializable]
public struct Edge {
    public int start;
    public int end;
}

public class WaypointGraph : MonoBehaviour
{
    public Waypoint[] waypoints;
    public Edge[] edges;

    public int Localise(Vector3 position)
    {
        int closest = -1;
        float squareDistance = Mathf.Infinity;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float dist = (waypoints[i].position - position).sqrMagnitude;
            if (dist < squareDistance)
            {
                squareDistance = dist;
                closest = i;
            }
        }
        return closest;
    }

    // public void Pathfind(int first, int last, ref Waypoint[] route, out int routeLength)
    // {
    //     List<PathfindNode> openSet = new List<PathfindNode>();
    //     openSet.Add(new PathfindNode(first, -1, 0, Heuristic(first, last)));

    //     while (openSet.Count > 0)
    //     {
    //         PathfindNode current = HighestPriority(openSet);
    //         if (current.nodeID == last)
    //             break;

    //         // test if g value or h value or something... ?

    //         float stepCost = 1;
    //         float costToReach = current.costToReach + stepCost;

    //         // update subset of nodes
    //         List<int> neighbours = Adjacent(current.nodeID);
    //         for (int i = 0; i < neighbours.Count; i++)
    //         {

    //             if (openSet[neighbours[i]].costToReach < costToReach)
    //             {
    //                 openSet[neighbours[i]].costToReach = costToReach;
    //                 openSet[neighbours[i]].parentID = current;
    //             }

    //         }
    //     }

    //     routeLength = 0;
    // }

    private PathfindNode HighestPriority(List<PathfindNode> openSet)
    {
        int bestIdx = -1;
        float best = Mathf.NegativeInfinity;
        for (int i = 0; i < openSet.Count; i++)
        {
            if (openSet[i].heuristic > best)
            {
                bestIdx = i;
                best = openSet[i].heuristic;
            }

        }
        return openSet[bestIdx];
    }

    private List<int> Adjacent(int nodeID)
    {
        List<int> neighbours = new List<int>();
        for (int i = 0; i < edges.Length; i++)
        {
            if (edges[i].start == nodeID)
                neighbours.Append(edges[i].end);
        }
        return neighbours;
    }

    private float Heuristic(int nodeID, int goalID)
    {
        return (waypoints[goalID].position - waypoints[nodeID].position).magnitude;
    }
}

struct PathfindNode {
    public int nodeID;
    public int parentID;
    public float costToReach;
    public float heuristic;

    public PathfindNode(int id, int parent, float cost, float heuristic)
    {
        this.nodeID = id;
        this.parentID = parent;
        this.costToReach = cost;
        this.heuristic = heuristic;
    }
}
