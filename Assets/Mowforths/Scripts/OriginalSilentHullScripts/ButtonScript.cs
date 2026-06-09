using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{

    [SerializeField] private Material greenMaterial;
    [SerializeField] GameObject wall;
    private Renderer objectRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (objectRenderer.material != greenMaterial)
            {
                
                objectRenderer.material = greenMaterial;
            }

            if (wall != null)
            {
                Destroy(wall.gameObject);
            }
        }
    }
}
