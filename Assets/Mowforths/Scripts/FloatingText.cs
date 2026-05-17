using UnityEngine;


// For speech bubbles
public class FloatingText : MonoBehaviour
{
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeSpeed = 1.0f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0);

    private TextMesh textMesh;
    private float timer;
    private Color color;
    private Transform camera;

    public void Init(TextMesh tm, float duration)
    {
        textMesh = tm;
        displayDuration = duration;
        timer = duration;
        color = textMesh.color;
    }

    void Start()
    {
        camera = Camera.main.transform;
    }

    void Update()
    {
        if (textMesh == null || camera == null)
        {
            return;
        }

        transform.LookAt(transform.position + camera.forward);
        timer -= Time.deltaTime;

        if (timer <= fadeSpeed)
        {
            float alpha = Mathf.Clamp01(timer / fadeSpeed);
            textMesh.color = new Color(color.r, color.g, color.b, alpha);
        }

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void setText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }
}
