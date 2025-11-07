using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class GhostBase : MonoBehaviour
{
    public enum DeathCause
    {
        Default,    // 通常 or 捕獲など
        Suicide     // 自爆による死亡
    }
    [Header("Ghost Data")]
    public GhostData data;

    [Header("Movement Boundaries")]
    [SerializeField] Boundry xBoundary;
    [SerializeField] Boundry yBoundary;

    private Transform capturePoint;
    private Transform releasePoint;
    [SerializeField] private float captureSpeed = 2f;
 
    public  bool isDead;
    private bool isStartIdle = true;
    private bool quickSpeedBoosted = false;
    private bool absorbedByDragon = false;
    private bool suicideTargetSet = false;

    private Vector3 suicideTargetPos;
    private Vector2 moveDir;
    private Coroutine scaleRoutine;

    private float floatTimer; 
    private float blinkTimer;
    private float dirTimer;
    private float lifeTimer;
    private float chargeTimer;
    private float startIdleTimer = 1f;
    private float bounceCooldown = 0f;
    private float absorbRadius = 3f;     
    private float absorbCooldown = 0f;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private DroppedBall targetBall = null;
    // ============================================================
    // Initialization
    // ============================================================
    void Start()
    {
        MiniMapManager.Instance?.RegisterGhost(gameObject);
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        var obj = GameObject.Find("Player");
        if (obj != null)
            capturePoint = obj.transform;
        var obj1 = GameObject.Find("GhostBallSpawner");
        if (obj1 != null)
            releasePoint = obj1.transform;
        lifeTimer = data.absorbTime;
        ApplyVisualStyleByType();
    }

    // ============================================================
    // Main Update Loop
    // ============================================================
    void Update()
    {
        if (HandleStartIdle()) return;
        if (HandleDeadState()) return;

        HandleLifeTimer();
        HandleGhostTypeBehavior();
    }
    void FixedUpdate()
    {
        if (isDead || isStartIdle) return;
        HandleMovement();
    }
    // ============================================================
    // 1-second idle state at the beginning
    // ============================================================
    private bool HandleStartIdle()
    {
        if (!isStartIdle) return false;

        startIdleTimer -= Time.deltaTime;
        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("Speed", 0f);

        if (startIdleTimer <= 0f)
            isStartIdle = false;

        return true;
    }

    // ============================================================
    // Dead or being absorbed state
    // ============================================================
    private bool HandleDeadState()
    {
        if (!isDead) return false;
        StartCoroutine(DestroyAfterDelay(0.2f));
        return true;
    }
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (GhostBallSpawner.Instance != null)
        {
            GhostBallSpawner.Instance.SpawnBulletByType(data.type, transform.position);
        }
        Destroy(gameObject);
    }
    // ============================================================
    // Movement and boundary handling
    // ============================================================

    private void HandleMovement()
    {
        if (data.type == GhostType.Suicide && suicideTargetSet)
            return;
        dirTimer -= Time.fixedDeltaTime;
        bounceCooldown -= Time.fixedDeltaTime;

        if (dirTimer <= 0f)
            PickRandomDirection();
        data.walkSpeed = 1f;
        // Move naturally
        rb.linearVelocity = moveDir * data.walkSpeed;

        Vector2 pos = rb.position;
        float epsilon = 0.05f; // small safety margin

        // ---- X boundary ----
        if (bounceCooldown <= 0f)
        {
            if (pos.x < xBoundary.min + epsilon)
            {
                pos.x = xBoundary.min + epsilon;
                moveDir.x = Mathf.Abs(moveDir.x); // push right
                bounceCooldown = 0.2f;
            }
            else if (pos.x > xBoundary.max - epsilon)
            {
                pos.x = xBoundary.max - epsilon;
                moveDir.x = -Mathf.Abs(moveDir.x); // push left
                bounceCooldown = 0.2f;
            }

            // ---- Y boundary ----
            if (pos.y < yBoundary.min + epsilon)
            {
                pos.y = yBoundary.min + epsilon;
                moveDir.y = Mathf.Abs(moveDir.y); // push up
                bounceCooldown = 0.2f;
            }
            else if (pos.y > yBoundary.max - epsilon)
            {
                pos.y = yBoundary.max - epsilon;
                moveDir.y = -Mathf.Abs(moveDir.y); // push down
                bounceCooldown = 0.2f;
            }
        }


        // Update animation
        animator.SetFloat("MoveX", moveDir.x);
        animator.SetFloat("MoveY", moveDir.y);
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
    }

    // ============================================================
    // Life timer behavior
    // ============================================================
    private void HandleLifeTimer()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer > 0f) return;

        if (data.type == GhostType.Normal || data.type == GhostType.Quick || data.type == GhostType.Tank||data.type == GhostType.Suicide|| data.type == GhostType.Lucky)
        {
            GameObject dragon = GameObject.Find("Dragon");
            if (dragon != null)
                Absorb(dragon.transform, true);
        }
    }

    // ============================================================
    // Change appearance depending on GhostType
    // ============================================================
  private void ApplyVisualStyleByType()
{
    if (data == null)
    {
        Debug.LogWarning($"{name}: GhostData not assigned!");
        return;
    }

    // 🧩 基本見た目（色とスケール）
    spriteRenderer.color = data.ghostColor;
    transform.localScale = Vector3.one * data.baseScale;

    // タイマー初期化（アニメ用）
    floatTimer = Random.Range(0f, Mathf.PI * 2f);
    blinkTimer = Random.Range(0f, Mathf.PI * 2f);
}
    private void HandleVisualEffects()
    {
        if (spriteRenderer == null || data == null) return;

        // 🎈 上下ふわふわアニメーション
        floatTimer += Time.deltaTime * data.floatSpeed;
        float offsetY = Mathf.Sin(floatTimer) * data.floatAmplitude;

        // ゴーストの位置をふわっと上下させる
        Vector3 pos = transform.position;
        pos.y += offsetY * Time.deltaTime; // 徐々に反映してなめらかに
        transform.position = pos;

        // 💡 点滅アニメーション（blinkSpeed > 0 の場合のみ）
        if (data.blinkSpeed > 0f && data.blinkIntensity > 0f)
        {
            blinkTimer += Time.deltaTime * data.blinkSpeed;
            float blink = Mathf.Sin(blinkTimer) * 0.5f + 0.5f;
            float brightness = Mathf.Lerp(1f - data.blinkIntensity, 1f, blink);

            Color c = data.ghostColor * brightness;
            c.a = data.ghostColor.a; // 透明度は維持
            spriteRenderer.color = c;
        }
    }
    public static Color GetColorByType(GhostType type)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager not found! Returning gray color.");
            return Color.gray;
        }

        GhostData data = GameManager.Instance.GetGhostData(type);
        if (data == null)
        {
            Debug.LogWarning($"No GhostData found for type: {type}");
            return Color.gray;
        }

        return data.ghostColor;
    }
    // ============================================================
    // Type-specific ghost behavior
    // ============================================================
    private void HandleGhostTypeBehavior()
    {
        switch (data.type)
        {
            case GhostType.Quick:
                if (!quickSpeedBoosted)
                {
                    data.walkSpeed *= 2f;
                    quickSpeedBoosted = true;
                }
                break;
            case GhostType.Tank:
                UpdateTank();
                break;
            case GhostType.Suicide:
                UpdateSuicide();
                break;
            case GhostType.Lucky:
                UpdateLucky();
                break;
        }
    }

    // ============================================================
    // Suicide-type ghost behavior
    // ============================================================
    private void UpdateSuicide()
    {
        // 🔹 死亡中は一切動かさない
        if (isDead) return;

        if (!suicideTargetSet)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Camera.main == null) return;

                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

                suicideTargetPos = worldPos;
                suicideTargetSet = true;

                Vector2 dir = (worldPos - transform.position).normalized;
                animator.SetFloat("MoveX", dir.x);
                animator.SetFloat("MoveY", dir.y);
                animator.SetFloat("Speed", 1f);

                Debug.Log($"🎯 Suicide target set: {suicideTargetPos}");
            }
            return;
        }

        // 🔹 ターゲットに突進
        transform.position = Vector3.MoveTowards(
            transform.position,
            suicideTargetPos,
            data.walkSpeed * 2f * Time.deltaTime
        );

        // 🔹 到達チェック
        if (Vector3.Distance(transform.position, suicideTargetPos) < 0.3f)
        {
            suicideTargetSet = false;
            Kill(DeathCause.Suicide);
        }
    }



    // ============================================================
    // Lucky-type ghost behavior
    // ============================================================
    private void UpdateLucky()
    {
       
    }

    private void UpdateTank()
    {
        // 🔹 クールダウン処理
        absorbCooldown -= Time.deltaTime;
        if (absorbCooldown > 0f) return;

        // 🔹 ターゲットが未設定なら探す
        if (targetBall == null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, absorbRadius);
            float nearestDist = float.MaxValue;
            DroppedBall nearestBall = null;

            foreach (var hit in hits)
            {
                DroppedBall ball = hit.GetComponent<DroppedBall>();
                if (ball != null && ball.isActiveAndEnabled)
                {
                    float dist = Vector2.Distance(transform.position, ball.transform.position);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestBall = ball;
                    }
                }
            }

            if (nearestBall != null)
            {
                targetBall = nearestBall;  // 👈 DroppedBallをターゲットに
                Debug.Log($"🧲 Tank {name} locked onto DroppedBall {targetBall.name}");
            }
        }

        // 🔹 ターゲットが存在すれば吸収方向へ移動
        if (targetBall != null)
        {
            Vector3 targetPos = targetBall.transform.position;
            float step = data.walkSpeed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

            // 💫 近づいたら吸収処理
            float dist = Vector2.Distance(transform.position, targetPos);
            if (dist < 0.5f)
            {
                // 🔹 DroppedBallを吸収（消す）
                targetBall.CollectTo(transform);
                targetBall = null;

                absorbCooldown = 1.5f; // 🔹 次の吸収までの待機時間
                Debug.Log($"🟢 Tank {name} absorbed a DroppedBall!");
            }
        }
    }

    public IEnumerator MoveToPointAndFreeze(Vector3 targetPos, float speed)
    {
        if (rb == null) yield break;

        while (true)
        {
            if (this == null || gameObject == null || transform == null)
                yield break;

            if (Vector3.Distance(transform.position, targetPos) <= 0.1f)
                break;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        if (rb != null)
            rb.simulated = false;

        if (animator != null)
            animator.SetFloat("Speed", 0f);
    }


    public void ResumeMovement()
    {
        if (rb != null)
            rb.simulated = true;
        animator.SetFloat("Speed", 1f);
    }
    // ============================================================
    // Common utilities: Kill / Absorb / Random Direction
    // ============================================================
    public void Kill(DeathCause cause = DeathCause.Default)
    {
        if (isDead) return;
        isDead = true;
        animator.SetBool("Dead", true);

        // 🔹 自爆時だけ爆発エフェクトを生成
        if (cause == DeathCause.Suicide &&
            data.type == GhostType.Suicide &&
            data.fireCirclePrefab != null)
        {
            Vector3 pos = transform.position;
            pos.z = 0;
            var circle = Instantiate(data.fireCirclePrefab, pos, Quaternion.identity);
            circle.transform.localScale = Vector3.one;
            Destroy(circle, data.fireCircleLifetime);
            Debug.Log("💥 Suicide explosion created!");
        }

        if (data.type == GhostType.Lucky)
        {
            GameManager.Instance?.AddLuckyScore();
        }
    }

    public void Absorb(Transform absorbPoint, bool isFromDragon = false)
    {
        if (isDead) return;
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        animator.SetBool("Dead", true);

        capturePoint = absorbPoint;
        absorbedByDragon = isFromDragon;
    }

    private void PickRandomDirection()
    {
        int x, y;

        do
        {
            x = Random.Range(-1, 2);
            y = Random.Range(-1, 2);
        } while (x == 0 && y == 0); 

        moveDir = new Vector2(x, y).normalized;
        dirTimer = data.changeDirectionTime;
    }
    public void Shrink(float scaleMultiplier = 0.8f, float duration = 0.3f)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        float currentScale = transform.localScale.x;  // 現在のサイズを取得
        float targetScale = currentScale * scaleMultiplier; // さらに0.8倍に

        scaleRoutine = StartCoroutine(ScaleTo(targetScale, duration));
    }

    public void Restore(float duration = 0.3f)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        float targetScale = (data != null) ? data.baseScale : 1f;

        scaleRoutine = StartCoroutine(ScaleTo(targetScale, duration));
    }

    private IEnumerator ScaleTo(float targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            yield return null;
        }

        transform.localScale = endScale; // ← 最後に正確に固定
    }

}
