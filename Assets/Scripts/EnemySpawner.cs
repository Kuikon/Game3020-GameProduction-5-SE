using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] ghostPrefabs;      // Different ghost prefabs
    public GameObject spawnEffectPrefab;   // Effect prefab (particles, flash, etc.)
    public Vector2 spawnAreaMin = new Vector2(-6f, -4f); // Bottom-left corner
    public Vector2 spawnAreaMax = new Vector2(6f, 4f);   // Top-right corner
    public float delayBeforeSpawn = 0.5f;  // Delay between effect and spawn
    public float autoSpawnInterval = 5f;   // Auto spawn interval in seconds

    private void Start()
    {
        // Start auto-spawning (remove this if you only want manual spawning)
        StartCoroutine(AutoSpawnRoutine());
    }

    private void Update()
    {
        // Manual spawn with Space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnWalker();
        }
    }

    /// <summary>
    /// Spawns a random ghost at a random position
    /// </summary>
    public void SpawnWalker()
    {
        // Random position in the defined area
        Vector2 pos = new Vector2(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );

        // Pick a random prefab from the list
        int index = Random.Range(0, ghostPrefabs.Length);
        GameObject prefab = ghostPrefabs[index];

        StartCoroutine(SpawnRoutine(pos, prefab));
    }

    /// <summary>
    /// Coroutine: effect Å® delay Å® spawn ghost
    /// </summary>
    private IEnumerator SpawnRoutine(Vector2 pos, GameObject prefab)
    {
        // 1. Spawn effect
        if (spawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(spawnEffectPrefab, pos, Quaternion.identity);
            Destroy(effect, 2f); // auto destroy after 2 seconds
        }

        // 2. Wait before actual spawn
        yield return new WaitForSeconds(delayBeforeSpawn);

        // 3. Spawn the ghost
        Instantiate(prefab, pos, Quaternion.identity);
        Debug.Log($"Spawned {prefab.name} at {pos}");
    }

    /// <summary>
    /// Auto spawn every few seconds
    /// </summary>
    private IEnumerator AutoSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSpawnInterval);
            SpawnWalker();
        }
    }
}
