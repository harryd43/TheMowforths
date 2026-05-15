using UnityEngine;

[ExecuteInEditMode]
public class PathBetweenWaypoints : MonoBehaviour
{
    public WaypointGraph graph;
    public int first { get; private set; }
    public int last  { get; private set; }
    private Vector3 target;
    public Waypoint[] route;

    public Vector3 GetTarget() { return this.target; }

    public void SetTarget(Vector3 target)
    {
        this.target = target;
        last = graph.Localise(target);
        // graph.Pathfind(first, last, ref route, out int length);
    }

    void Update()
    {
        first = graph.Localise(transform.position);
    }

}
