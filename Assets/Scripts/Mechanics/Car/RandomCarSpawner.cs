using UnityEngine;

public class RandomCarSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject carPrefab;

    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnTime = 1f;
    [SerializeField] private float maxSpawnTime = 2.5f;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private float timer;
    private float nextSpawnTime;

    private void Start()
    {
        SetNextSpawnTime();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            SpawnCar();
            timer = 0f;
            SetNextSpawnTime();
        }
    }

    private void SpawnCar()
    {
        if (carPrefab == null || spawnPoints.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedSpawnPoint = spawnPoints[randomIndex];

        Instantiate(carPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
    }

    private void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }
}