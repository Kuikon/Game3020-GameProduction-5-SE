using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Automatically detects "grave tiles" on a Tilemap
/// and spawns ghosts randomly on top of them.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header(" Spawn Settings")]
    [SerializeField] GameObject[] ghostPrefabs;      // Array of ghost prefabs to spawn
    [SerializeField] GameObject spawnEffectPrefab;   // Optional spawn effect (smoke, glow, etc.)
    [SerializeField] float delayBeforeSpawn = 0.5f;  // Delay between effect and actual spawn
    [SerializeField] private int initialSpawnCount = 10;
    [SerializeField] private GhostType targetType = GhostType.Normal;
    private Coroutine spawnLoop;
    private void Start()
    {
        SpawnInitialGhosts();
    }
    public void SpawnInitialGhosts()
    {
        int count = 0;
        for (int i = 0; i < initialSpawnCount; i++)
        {
            Vector3 pos = GetRandomPosition();
            GameObject prefab = GetPrefabByType(targetType);
            if (prefab != null)
            {
                StartCoroutine(SpawnSequence(pos, prefab));
                count++;
            }
        }
        Debug.Log($"👻 Spawned {count} {targetType} ghosts at start!");
    }

    // 🔹 ゴーストタイプ指定でPrefab取得
    private GameObject GetPrefabByType(GhostType type)
    {
        foreach (var p in ghostPrefabs)
        {
            GhostBase g = p.GetComponent<GhostBase>();
            if (g != null && g.data.type == type)
                return p;
        }
        return null;
    }


    /// <summary>
    /// Handles the spawn effect → delay → ghost spawn sequence.
    /// </summary>
    private IEnumerator SpawnSequence(Vector3 pos, GameObject prefab)
    {
        // ① Spawn effect
        if (spawnEffectPrefab != null)
        {
            Vector3 effectPos = pos + new Vector3(0f, -.5f, 0f);
            GameObject effect = Instantiate(spawnEffectPrefab, effectPos, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // ② Wait before spawning
        yield return new WaitForSeconds(delayBeforeSpawn);

        // ③ Spawn the ghost
        Instantiate(prefab, pos, Quaternion.identity);
        //Debug.Log($"👻 Spawned {prefab.name} on a grave at {pos}.");
    }
    public void SpawnAtPosition(Vector3 pos)
    {
        if (ghostPrefabs.Length == 0) return;

        GameObject ghostPrefab = ghostPrefabs[Random.Range(0, ghostPrefabs.Length)];
        StartCoroutine(SpawnSequence(pos, ghostPrefab));
    }
    private Vector3 GetRandomPosition()
    {
        // 画面上のランダム位置に出す（例）
        float x = Random.Range(-5f, 5f);
        float y = Random.Range(-3f, 3f);
        return new Vector3(x, y, 0f);
    }

}
