using UnityEngine;
using System;
using System.Collections;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target to follow")]
    [SerializeField] Transform target;

    [Header("Camera settings")]
    [SerializeField] float smoothSpeed = 5f;
    [SerializeField] Vector3 offset;

    [Header("Patrol Settings")]
    public Transform[] ghostPoints;      // ゴーストポイント配列（巡回順に並べる）
    public float patrolMoveSpeed = 3f;   // カメラ巡回スピード
    public float lookTime = 1.5f;        // 各ポイントで停止する時間

    [Header("Player control")]
    [SerializeField] PlayerController playerController; // プレイヤー操作スクリプト

    // 🔵 イベント
    public Action OnPatrolStarted;                  // パトロール開始時
    public Action<Transform> OnReachedPoint;        // 各ポイント到達時（スポーン用）

    private bool isPatrolling = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // 安全対策：未設定なら target 側から探す
        if (playerController == null && target != null)
        {
            playerController = target.GetComponentInParent<PlayerController>();
        }
    }
    private void Start()
    {
        // ghostPoints が空なら、シーンから自動で読み込む
        if (ghostPoints == null || ghostPoints.Length == 0)
        {
            GameObject pointsRoot = GameObject.Find("GhostPoints");
            if (pointsRoot != null)
            {
                ghostPoints = pointsRoot.GetComponentsInChildren<Transform>();
            }
            else
            {
                Debug.LogWarning("GhostPoints object not found in scene");
            }
        }
    }
    void LateUpdate()
    {
        if (isPatrolling) return;
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    //---------------------------------------------------------------
    // 🔴 外部から呼ぶ「パトロール開始」
    //---------------------------------------------------------------
    public void StartPatrol()
    {
        if (!gameObject.activeInHierarchy)
        {
           
            gameObject.SetActive(true);
        }
        if (!isPatrolling)
        {
            OnPatrolStarted?.Invoke();  // ← パトロール開始イベント発火
            StartCoroutine(PatrolRoutine());
        }
    }

    //---------------------------------------------------------------
    // 🔵 パトロール処理（順番にポイント巡回）
    //---------------------------------------------------------------
    IEnumerator PatrolRoutine()
    {
        isPatrolling = true;

        // 🔴 プレイヤー移動停止
        if (playerController != null)
        {
            playerController.enabled = false;

            // Rigidbody2D で動くプレイヤーなら止める
            var rb = playerController.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        // 🔵 ゴーストポイントを順番に巡回
        foreach (Transform point in ghostPoints)
        {
            yield return StartCoroutine(MoveToPoint(point)); // 到達するとイベント発火
            yield return new WaitForSeconds(lookTime);       // その場で停止
        }

        // 🟢 プレイヤー操作復活
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        isPatrolling = false;
    }
    public void SetGhostPoints(Transform[] points)
    {
        ghostPoints = points;
    }
    //---------------------------------------------------------------
    // 🔵 カメラ移動（完了時に OnReachedPoint 発火）
    //---------------------------------------------------------------
    IEnumerator MoveToPoint(Transform point)
    {
        Vector3 targetPos = point.position;
        targetPos.z = transform.position.z;

        // カメラをポイント位置へ滑らかに移動
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPos,
                patrolMoveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 🔥 ポイントに到着した瞬間イベント発火
        OnReachedPoint?.Invoke(point);
    }
}
