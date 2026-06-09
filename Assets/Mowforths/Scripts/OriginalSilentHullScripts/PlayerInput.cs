using UnityEngine;
using UnityEngine.UIElements;

// Handles player input, such as selecting Mowforths and issuing movement commands
public class PlayerInput : MonoBehaviour
{
    private AgentController selectedAgent = null;
    [SerializeField] Camera camera;
    private Vector3 cameraTargetPosition;
    [SerializeField] float cameraMoveSpeed = 5f;
    private bool cameraMoving = false;

    [SerializeField] GameObject macca;
    [SerializeField] GameObject ashley;
    [SerializeField] GameObject shane;
    [SerializeField] GameObject sarah;
    [SerializeField] Canvas startUI;
    [SerializeField] Canvas GameUI;
    [SerializeField] Camera startCam;
    [SerializeField] Canvas finalCanvas;


    [SerializeField] private AudioClip music;

    private void Start()
    {
        GameUI.enabled = false;
        finalCanvas.enabled = false;

        AudioSource audio = gameObject.AddComponent<AudioSource>();
        audio.clip = music;
        audio.loop = true;
        audio.Play();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
        if (cameraMoving)
        {
            camera.transform.position = Vector3.MoveTowards(camera.transform.position, cameraTargetPosition, cameraMoveSpeed * Time.deltaTime);

            if (Vector3.Distance(camera.transform.position, cameraTargetPosition) < 0.2f)
            {
                cameraMoving = false;
            }
        }
    }

    void HandleClick()
    {

        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
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
                GetComponent<ClickMarker>().ShowMarker(hit.point);
            }
        }
    }

    public void SelectAgent(AgentController agent)
    {
        if (selectedAgent != null)
        {
            selectedAgent.Deselect();
        }
        selectedAgent = agent;
        selectedAgent.Select();

        cameraTargetPosition = new Vector3(selectedAgent.transform.position.x, camera.transform.position.y, (selectedAgent.transform.position.z - 16.29f));

        cameraMoving = true;
    }

    public void activateGameObjects()
    {
        macca.SetActive(true);
        shane.SetActive(true);
        ashley.SetActive(true);
        sarah.SetActive(true);
        startUI.enabled = false;
        GameUI.enabled = true;
        startCam.enabled = false;

    }

    public void finish()
    {
        GameUI.enabled = false;
        finalCanvas.enabled = true;
    }

}
