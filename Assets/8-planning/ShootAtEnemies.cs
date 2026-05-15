using UnityEngine;

[RequireComponent(typeof(TrackEnemies))]
public class ShootAtEnemies : MonoBehaviour
{
    private GameObject bullet;
    [SerializeField] private GameObject bulletPrefab;

    TrackEnemies trackEnemies;

    void Start()
    {
        trackEnemies = GetComponent<TrackEnemies>();
    }

    public void Shoot()
    {
        Debug.Log("Bang!");
        bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = (trackEnemies.GetEnemy().transform.position - transform.position).normalized;
    }
}
