using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    [SerializeField] private Vector3 startPosition;
    private new Rigidbody rigidbody;
    [SerializeField] private float kickVelocity = 5;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 difference = collision.gameObject.transform.position - transform.position;
            Vector3 direction = -new Vector3(difference.x, 0, difference.z).normalized;
            rigidbody.AddForce(direction * kickVelocity, ForceMode.VelocityChange);
        }
    }

    void Update()
    {
        if (transform.position.y < 0)
            ResetPosition();
    }

    [ContextMenu("ResetPosition")]
    void ResetPosition()
    {
        transform.position = startPosition;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }
}
