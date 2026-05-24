using UnityEngine;

public class RandomCarSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject carPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Timing")]
    [SerializeField] private float minSpawnTime = 0.9f;
    [SerializeField] private float maxSpawnTime = 1.8f;

    [Header("Safety")]
    [SerializeField] private float chanceToSwitchLane = 0.75f;
    [SerializeField] private float chanceToSkipSpawn = 0.15f;

    private float timer;
    private float nextSpawnTime;
    private int lastLane = -1;

    private void Start()
    {
        SetNextSpawnTime();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            timer = 0f;

            TrySpawnCar();
            SetNextSpawnTime();
        }
    }

    private void TrySpawnCar()
    {
        if (carPrefab == null || spawnPoints.Length == 0)
        {
            return;
        }

        if (Random.value < chanceToSkipSpawn)
        {
            return;
        }

        int lane = ChooseLane();

        Instantiate(
            carPrefab,
            spawnPoints[lane].position,
            spawnPoints[lane].rotation
        );

        lastLane = lane;
    }

    private int ChooseLane()
    {
        if (spawnPoints.Length == 1)
        {
            return 0;
        }

        if (lastLane == -1)
        {
            return Random.Range(0, spawnPoints.Length);
        }

        bool shouldSwitchLane = Random.value < chanceToSwitchLane;

        if (shouldSwitchLane)
        {
            return lastLane == 0 ? 1 : 0;
        }

        return lastLane;
    }

    private void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }
}