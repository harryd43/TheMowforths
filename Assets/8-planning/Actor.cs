using UnityEngine;

public class Actor : MonoBehaviour
{
    public Actor enemy;

    public bool IsBehindWall()
    {
        Vector3 toEnemy = enemy.transform.position - transform.position;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, toEnemy, out hit))
        {
            return hit.transform.tag == "Wall";
        }
        return false;
    }

    public bool IsUnderFire()
    {
        return false;
    }
}
