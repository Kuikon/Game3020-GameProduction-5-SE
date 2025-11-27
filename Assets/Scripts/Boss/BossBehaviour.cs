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

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float baseWaitTime = 4f;
    [SerializeField] private Vector3 patrolOffset = new(0, 1.5f, 0);
    [SerializeField] private Vector3 spawnOffset = new(0, -1.5f, 0);
    private bool isSpawningAtPoint = false;
    private List<Vector3> patrolPoints = new();
    private int currentIndex = 0;
    private Vector3 basePoint;
    private BossState state = BossState.Idle;
    private float stateTimer = 0f;

    // =========================================================
    // Setup
    // =========================================================
    private void Awake()
    {
        basePoint = transform.position;
        if (graveManager == null)
            graveManager = FindFirstObjectByType<GraveManager>();
        if (spawnerController == null)
            spawnerController = GetComponent<BossSpawnerController>();
    }

    private void Start()
    {
        if (graveManager != null)
        {
            graveManager.InitializeGraves();               
            patrolPoints = graveManager.GetPatrolPoints();      
            Debug.Log($"👻 Patrol points loaded: {patrolPoints.Count}");
        }

        if (patrolPoints.Count > 0)
            SetState(BossState.Patrol);
        else
            SetState(BossState.Idle);
    }

    private void Update()
    {
        switch (state)
        {
            case BossState.Idle: UpdateIdle(); break;
            case BossState.Patrol: UpdatePatrol(); break;
            case BossState.Return: UpdateReturn(); break;
            case BossState.Rebuild: UpdateRebuild(); break;
        }
    }

    // =========================================================
    // State Machine
    // =========================================================
    private void SetState(BossState newState)
    {
        state = newState;
        stateTimer = 0f;
        Debug.Log($"🌀 Boss entered state: {state}");

        switch (newState)
        {
            case BossState.Idle:
                animator.SetFloat("MoveX", 0);
                animator.SetFloat("MoveY", -1);
                break;
            case BossState.Patrol:
                currentIndex = 0;
                break;
            case BossState.Return:
                spawnerController?.StopSpawnLoop();
                break;
            case BossState.Rebuild:
                StartCoroutine(RebuildRoutine());
                break;
        }
    }

    // =========================================================
    // States
    // =========================================================
    private void UpdateIdle()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer >= baseWaitTime && patrolPoints.Count > 0)
            SetState(BossState.Patrol);
    }

    private void UpdatePatrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        Vector3 target = patrolPoints[currentIndex] + patrolOffset;
        MoveTowards(target);
        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            if (!isSpawningAtPoint)
            {
                spawnerController?.StartSpawnLoop(target + spawnOffset);
                isSpawningAtPoint = true;
            }

            stateTimer += Time.deltaTime;

            if (stateTimer >= waitTime)
            {
                spawnerController?.StopSpawnLoop();
                isSpawningAtPoint = false; 
                currentIndex++;

                if (currentIndex >= patrolPoints.Count)
                    SetState(BossState.Return);  
                else
                    stateTimer = 0f;           
            }
        }
        else
        {
            isSpawningAtPoint = false;
        }
    }

    private void UpdateReturn()
    {
        MoveTowards(basePoint);

        if (Vector3.Distance(transform.position, basePoint) < 0.05f)
        {
            SetState(BossState.Idle);
        }
    }

    private void UpdateRebuild()
    {

    }

    private IEnumerator RebuildRoutine()
    {
        yield return StartCoroutine(graveManager.RebuildGravesRoutine());
        patrolPoints = graveManager.GetPatrolPoints();
        SetState(BossState.Patrol);
    }

    private void MoveTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    // =========================================================
    // Event Reaction
    // =========================================================
    private void OnEnable() => GhostEvents.OnGravesCaptured += OnGravesCaptured;
    private void OnDisable() => GhostEvents.OnGravesCaptured -= OnGravesCaptured;

    private void OnGravesCaptured(List<GameObject> captured)
    {
        if (graveManager == null) return;

        graveManager.ReplaceCapturedGraves(captured);
        if (graveManager.AllGravesBroken())
            SetState(BossState.Rebuild);
    }

    // =========================================================
    // External Call (from DragonEgg)
    // =========================================================
    public void InitializeAfterHatch(GraveManager gm)
    {
        graveManager = gm;
        graveManager.InitializeGraves();
        patrolPoints = graveManager.GetPatrolPoints();

        Debug.Log($"🐲 Dragon hatched! Found {patrolPoints.Count} graves to patrol.");
        StartCoroutine(HatchIdleRoutine());
    }
    private IEnumerator HatchIdleRoutine()
    {
        animator.SetFloat("MoveX", 0);
        animator.SetFloat("MoveY", -1);
        transform.position = basePoint;
        state = BossState.Idle;
        stateTimer = 0f;
        yield return new WaitForSeconds(2f);
        SetState(BossState.Patrol);
    }

}

public enum BossState
{
    Idle,      
    Patrol,    
    Return,    
    Rebuild    
}
