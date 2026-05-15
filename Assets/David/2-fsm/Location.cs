using System.Collections;
using UnityEngine;

public enum Resource {
    Water,
    Wood
}

public enum LocationType {
    House,
    Warehouse,
    Well,
    Forest
}

public class Location : MonoBehaviour
{
    public int water;
    public int wood;
    public LocationType type;

    void Start()
    {
        StartCoroutine(ResourceRegeneration());
    }

    private IEnumerator ResourceRegeneration()
    {
        while (true)
        {
            yield return new WaitForSeconds(10.0f);
            if (type == LocationType.Forest)
                wood += 2;
            if (type == LocationType.Well)
                water = 10;
        }
    }

    public int ReceiveResource(Resource resource, int offered) {
        int accepted = offered;
        if (resource == Resource.Water)
            water += accepted;
        if (resource == Resource.Wood)
            wood += accepted;
        return offered - accepted;
    }

    public int LoseResource(Resource resource, int requested) {
        int provided = Mathf.Min(requested, Available(resource));
        if (resource == Resource.Water)
            water -= provided;
        if (resource == Resource.Wood)
            wood -= provided;
        return provided;
    }

    public int Available(Resource resource)
    {
        if (resource == Resource.Water)
            return water;
        if (resource == Resource.Wood)
            return wood;
        return 0;
    }
}
