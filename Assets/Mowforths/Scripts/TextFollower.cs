using UnityEngine;

// Keeps flaoting text above targets head, avoiding parenting due to rotation 
public class TextFollower : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;

    public void Init(Transform target, Vector3 offset)
    {
        this.target = target;
        this.offset = offset;
    }

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
