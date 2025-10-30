using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BossBehaviour : MonoBehaviour
{
    [Header("Grave Settings")]
    [SerializeField] private GameObject gravePrefab;     
    [SerializeField] private GameObject brokenGravePrefab;
    [SerializeField] private int initialGraveCount = 5;
    [SerializeField] private int lastGraveCount;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float baseWaitTime = 4f;
    [SerializeField] private Vector3 patrolOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, -1.5f, 0f);

    [Header("Spawner Reference")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private float continuousSpawnInterval = 3f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [SerializeField] private Boundry xBoundary;
    [SerializeField] private Boundry yBoundary;
    private List<GameObject> graveList = new();
    private List<Vector3> patrolPoints = new();
    private int currentIndex = 0;
    private bool hasCompletedLoop = false;
    private Coroutine spawnLoop;
    [HideInInspector] public Vector3 basePoint;

    private void Awake()
    {
        basePoint = transform.position;
    }

    // ============================================================
    // Initialization
    // ============================================================
    private void Start()
    {
        lastGraveCount = initialGraveCount;
        SpawnInitialGraves(initialGraveCount);
        CacheGravePositions();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (patrolPoints.Count > 0)
        {
            transform.position = patrolPoints[0];
            StartCoroutine(PatrolRoutine());
        }
    }
    private void SpawnInitialGraves(int count)
    {
        float minDistance = 2f;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetValidSpawnPosition(minDistance);
            GameObject grave = Instantiate(gravePrefab, pos, Quaternion.identity);
            graveList.Add(grave);
        }
        Debug.Log($"🪦 Spawned {count} graves at start.");
    }
    // ============================================================
    // Find all grave GameObjects
    // ============================================================
    private void CacheGravePositions()
    {
        patrolPoints.Clear();
        List<GameObject> aliveGraves = new();
        foreach (GameObject grave in graveList)
        {
            if (grave == null) continue;
            if (!grave.CompareTag("BrokenGrave"))
            {
                aliveGraves.Add(grave);
                patrolPoints.Add(grave.transform.position);
            }
        }
        graveList = aliveGraves;
        Debug.Log($"📍 Found {patrolPoints.Count} active graves for patrol.");
    }

    // ============================================================
    // Patrol behavior loop
    // ============================================================
    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(MoveToNextPoint());

            if (hasCompletedLoop)
            {
                yield return StartCoroutine(ReturnToBasePoint());
                hasCompletedLoop = false;
            }
        }
    }

    // ============================================================
    // Move to the next patrol point
    // ============================================================
    private IEnumerator MoveToNextPoint()
    {
      
        if (patrolPoints.Count == 0) yield break;

        Vector3 basePos = patrolPoints[currentIndex];
        Vector3 targetPos = basePos + patrolOffset;
        Vector3 startPos = transform.position;
        Vector3 dir = (targetPos - startPos).normalized;

        if (animator != null)
        {
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
        }

        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        if (animator != null)
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", -1f);
        }

        // 💀 Ghost spawn
        if (enemySpawner != null)
        {
            if (spawnLoop != null)
                StopCoroutine(spawnLoop);
            spawnLoop = StartCoroutine(SpawnLoop(targetPos));
        }

        yield return new WaitForSeconds(waitTime);

        if (spawnLoop != null)
        {
            StopCoroutine(spawnLoop);
            spawnLoop = null;
        }

        currentIndex++;
        if (currentIndex >= patrolPoints.Count)
        {
            hasCompletedLoop = true;
            currentIndex = 0;
        }

    }

    private IEnumerator ReturnToBasePoint()
    {
        Debug.Log("🏰 Returning to base point...");

        Vector3 dir = (basePoint - transform.position).normalized;
        if (animator != null)
        {
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
        }

        while (Vector3.Distance(transform.position, basePoint) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, basePoint, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = basePoint;

        if (animator != null)
        {
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("MoveY", -1f);
        }

        Debug.Log($"😴 Resting at base for {baseWaitTime} seconds...");
        yield return new WaitForSeconds(baseWaitTime);
    }
    private bool AllGravesBroken()
    {
        foreach (GameObject g in graveList)
        {
            if (g != null && !g.CompareTag("BrokenGrave"))
                return false;
        }
        return true;
    }
    // ============================================================
    // Continuous ghost spawning
    // ============================================================
    private IEnumerator SpawnLoop(Vector3 pos)
    {
        while (true)
        {
            Vector3 basePos = pos + spawnOffset;
            Vector3 spawnPos = basePos + new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 0.5f), 0f);
            enemySpawner.SpawnAtPosition(spawnPos);
            yield return new WaitForSeconds(continuousSpawnInterval);
        }
    }
   

    // ============================================================
    // Replace only *captured* graves with broken versions
    // ============================================================
    public void ReplaceCapturedGraves(List<GameObject> captured)
    {
        foreach (var grave in captured)
        {
            if (grave == null) continue;
            Vector3 pos = grave.transform.position;
            graveList.Remove(grave);
            Destroy(grave);
            GameObject broken = Instantiate(brokenGravePrefab, pos, Quaternion.identity);
            broken.tag = "BrokenGrave";
        }

        RecalculatePatrolPoints();
        if (AllGravesBroken())
        {
            Debug.Log("⚰️ All graves broken — rebuilding triggered manually!");
            StopAllCoroutines();
            StartCoroutine(RebuildGravesRoutine());
        }
    }
    private IEnumerator RebuildGravesRoutine()
    {
        yield return new WaitForSeconds(2f);

        int newCount = lastGraveCount + 1;
        lastGraveCount = newCount;
        graveList.Clear();
        patrolPoints.Clear();
        float minDistance = 2f;
        for (int i = 0; i < newCount; i++)
        {
            Vector3 pos = GetValidSpawnPosition(minDistance);
            GameObject grave = Instantiate(gravePrefab, pos, Quaternion.identity);
            graveList.Add(grave);
        }

        Debug.Log($"🌱 Rebuilt {newCount} new graves!");

        CacheGravePositions();
        currentIndex = 0;

        StartCoroutine(PatrolRoutine());

    }
    public void RecalculatePatrolPoints()
    {
        Debug.Log("🧭 Boss Recalculating Patrol Points...");

        StopAllCoroutines();
        CacheGravePositions(); 
        currentIndex = 0;
        hasCompletedLoop = false;

        if (patrolPoints.Count > 0)
            StartCoroutine(PatrolRoutine());
    }
    private Vector3 GetValidSpawnPosition(float minDistance)
    {
        Vector3 pos = Vector3.zero;
        bool validPosition = false;
        int safety = 0;
        do
        {
            pos = new Vector3(
                Random.Range(xBoundary.min, xBoundary.max),
                Random.Range(yBoundary.min, yBoundary.max),
                0f
            );

            validPosition = true;
            foreach (var existing in graveList)
            {
                if (existing == null) continue;
                if (Vector3.Distance(pos, existing.transform.position) < minDistance)
                {
                    validPosition = false;
                    break;
                }
            }
            safety++;
            if (safety > 100)
            {
                Debug.LogWarning("⚠️ Could not find valid non-overlapping position!");
                break;
            }

        } while (!validPosition);
        return pos;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach (var p in patrolPoints)
            Gizmos.DrawSphere(p, 0.15f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(basePoint, 0.2f);
    }

}
