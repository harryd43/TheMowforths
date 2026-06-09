using UnityEngine;
using UnityEngine.Rendering;

public class StartCamera : MonoBehaviour
{
    public bool goingLeft = false;
    [SerializeField] private float speed = 5f;
    private float leftLimit = -45f;
    private float rightLimit = 45f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float rotation = transform.eulerAngles.y;
        if (rotation > 180f)
        {
            rotation -= 360f;
        }
        if (!goingLeft)
        {
            transform.Rotate(0, speed * Time.deltaTime, 0);
            if (rotation >= rightLimit)
            {
                goingLeft = true;
            }
        }
        else
        {
            transform.Rotate(0, -speed * Time.deltaTime, 0);
            if (rotation <= leftLimit || rotation > 180f)
            {
                goingLeft = false;
            }
        }

    }
}
