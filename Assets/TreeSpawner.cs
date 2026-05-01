using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public GameObject redBullPrefab;
    public float startSpawnRate = 0.3f;
    public float minSpawnRate = 0.1f;
    public float spawnAcceleration = 0.005f;
    [HideInInspector] public float spawnWidth;
    public float spawnDistanceY = 15f;
    public float minSize = 0.8f;
    public float maxSize = 2.0f;
    public float redBullChance = 0.05f;

    private float currentSpawnRate;
    private float timer = 0f;
    private float lastSpawnX = 100f;

    void Start()
    {
        float screenEdge = Camera.main.orthographicSize * Camera.main.aspect;
        spawnWidth = screenEdge - 1f;

        currentSpawnRate = startSpawnRate;
    }

    void Update()
    {
        if (currentSpawnRate > minSpawnRate)
        {
            currentSpawnRate -= spawnAcceleration * Time.deltaTime;
        }

        timer += Time.deltaTime;

        if (timer >= currentSpawnRate)
        {
            SpawnObject();
            timer = 0f;
        }
    }

    void SpawnObject()
    {
        if (obstaclePrefabs.Length == 0) return;

        float randomX;
        int attempts = 0;

        do
        {
            randomX = Random.Range(-spawnWidth, spawnWidth);
            attempts++;
        } while (Mathf.Abs(randomX - lastSpawnX) < 1.5f && attempts < 10);

        lastSpawnX = randomX;

        float spawnY = transform.position.y - spawnDistanceY;
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        if (redBullPrefab != null && Random.value < redBullChance)
        {
            GameObject newRedBull = Instantiate(redBullPrefab, spawnPosition, Quaternion.identity);
            Destroy(newRedBull, 8f);
            return;
        }

        int randomIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject selectedObstacle = obstaclePrefabs[randomIndex];

        GameObject newObstacle = Instantiate(selectedObstacle, spawnPosition, Quaternion.identity);

        float randomScale = Random.Range(minSize, maxSize);
        newObstacle.transform.localScale = new Vector3(randomScale, randomScale, 1f);

        Destroy(newObstacle, 8f);
    }
}