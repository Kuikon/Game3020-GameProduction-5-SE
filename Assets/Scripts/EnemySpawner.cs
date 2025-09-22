using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject walkerPrefab;       // RandomWalker prefab
    public GameObject spawnEffectPrefab;  // Effect prefab (e.g. particle system)
    public Vector2 spawnAreaMin = new Vector2(-6f, -4f); // spawn area min
    public Vector2 spawnAreaMax = new Vector2(6f, 4f);   // spawn area max
    public float delayBeforeSpawn = 0.5f; // time between effect and walker

    public void SpawnWalker()
    {
        // Pick a random position
        Vector2 pos = new Vector2(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );

        StartCoroutine(SpawnRoutine(pos));
    }

    private IEnumerator SpawnRoutine(Vector2 pos)
    {
        // 1. Spawn effect first
        if (spawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(spawnEffectPrefab, pos, Quaternion.identity);
            Destroy(effect, 2f); // auto-destroy effect after 2s
        }

        // 2. Wait before spawning the walker
        yield return new WaitForSeconds(delayBeforeSpawn);

        // 3. Spawn the RandomWalker
        Instantiate(walkerPrefab, pos, Quaternion.identity);
    }
}
