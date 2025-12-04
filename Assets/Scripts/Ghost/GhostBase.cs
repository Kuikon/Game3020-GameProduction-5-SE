using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class GhostBase : MonoBehaviour
{
    public enum DeathCause
    {
        Default,   
        Suicide   
    }
    [Header("Ghost Data")]
    public GhostData data;

    [Header("Movement Boundaries")]
    [SerializeField] Boundry xBoundary;
    [SerializeField] Boundry yBoundary;
    public Vector3 originalScale;
    private Transform capturePoint;
 
    public  bool isDead;
    private bool isStartIdle = true;
    public bool isCaptured = false;
    private bool suicideTargetSet = false;
    private bool isAbsorbing = false;

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
    private float orbitAngle = 0f;      
    private float orbitRadius = 2.5f;   
    private float orbitSpeed = 2f;
    // ============================================================
    // Initialization
    // ============================================================
    void Start()
    {

        lifeTimer = data.absorbTime;
        MiniMapManager.Instance?.RegisterGhost(gameObject);
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        var obj = GameObject.Find("Player");
        if (obj != null)
            capturePoint = obj.transform;
        lifeTimer = data.absorbTime;
        ApplyVisualStyleByType();
        originalScale = transform.localScale;
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
        if (isAbsorbing)
            return true;
        StartCoroutine(DestroyAfterDelay(0.2f));
        return true;
    }
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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
        if (data.type != GhostType.Normal && data.type != GhostType.Quick)
        {
            if (dirTimer <= 0f)
                PickRandomDirection();
        }

        float speed = data.walkSpeed;
        rb.linearVelocity = moveDir * speed;
        Vector2 pos = rb.position;
        float epsilon = 0.05f;

        if (bounceCooldown <= 0f)
        {
            if (pos.x < xBoundary.min + epsilon)
            {
                pos.x = xBoundary.min + epsilon;
                moveDir.x = Mathf.Abs(moveDir.x);
                bounceCooldown = 0.2f;
            }
            else if (pos.x > xBoundary.max - epsilon)
            {
                pos.x = xBoundary.max - epsilon;
                moveDir.x = -Mathf.Abs(moveDir.x);
                bounceCooldown = 0.2f;
            }

            if (pos.y < yBoundary.min + epsilon)
            {
                pos.y = yBoundary.min + epsilon;
                moveDir.y = Mathf.Abs(moveDir.y);
                bounceCooldown = 0.2f;
            }
            else if (pos.y > yBoundary.max - epsilon)
            {
                pos.y = yBoundary.max - epsilon;
                moveDir.y = -Mathf.Abs(moveDir.y);
                bounceCooldown = 0.2f;
            }
        }

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

        // 10秒 → 9.9 → ... → 0.0 → -0.01
        if (lifeTimer > 0f)
            return;

        // 一度だけ実行
        lifeTimer = 0f;

        if (data.type == GhostType.Normal ||
            data.type == GhostType.Quick ||
            data.type == GhostType.Tank ||
            data.type == GhostType.Suicide ||
            data.type == GhostType.Lucky)
        {
            GameObject dragon = GameObject.Find("Boss(Clone)");

            if (dragon != null)
                Absorb(dragon.transform, true);
            else
                Debug.Log("❌ Dragon not found");
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

    spriteRenderer.color = data.ghostColor;
    transform.localScale = Vector3.one * data.baseScale;

    floatTimer = Random.Range(0f, Mathf.PI * 2f);
    blinkTimer = Random.Range(0f, Mathf.PI * 2f);
}
    private void HandleVisualEffects()
    {
        if (spriteRenderer == null || data == null) return;
        floatTimer += Time.deltaTime * data.floatSpeed;
        float offsetY = Mathf.Sin(floatTimer) * data.floatAmplitude;
        Vector3 pos = transform.position;
        pos.y += offsetY * Time.deltaTime;
        transform.position = pos;
        if (data.blinkSpeed > 0f && data.blinkIntensity > 0f)
        {
            blinkTimer += Time.deltaTime * data.blinkSpeed;
            float blink = Mathf.Sin(blinkTimer) * 0.5f + 0.5f;
            float brightness = Mathf.Lerp(1f - data.blinkIntensity, 1f, blink);

            Color c = data.ghostColor * brightness;
            c.a = data.ghostColor.a; 
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
            case GhostType.Normal:
                UpdateNormal();   
                break;
            case GhostType.Quick:
                UpdateQuick();
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
    private void UpdateNormal()
    {
        if (capturePoint == null) return; 

        Vector2 dir = (capturePoint.position - transform.position);
        if (dir.sqrMagnitude < 0.001f) return; 
        moveDir = dir.normalized;
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


    private void UpdateQuick()
    {
        if (capturePoint == null) return;

        float dist = Vector2.Distance(transform.position, capturePoint.position);

        //① プレイヤーから離れすぎている → 近づく
        if (dist > orbitRadius + 0.2f)
        {
            moveDir = (capturePoint.position - transform.position).normalized;
            rb.linearVelocity = moveDir * data.walkSpeed;
            return;
        }

        //② プレイヤーに近すぎる → 離れる
        if (dist < orbitRadius - 0.2f)
        {
            moveDir = (transform.position - capturePoint.position).normalized;
            rb.linearVelocity = moveDir * data.walkSpeed;
            return;
        }

        //③ ちょうどの距離 → 周回運動
        orbitAngle += orbitSpeed * Time.deltaTime;
        float rad = orbitAngle;

        Vector2 orbitPos = new Vector2(
            capturePoint.position.x + Mathf.Cos(rad) * orbitRadius,
            capturePoint.position.y + Mathf.Sin(rad) * orbitRadius
        );

        moveDir = (orbitPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = moveDir * data.walkSpeed;

        animator.SetFloat("MoveX", moveDir.x);
        animator.SetFloat("MoveY", moveDir.y);
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
    }



    // ============================================================
    // Lucky-type ghost behavior
    // ============================================================
    private void UpdateLucky()
    {
       
    }

    private void UpdateTank()
    {
        absorbCooldown -= Time.deltaTime;
        if (absorbCooldown > 0f) return;
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
                targetBall = nearestBall;  
                Debug.Log($"🧲 Tank {name} locked onto DroppedBall {targetBall.name}");
            }
        }

        if (targetBall != null)
        {
            Vector3 targetPos = targetBall.transform.position;
            float step = data.walkSpeed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
            float dist = Vector2.Distance(transform.position, targetPos);
            if (dist < 0.5f)
            {
                targetBall.CollectTo(transform);
                targetBall = null;

                absorbCooldown = 1.5f; 
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
        isCaptured = true;

        if (cause == DeathCause.Suicide &&
            data.type == GhostType.Suicide &&
            data.fireCirclePrefab != null)
        {
            Vector3 pos = transform.position;
            pos.z = 0;
            var circle = Instantiate(data.fireCirclePrefab, pos, Quaternion.identity);
            SoundManager.Instance.PlaySE(SESoundData.SE.SuicideFire);
            circle.transform.localScale = Vector3.one;
            Destroy(circle, data.fireCircleLifetime);
        }

        // ★ Destroy は HandleDeadState() に任せるので入れない
    }


    public void Absorb(Transform absorbPoint, bool isFromDragon = false)
    {
        if (isDead) return;
        isDead = true;
        isAbsorbing = true;
        animator.SetBool("Dead", true);
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        StartCoroutine(AbsorbSequence(absorbPoint));
    }
    private IEnumerator AbsorbSequence(Transform absorbPoint)
    {
        float speed = 8f;

        while (Vector3.Distance(transform.position, absorbPoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                absorbPoint.position,
                speed * Time.deltaTime
            );
            yield return null;
        }
        var dragonHP = absorbPoint.GetComponent<DragonHealth>();
        if (dragonHP != null)
        {
            dragonHP.Heal(1);
        }
        Destroy(gameObject);
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
    public void Shrink(float ratio)
    {
        float targetScale = originalScale.x * ratio;

        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        bool isFinalCaptureShrink = ratio <= 0.6f;

        scaleRoutine = StartCoroutine(
            ScaleTo(targetScale, 0.15f, () =>
            {
                if (isFinalCaptureShrink && !isCaptured)
                {
                    isCaptured = true;
                    Kill();              
                    Destroy(gameObject);  
                }
            })
        );
    }


    public void Restore(float duration = 0.3f)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        float targetScale = (data != null) ? data.baseScale : 1f;

        scaleRoutine = StartCoroutine(ScaleTo(targetScale, duration));
    }

    private IEnumerator ScaleTo(float targetScale, float duration, System.Action onComplete = null)
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

        transform.localScale = endScale;

        onComplete?.Invoke(); 
    }


}
