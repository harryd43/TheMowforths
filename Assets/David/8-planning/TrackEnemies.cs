using UnityEngine;

public class TrackEnemies : MonoBehaviour
{
    [SerializeField] private GameObject enemy;

    public GameObject GetEnemy()
    {
        return enemy;
    }

    public bool HasEnemy()
    {
        return true;
    }
}
