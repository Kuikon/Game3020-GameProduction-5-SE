using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class BossBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GraveManager graveManager;
    [SerializeField] private BossSpawnerController spawnerController;
    [SerializeField] private Animator animator;
    [SerializeField] private UIBoxManager uiBoxManager;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float baseWaitTime = 4f;
    [SerializeField] private Vector3 patrolOffset = new(0, 1.5f, 0);
    [SerializeField] private Vector3 spawnOffset = new(0, -1.5f, 0);

    private List<Vector3> patrolPoints = new();
    private int currentIndex = 0;
    private bool hasCompletedLoop = false;
    private Vector3 basePoint;
    private Coroutine patrolRoutine;

    // =========================================================
    // Setup
    // =========================================================
    private void Awake()
    {
        basePoint = transform.position;

        // 🔹 自動的に参照を見つける
        if (graveManager == null)
            graveManager = FindFirstObjectByType<GraveManager>();
        if (spawnerController == null)
            spawnerController = GetComponent<BossSpawnerController>();
        if (uiBoxManager == null)
            uiBoxManager = FindFirstObjectByType<UIBoxManager>();
    }

    private void Start()
    {
        if (graveManager != null)
        {
            graveManager.InitializeGraves();
            patrolPoints = graveManager.GetPatrolPoints();
            Debug.Log($"📍 Patrol points count: {patrolPoints.Count}");
        }
        else
        {
            Debug.LogError("❌ GraveManager reference is missing in BossBehaviour!");
        }

        if (patrolPoints.Count > 0)
            patrolRoutine = StartCoroutine(PatrolRoutine());
        else
            Debug.LogWarning("⚠️ No patrol points available at Start!");
    }


    private void OnEnable() => GhostEvents.OnGravesCaptured += OnGravesCaptured;
    private void OnDisable() => GhostEvents.OnGravesCaptured -= OnGravesCaptured;

    // =========================================================
    // Event Reaction
    // =========================================================
    public void OnGravesCaptured(List<GameObject> captured)
    {
        if (graveManager == null) return;

        graveManager.ReplaceCapturedGraves(captured);

        if (graveManager.AllGravesBroken())
        {
            Debug.Log("⚰️ All graves broken — starting rebuild...");
            StopAllCoroutines();
            StartCoroutine(RebuildAndRestart());
        }
        else
        {
            patrolPoints = graveManager.GetPatrolPoints();
            currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, patrolPoints.Count - 1)); // ✅ 安全リセット
            RestartPatrol();
        }
    }

    private IEnumerator RebuildAndRestart()
    {
        if (graveManager == null) yield break;

        yield return StartCoroutine(graveManager.RebuildGravesRoutine());
        patrolPoints = graveManager.GetPatrolPoints();
        currentIndex = 0;
        RestartPatrol();
    }

    private void RestartPatrol()
    {
        if (patrolRoutine != null)
            StopCoroutine(patrolRoutine);

        if (patrolPoints.Count == 0)
        {
            Debug.LogWarning("⚠️ No patrol points to restart.");
            return;
        }

        patrolRoutine = StartCoroutine(PatrolRoutine());
    }

    // =========================================================
    // Patrol Logic
    // =========================================================
    private IEnumerator PatrolRoutine()
    {
        uiBoxManager?.ClearBoxes();

        while (true)
        {
            if (patrolPoints == null || patrolPoints.Count == 0)
            {
                Debug.LogWarning("⚠️ Patrol points empty, waiting...");
                yield break;
            }

            yield return StartCoroutine(MoveToNextPoint());

            if (hasCompletedLoop)
            {
                yield return StartCoroutine(ReturnToBasePoint());
                hasCompletedLoop = false;
            }
        }
    }

    private IEnumerator MoveToNextPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) yield break;

        currentIndex = Mathf.Clamp(currentIndex, 0, patrolPoints.Count - 1);
        Vector3 target = patrolPoints[currentIndex] + patrolOffset;

        Vector3 dir = (target - transform.position).normalized;
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);

        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetFloat("MoveX", 0f);
        animator.SetFloat("MoveY", -1f);

        spawnerController?.StartSpawnLoop(target + spawnOffset);
        yield return new WaitForSeconds(waitTime);
        spawnerController?.StopSpawnLoop();

        currentIndex++;
        if (currentIndex >= patrolPoints.Count)
        {
            currentIndex = 0;
            hasCompletedLoop = true;
        }
    }

    private IEnumerator ReturnToBasePoint()
    {
        Vector3 dir = (basePoint - transform.position).normalized;
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);

        while (Vector3.Distance(transform.position, basePoint) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, basePoint, moveSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetFloat("MoveX", 0);
        animator.SetFloat("MoveY", -1f);
        uiBoxManager?.SpawnBoxes(basePoint);
        yield return new WaitForSeconds(baseWaitTime);
    }

    // =========================================================
    // External Setters (used by DragonEgg)
    // =========================================================
    public void SetGraveManager(GraveManager gm)
    {
        graveManager = gm;
    }

    public void SetEnemySpawner(EnemySpawner es)
    {
        if (spawnerController != null)
            spawnerController.SetEnemySpawner(es);
    }
}
