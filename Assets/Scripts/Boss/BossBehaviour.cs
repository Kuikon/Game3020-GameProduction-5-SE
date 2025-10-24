using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class BossBehaviour : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [SerializeField] Tilemap patrolTilemap;
    [SerializeField] TileBase targetTile;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float waitTime = 1.5f;
    [SerializeField] bool loopPatrol = true;

    [Header("Spawner Reference")]
    [SerializeField] EnemySpawner enemySpawner;

    [Header("Animation")]
    [SerializeField] Animator animator;

    private List<Vector3> patrolPoints = new();
    private int currentIndex = 0;
    private bool isMoving = false;

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

    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (!isMoving)
                StartCoroutine(MoveToNextPoint());
            yield return null;
        }
    }

    private IEnumerator MoveToNextPoint()
    {
        if (patrolPoints.Count == 0) yield break;
        isMoving = true;

        Vector3 targetPos = patrolPoints[currentIndex];
        Vector3 startPos = transform.position;

        // Direction vector
        Vector3 dir = (targetPos - startPos).normalized;

        // 🎬 Set Animator parameters
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
        }

        // Move toward the target
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

        // 👻 Spawn ghost
        if (enemySpawner != null)
            enemySpawner.SpawnAtPosition(targetPos);

        yield return new WaitForSeconds(waitTime);

        // Move to next
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach (var p in patrolPoints)
            Gizmos.DrawSphere(p, 0.1f);
    }
}
