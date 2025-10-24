using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Boss patrols between grave tiles and summons ghosts while standing still.
/// </summary>
public class BossBehaviour : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [SerializeField] Tilemap patrolTilemap;
    [SerializeField] TileBase targetTile;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float waitTime = 2f;
    [SerializeField] bool loopPatrol = true;

    [Header("Spawner Reference")]
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] float continuousSpawnInterval = 3f; 

    [Header("Animation")]
    [SerializeField] Animator animator;

    private List<Vector3> patrolPoints = new();
    private int currentIndex = 0;
    private bool isMoving = false;
    private bool isSpawning = false;
    private Coroutine spawnLoop;

    // ============================================================
    // Initialization
    // ============================================================
    private void Start()
    {
        CachePatrolPoints();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (patrolPoints.Count > 0)
        {
            transform.position = patrolPoints[0];
            StartCoroutine(PatrolRoutine());
        }
    }

    // ============================================================
    // Find all patrol tiles
    // ============================================================
    private void CachePatrolPoints()
    {
        patrolPoints.Clear();
        if (patrolTilemap == null)
        {
            Debug.LogError("❌ Patrol Tilemap not assigned!");
            return;
        }

        BoundsInt bounds = patrolTilemap.cellBounds;
        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            TileBase tile = patrolTilemap.GetTile(cellPos);
            if (tile == targetTile)
            {
                Vector3 worldPos = patrolTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.3f, 0f);
                patrolPoints.Add(worldPos);
            }
        }

        Debug.Log($"📍 Found {patrolPoints.Count} patrol tiles.");
    }

    // ============================================================
    // Patrol behavior loop
    // ============================================================
    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (!isMoving)
                StartCoroutine(MoveToNextPoint());
            yield return null;
        }
    }

    // ============================================================
    // Move to the next patrol point
    // ============================================================
    private IEnumerator MoveToNextPoint()
    {
        if (patrolPoints.Count == 0) yield break;
        isMoving = true;

        Vector3 basePos = patrolPoints[currentIndex];
        Vector3 targetPos = basePos + new Vector3(0f, 1.5f, 0f);
        Vector3 startPos = transform.position;

        Vector3 dir = (targetPos - startPos).normalized;

        // 🎬 Animation setup
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
        }

        // Move smoothly toward next point
        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // ✅ Arrived
        transform.position = targetPos;

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", -1f);
        }

        // 💀 Begin ghost spawning while waiting
        if (enemySpawner != null)
        {
            if (spawnLoop != null)
                StopCoroutine(spawnLoop);
            float randomXOffset = Random.Range(-1, 2);
            Vector3 spawnBasePos = basePos + new Vector3(randomXOffset, 0f, 0f);
            spawnLoop = StartCoroutine(SpawnLoop(spawnBasePos));
        }

        yield return new WaitForSeconds(waitTime);

        // 🚫 Stop spawning before moving again
        if (spawnLoop != null)
        {
            StopCoroutine(spawnLoop);
            spawnLoop = null;
        }

        // Move to next patrol point
        currentIndex++;
        if (currentIndex >= patrolPoints.Count)
        {
            if (loopPatrol)
                currentIndex = 0;
            else
                yield break;
        }

        isMoving = false;
    }


    // ============================================================
    // Continuous spawn while boss is stationary
    // ============================================================
    private IEnumerator SpawnLoop(Vector3 pos)
    {
        isSpawning = true;
        while (isSpawning)
        {
            float randomXOffset = Random.Range(-1, 2);
            float randomYOffset = Random.Range(-1, 2) * 0.5f;
            Vector3 randomSpawnPos = pos + new Vector3(randomXOffset,  randomYOffset, 0f);
            enemySpawner.SpawnAtPosition(randomSpawnPos);
            yield return new WaitForSeconds(continuousSpawnInterval);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach (var p in patrolPoints)
            Gizmos.DrawSphere(p, 0.1f);
    }
}
