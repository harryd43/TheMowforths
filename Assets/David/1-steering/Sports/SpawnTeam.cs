using System.Collections.Generic;
using UnityEngine;

public class SpawnTeam : MonoBehaviour
{
    [SerializeField] private TeamInfo team;
    [SerializeField] private Vector3[] spawnPoints;
    [SerializeField] private Teammate prefab;

    public Vector3[] GetSpawnPoints()
    {
        return spawnPoints;
    }

    public void SetSpawnPoint(int index, Vector3 position)
    {
        spawnPoints[index] = position;
    }

    void Awake() {
        SpawnAll();
    }

    [ContextMenu("SpawnAll")]
    public void SpawnAll()
    {
        if (prefab == null)
            Debug.Log("Nothing to spawn, prefab is null");
        else
            for (int i = 0; i < spawnPoints.Length; i++)
                SpawnSingle(i);
    }

    public void SpawnSingle(int index)
    {
        Teammate player = Instantiate(prefab, spawnPoints[index] + Vector3.up * prefab.transform.localScale.y, Quaternion.identity);
        player.AssignToTeam(team, Role.FieldPlayer);
    }
}
