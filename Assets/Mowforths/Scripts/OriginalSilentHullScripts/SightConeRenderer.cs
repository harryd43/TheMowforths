using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

// Renders guards sight cone so players can see it etc, shifts colour on alert level
public class SightConeRenderer : MonoBehaviour
{
    [SerializeField] private int numberOfRaysInCone = 30;
    [SerializeField] private float heightOffset = 0.5f;

    private SightCone sightCone;
    private Mesh coneMesh;
    private MeshFilter filter;
    private MeshRenderer renderer;
    private Material material;

    void Start()
    {
       sightCone = GetComponent<SightCone>();

        // Child hold cone so it doesn't mess with guard
        GameObject coneObject = new GameObject("ConeObject");
        coneObject.transform.SetParent(transform);
        coneObject.transform.localPosition = Vector3.zero;
        coneObject.transform.localRotation = Quaternion.identity;

        filter = coneObject.AddComponent<MeshFilter>();
        renderer = coneObject.AddComponent<MeshRenderer>();

        material = new Material(Shader.Find("Sprites/Default"));
        material.color = new Color(1f, 1f, 0f, 0.38f);
        renderer.material = material;

        coneMesh = new Mesh();
        filter.mesh = coneMesh;
    }

    void Update()
    {
        if (sightCone == null)
        {
            return;
        }
        Color coneColor = Color.Lerp(new Color(1f, 1f, 0f, 0.25f), new Color(1f, 0f, 0f, 0.4f), sightCone.alertLevel);
        material.color = coneColor;

        BuildConeMesh();
    }

    void BuildConeMesh()
    {
        float angle = sightCone.viewAngle;
        float distance = sightCone.viewDistance;

        Vector3[] vertices = new Vector3[numberOfRaysInCone + 2];
        int[] triangles = new int[numberOfRaysInCone * 3];

        vertices[0] = new Vector3(0, heightOffset - transform.position.y, 0);

        float startAngle = -angle * 0.5f;
        float angleStep = angle / numberOfRaysInCone;

        for (int i = 0; i <= numberOfRaysInCone; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            float radians = currentAngle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Sin(radians), 0 , Mathf.Cos(radians));

            RaycastHit hit;
            float rayDistance = distance;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.TransformDirection(direction), out hit, distance, sightCone.obstacleMask))
            {
                rayDistance = hit.distance;
            }

            vertices[i + 1] = direction * rayDistance + new Vector3(0, heightOffset - transform.position.y, 0);
        }

        for (int i = 0; i < numberOfRaysInCone; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        coneMesh.Clear();
        coneMesh.vertices = vertices;
        coneMesh.triangles = triangles;
        coneMesh.RecalculateNormals();
    }
}
