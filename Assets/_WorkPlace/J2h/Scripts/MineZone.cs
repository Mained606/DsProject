using System.Collections.Generic;
using UnityEngine;

public class MineZone : MonoBehaviour
{
    [SerializeField] private GameObject[] rockPrefabs;
    [SerializeField] private int maxSpawnAmount = 20;
    [SerializeField] private float spawnRadius = 10f;
    private SphereCollider collider;

    private List<GameObject> rockList = new List<GameObject>();

    private void Awake()
    {
        collider = GetComponent<SphereCollider>();
        collider.radius = spawnRadius;
        RockSpawn();
    }

    private void RockSpawn()
    {
        foreach (GameObject rock in rockList)
        {
            if (rock != null)
                Destroy(rock);
        }
        rockList.Clear();

        for (int i = 0; i < maxSpawnAmount; i++)
        {
            Vector3 randomPosition = GetRandomPositionInCollider();
            randomPosition.y = GetTerrainHeightAtPosition(randomPosition);
            int index = Random.Range(0, rockPrefabs.Length);
            GameObject rock = Instantiate(rockPrefabs[index], randomPosition, Quaternion.identity, transform);
            rockList.Add(rock);
        }
    }

    private Vector3 GetRandomPositionInCollider()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;
        randomPoint.y = transform.position.y;
        return randomPoint;
    }

    private float GetTerrainHeightAtPosition(Vector3 position)
    {
        if (Terrain.activeTerrain != null)
        {
            return Terrain.activeTerrain.SampleHeight(position) + Terrain.activeTerrain.transform.position.y;
        }
        return position.y;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
