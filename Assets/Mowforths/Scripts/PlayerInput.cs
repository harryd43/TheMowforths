using UnityEngine;

// Handles player input, such as selecting Mowforths and issuing movement commands
public class PlayerInput : MonoBehaviour
{
    private AgentController selectedAgent = null;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }        
    }

    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            AgentController clickedAgent = hit.collider.GetComponent<AgentController>();
            if (clickedAgent != null)
            {
                SelectAgent(clickedAgent);
            }
            else if (selectedAgent != null)
            {
                selectedAgent.MoveTo(hit.point);
            }
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
