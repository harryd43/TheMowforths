using UnityEngine;

// Bomb destroys wall
public class BombBehaviour : MonoBehaviour
{
    private GameObject target;
    private float lifetime = 5f;

    public void Init(GameObject target)
    {
        this.target = target;
    }

    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bombable"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Bomb Destroyed");
    }
}
