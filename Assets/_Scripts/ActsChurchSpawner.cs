using UnityEngine;

public class ActsChurchSpawner : MonoBehaviour
{
    public static ActsChurchSpawner Instance { get; private set; }
    public GameObject SatelliteChurchPrefab;
    public GameObject BaseChurchPrefab;
    public float SpawnRadius = 6f;
    public ActsChurchGridManager GridManager;

    public float timeSinceLastSpawn = 0f;
    public float currentTime = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnChurch(BaseChurchPrefab);
        SpawnChurch(BaseChurchPrefab);
        SpawnChurch(SatelliteChurchPrefab);
        SpawnChurch(SatelliteChurchPrefab);
        SpawnChurch(SatelliteChurchPrefab);
    }

    void Update()
    {
        currentTime += ActsTimeKeeper.Instance.DeltaTime;
        timeSinceLastSpawn += ActsTimeKeeper.Instance.DeltaTime;

        float interval = GetRandomizedSpawnInterval(currentTime);
        if (timeSinceLastSpawn >= interval)
        {
            timeSinceLastSpawn = 0f;
            SpawnChurch(SatelliteChurchPrefab);
        }
    }
    void SpawnChurch(GameObject church)
    {
        Vector3? spawnPos = GridManager.GetValidSpawnPosition();
        if (spawnPos.HasValue)
        {
            var go = Instantiate(church, spawnPos.Value, Quaternion.identity, this.transform);
            go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, 0);
        }
    }
    float GetRandomizedSpawnInterval(float t)
    {
        float baseInterval = GetStationSpawnInterval(t);
        float noise = UnityEngine.Random.Range(-0.2f, 0.2f); // ±20% variability
        return baseInterval * (1f + noise);
    }

    //float GetStationSpawnInterval(float t)
    //{
    //    // Wait before starting spawn logic
    //    float startDelay = 30f;
    //    if (t < startDelay)
    //        return Mathf.Infinity; // Effectively disables spawning during first 30s

    //    // Configurable burst/lull cycle
    //    float cycleLength = 90f;     // One full cycle = 30s burst + 60s lull
    //    float burstDuration = 30f;   // Burst phase duration
    //    float burstInterval = 5f;    // Fast spawning during burst
    //    float lullInterval = 10f;    // Rare spawns during lull

    //    float adjustedTime = t - startDelay;
    //    float cycleTime = adjustedTime % cycleLength;

    //    if (cycleTime < burstDuration)
    //        return burstInterval;   // Burst phase
    //    else
    //        return lullInterval;    // Lull phase
    //}

    float GetStationSpawnInterval(float t)
    {
        float startDelay = 30f;
        if (t < startDelay)
            return Mathf.Infinity;

        float adjustedTime = t - startDelay;

        // Parameters you can tweak
        float maxInterval = 10f;   // Slowest: one spawn every 15s
        float minInterval = 4f;    // Fastest: one spawn every 1s
        float rampDuration = 300f; // Time (in seconds) it takes to reach max speed

        // Calculate how far along the ramp we are (0 to 1)
        float rampProgress = Mathf.Clamp01(adjustedTime / rampDuration);

        // Linearly interpolate from maxInterval to minInterval
        float currentInterval = Mathf.Lerp(maxInterval, minInterval, rampProgress);

        return currentInterval;
    }
}