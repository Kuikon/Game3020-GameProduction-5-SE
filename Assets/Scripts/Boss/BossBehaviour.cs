using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GraveManager graveManager;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float pointReachDistance = 0.05f;
    [SerializeField] private float idleDurationAtPoint = 2f;  
    [SerializeField] private float idleDurationAfterHatch = 2.5f;
    [SerializeField] private float stopOffset = 1.0f;
    private List<Vector3> patrolPoints = new();
    private int currentIndex = 0;
    private Vector3 basePoint;

    private BossState state = BossState.Patrol;
    private float stateTimer = 0f;
    [SerializeField] private BossSpawnerController spawnerController;
    private void Awake()
    {
        basePoint = transform.position;

        if (graveManager == null)
            graveManager = FindFirstObjectByType<GraveManager>();

        if (spawnerController == null)
            spawnerController = FindFirstObjectByType<BossSpawnerController>();
    }

    private void Start()
    {
        graveManager.InitializeGraves();
        patrolPoints = graveManager.GetPatrolPoints();

        Debug.Log($"🟦 Loaded {patrolPoints.Count} patrol points.");
    }

    private void Update()
    {
        switch (state)
        {
            case BossState.Patrol: UpdatePatrol(); break;
            case BossState.IdleAtPoint: UpdateIdleAtPoint(); break;
        }
    }
    private void MoveTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
        animator.SetFloat("Speed", 1f);

        // 実際の移動
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );
    }
    // =========================================================
    // Patrol
    // =========================================================
    private void UpdatePatrol()
    {
        if (patrolPoints.Count == 0) return;

        Vector3 target = patrolPoints[currentIndex] + new Vector3(0, stopOffset, 0);
        MoveTowards(target);

        if (Vector3.Distance(transform.position, target) < pointReachDistance)
        {
            // 本当の墓の位置でチェックする
            Vector3 gravePos = patrolPoints[currentIndex];
            CheckGraveAtPoint(gravePos);

            EnterIdleState(idleDurationAtPoint);
        }
    }

    // =========================================================
    // Idle state (停止中)
    // =========================================================
    private void EnterIdleState(float duration)
    {
        state = BossState.IdleAtPoint;
        stateTimer = duration;
        animator.SetFloat("MoveX", 0);
        animator.SetFloat("MoveY", -1);
        animator.SetFloat("Speed", 0);
        SoundManager.Instance.PlaySE(SESoundData.SE.DragonStart);
        if (spawnerController != null)
            spawnerController.StartSpawnLoop(transform.position);
    }

    private void UpdateIdleAtPoint()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer > 0f) return;
        if (spawnerController != null)
            spawnerController.StopSpawnLoop();
        GoToNextPoint();
        state = BossState.Patrol;
    }

    private void GoToNextPoint()
    {
        currentIndex++;
        if (currentIndex >= patrolPoints.Count)
            currentIndex = 0;
    }

    // =========================================================
    // Grave interaction
    // =========================================================
    private void CheckGraveAtPoint(Vector3 pos)
    {
        GameObject broken = graveManager.GetBrokenGraveAt(pos);
        if (broken != null)
        {
            graveManager.RepairBrokenGrave(broken);
            Debug.Log("🪦 Boss repaired a broken grave.");
            return;
        }

        HealBoss();
    }

    private void HealBoss()
    {
        var hp = GetComponent<DragonHealth>();
        if (hp != null)
        {
            hp.Heal(1);
            Debug.Log("💖 Boss healed at a normal grave.");
        }
    }

    // =========================================================
    // Called from DragonEgg
    // =========================================================
    public void InitializeAfterHatch(GraveManager gm)
    {
        graveManager = gm;
        graveManager.InitializeGraves();
        patrolPoints = graveManager.GetPatrolPoints();

        currentIndex = 0;
        transform.position = basePoint;

        Debug.Log($"🐉 Boss: Hatched → {patrolPoints.Count} patrols loaded.");
        EnterIdleState(idleDurationAfterHatch);
    }
}

public enum BossState
{
    Patrol,
    IdleAtPoint
}
