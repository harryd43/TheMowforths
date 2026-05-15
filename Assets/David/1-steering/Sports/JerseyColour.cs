using UnityEngine;

[RequireComponent(typeof(Teammate))]
[ExecuteInEditMode]
public class JerseyColour : MonoBehaviour
{
    private Teammate teammate;
    private new Renderer renderer;

    void Awake()
    {
        teammate = GetComponent<Teammate>();
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (teammate.GetTeamInfo() != null)
            renderer.material = teammate.GetTeamInfo().GetMaterial();
    }
}
