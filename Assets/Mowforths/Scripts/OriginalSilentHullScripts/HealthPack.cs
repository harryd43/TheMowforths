using UnityEngine;

public class HealthPack : MonoBehaviour
{

    [SerializeField] public float healAmount = 50f;

    private void OnTriggerEnter(Collider other)
    {
        MowforthHealth health = other.gameObject.GetComponent<MowforthHealth>();
        if (health != null)
        {
            health.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
