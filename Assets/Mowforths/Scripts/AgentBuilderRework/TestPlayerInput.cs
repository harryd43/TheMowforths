using UnityEngine;

public class TestPlayerInput : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private AgentController selectedAgent;
    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }
        
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit))
        {
            return;
        }
        
        AgentController clickedAgent = hit.collider.GetComponent<AgentController>();
        if (clickedAgent != null)
        {
            SelectAgent(clickedAgent);
            return;
        }
        if (selectedAgent  != null)
        {
            selectedAgent.MoveTo(hit.point);
        }
    }

    void SelectAgent(AgentController agent)
    {
        if (selectedAgent != null)
        {
            selectedAgent.Deselect();
        }
        selectedAgent = agent;
        selectedAgent.Select();
    }
}
