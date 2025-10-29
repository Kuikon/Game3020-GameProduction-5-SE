using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class GhostBase : MonoBehaviour
{
    [Header("Ghost Data")]
    [SerializeField] GhostData data;

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
    private float dirTimer;
    private float lifeTimer;
    private float chargeTimer;
    private float startIdleTimer = 1f;
    private float bounceCooldown = 0f;
    private float absorbRadius = 3f;     
    private float absorbCooldown = 0f;
    private int runtimeLuckyScore = 0;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BallController targetBall = null;
    // ============================================================
    // Initialization
    // ============================================================
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        var obj = GameObject.Find("CapturePoint");
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

        if (capturePoint != null)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                capturePoint.position,
                captureSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, capturePoint.position) < 1f)
            {

                if (!absorbedByDragon && releasePoint != null)
                {
                    GhostEvents.RaiseGhostCaptured(data.type, releasePoint.position);
                }
                Destroy(gameObject);
            }
               
        }

        return true;
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
        switch (data.type)
        {
            case GhostType.Normal:
                transform.localScale = Vector3.one * 1f;
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                break;

            case GhostType.Quick:
                transform.localScale = Vector3.one * 0.8f; 
                spriteRenderer.color = new Color(1f, 0.8f, 0.8f, 0.9f); 
                break;

            case GhostType.Tank:
                transform.localScale = Vector3.one * 1.4f; 
                spriteRenderer.color = new Color(0.6f, 0.6f, 1f, 1f); 
                break;

            case GhostType.Suicide:
                transform.localScale = Vector3.one * 1.2f;
                spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 0.8f); 
                break;

            case GhostType.Lucky:
                transform.localScale = Vector3.one * 1.1f;
                spriteRenderer.color = new Color(1f, 1f, 0.6f, 1f); 
                break;
        }
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

        transform.position = Vector3.MoveTowards(
            transform.position,
            suicideTargetPos,
            data.walkSpeed * 2f * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, suicideTargetPos) < 0.3f)
        {
            suicideTargetSet = false;
            Kill();
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
        absorbCooldown -= Time.deltaTime;
        if (absorbCooldown > 0f) return;
        if (targetBall == null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, absorbRadius);
            float nearestDist = float.MaxValue;
            BallController nearestBall = null;

            foreach (var hit in hits)
            {
                BallController ball = hit.GetComponent<BallController>();
                if (ball != null)
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
                Debug.Log($"🎯 Tank {name} found target ball {targetBall.name}");
            }
        }

        if (targetBall != null)
        {
            Vector3 targetPos = targetBall.transform.position;
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                data.walkSpeed * Time.deltaTime
            );

            Vector2 dir = (targetPos - transform.position).normalized;
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
            animator.SetFloat("Speed", 1f);

            float distToBall = Vector2.Distance(transform.position, targetPos);
            if (distToBall < 0.4f)
            {
                Debug.Log($"💪 Tank {name} absorbed ball {targetBall.name}");
                Destroy(targetBall.gameObject);

               

                absorbCooldown = 5f;
                targetBall = null; 
            }
        }
    }
    public IEnumerator MoveToPointAndFreeze(Vector3 targetPos, float speed)
    {
        if (rb == null) yield break;

        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            if (this == null || gameObject == null)
                yield break;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        rb.simulated = false;
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
    public void Kill()
    {
        if (isDead) return;
        isDead = true;
        animator.SetBool("Dead", true);

        if (data.type == GhostType.Suicide && data.fireCirclePrefab != null)
        {
            var circle = Instantiate(data.fireCirclePrefab, transform.position, Quaternion.identity);
            Destroy(circle, data.fireCircleLifetime);
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
}
