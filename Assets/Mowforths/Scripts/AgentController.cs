using UnityEngine;
using UnityEngine.AI;

// Controls all Mowforths, handling the movement with NavMesh
// Movement will be extended later for each Mowforths personality
public class AgentController : MonoBehaviour
{
    public string agentName = "Agent";

    [HideInInspector]
    public bool isSelected = false;

    private NavMeshAgent navAgent;
    private Renderer agentRenderer;

    private Color originalColor;
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        agentRenderer = GetComponent<Renderer>();

        if (agentRenderer != null)
        {
            agentRenderer.material = new Material(agentRenderer.material);
            originalColor = agentRenderer.material.color;
        }
            
    }

    public void Select()
    {
        isSelected = true;

        if (agentRenderer != null)
        {
            agentRenderer.material.color = Color.yellow;
        }
        Debug.Log(agentName + " selected");
    }

    public void Deselect()
    {
        isSelected = false;

        if (agentRenderer != null) 
        { 
            agentRenderer.material.color = originalColor;
        }
    }
    
    public void MoveTo(Vector3 destination)
    {
        if (navAgent != null)
        {
            navAgent.SetDestination(destination);

            UtilityAgent utilityAgent = GetComponent<UtilityAgent>();
            if (utilityAgent != null)
            {
                utilityAgent.SetGoal(destination);
            }
            Debug.Log(agentName + "moveing to " + destination);
        }
    }
}
