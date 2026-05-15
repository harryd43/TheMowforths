using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Location))]
public class TreesFromWood : MonoBehaviour
{
    [SerializeField] private GameObject big;
    [SerializeField] private GameObject medium;
    [SerializeField] private GameObject small;

    Location location;

    void Awake()
    {
        location = GetComponent<Location>();
    }

    void Update()
    {
        if (location.wood == 0)
        {
            big.SetActive(false);
            medium.SetActive(false);
            small.SetActive(false);
        } else if (location.wood > 0 && location.wood <= 20)
        {
            big.SetActive(false);
            medium.SetActive(false);
            small.SetActive(true);
        } else if (location.wood > 20 && location.wood <= 60)
        {
            big.SetActive(false);
            medium.SetActive(true);
            small.SetActive(true);
        } else if (location.wood > 60)
        {
            big.SetActive(true);
            medium.SetActive(true);
            small.SetActive(true);
        }
    }
}
