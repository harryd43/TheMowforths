using UnityEngine;

public class ClickMarker : MonoBehaviour
{
    [SerializeField] private GameObject markerPrefab;

    public void ShowMarker(Vector3 position)
    {
        StartCoroutine(SpawnMarker(position));
    }
    System.Collections.IEnumerator SpawnMarker(Vector3 position)
    {
        GameObject marker = Instantiate(markerPrefab, position + Vector3.up * 0.1f, Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        Destroy(marker);
    }
}
