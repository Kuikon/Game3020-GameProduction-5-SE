using UnityEngine;
using System.Collections;

public class BossSpawnerController : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private float continuousSpawnInterval = 3f;

    private Coroutine spawnLoop;
    private void Awake()
    {
        // ✅ 孵化後に自動でシーン内の EnemySpawner を探す
        if (enemySpawner == null)
        {
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
            if (enemySpawner == null)
                Debug.LogError("❌ EnemySpawner not found in scene!");
        }
    }
    public void StartSpawnLoop(Vector3 pos)
    {
        StopSpawnLoop();
        spawnLoop = StartCoroutine(SpawnLoop(pos));
    }

    public void StopSpawnLoop()
    {
        if (spawnLoop != null)
        {
            StopCoroutine(spawnLoop);
            spawnLoop = null;
        }
    }

    private IEnumerator SpawnLoop(Vector3 pos)
    {
        while (true)
        {
            Vector3 spawnPos = pos + new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 0.5f), 0);
            enemySpawner.SpawnAtPosition(spawnPos);
            yield return new WaitForSeconds(continuousSpawnInterval);
        }
    }
    public void SetEnemySpawner(EnemySpawner spawner)
    {
        enemySpawner = spawner;
    }
}
