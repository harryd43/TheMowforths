using UnityEngine;

public class MoveBetweenNodes : MonoBehaviour
{
    [SerializeField] private Vector3[] nodes;

    public void MoveToNode(int node)
    {
        if (node < nodes.Length)
            transform.position += (nodes[node] - transform.position).normalized * Time.deltaTime;
    }

    public bool AtNode(int node)
    {
        if (node < nodes.Length)
            return (nodes[node] - transform.position).sqrMagnitude < 0.4f;
        return false;
    }

}
