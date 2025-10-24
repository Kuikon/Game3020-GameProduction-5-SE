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

    private void Start()
    {
       
    }
    /// <summary>
    /// Handles the spawn effect → delay → ghost spawn sequence.
    /// </summary>
    private IEnumerator SpawnSequence(Vector3 pos, GameObject prefab)
    {
        // ① Spawn effect
        if (spawnEffectPrefab != null)
        {
            Vector3 effectPos = pos + new Vector3(0f, -0.5f, 0f);
            GameObject effect = Instantiate(spawnEffectPrefab, effectPos, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // ② Wait before spawning
        yield return new WaitForSeconds(delayBeforeSpawn);

        // ③ Spawn the ghost
        Instantiate(prefab, pos, Quaternion.identity);
        Debug.Log($"👻 Spawned {prefab.name} on a grave at {pos}.");
    }
    public void SpawnAtPosition(Vector3 pos)
    {
        if (ghostPrefabs.Length == 0) return;

        GameObject ghostPrefab = ghostPrefabs[Random.Range(0, ghostPrefabs.Length)];
        StartCoroutine(SpawnSequence(pos, ghostPrefab));
    }
}
