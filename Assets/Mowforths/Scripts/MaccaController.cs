using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

// Controls basic movement for macca using Unity's NavMesh pathfinding
public class MaccaController : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private float arrivalThreshold = 0.5f; // Distance at which Macca considers he has arrived at the destination
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
                       Debug.LogError("NavMeshAgent component not found on Macca!");
        }
    }

    
    void Update()
    {
      if (Input.GetMouseButton(0))
        {
            MoveToPoint();
        }  
    }

    void MoveToPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            agent.SetDestination(hit.point);
            Debug.Log("Macca moving to: " + hit.point);
        }
    }

    private void OnDrawGizmos()
    {
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(agent.destination, 0.2f);
        }
    }
}
