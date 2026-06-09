using Unity.VisualScripting;
using UnityEngine;


// Helper for agent sensisng, stops nearest tag etc being re done for each action
public class AgentHelper : MonoBehaviour
{
    // distance to tag
    public static GameObject FindNearestTagged(Vector3 origin, string tag, float maxRadius, out float distance)
    {
        distance = float.MaxValue;
        GameObject nearestObj = null;

        GameObject[] potentials = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject potential in potentials)
        {
            if (!potential.activeSelf)
            {
                continue;
            }
            float dist = Vector3.Distance(origin, potential.transform.position);
            if (dist < distance && dist <= maxRadius)
            {
                distance = dist;
                nearestObj = potential;
            }
        }
        return nearestObj;
    }

    // points on surface of collider for bomb walls etc
    public static Vector3 NearestSurfacePoint(GameObject obj, Vector3 from)
    {
        if (obj == null)
        {
            return from;
        }
        Collider collider = obj.GetComponent<Collider>();
        if (collider == null)
        {
            collider = obj.GetComponentInChildren<Collider>();
        }
        if (collider != null)
        {
            return collider.ClosestPointOnBounds(from);
        }
        return obj.transform.position;
    }

    public static float ProximityScore(float distance, float maxRadius)
    {
        if (distance > maxRadius)
        {
            return 0f;
        }
        return Mathf.Clamp01(1f - (distance / maxRadius));
    }
}
