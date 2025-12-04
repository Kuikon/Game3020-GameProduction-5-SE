using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    public float moveSpeed = 3f;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] public LineRenderer aimLine;
    [SerializeField] private Camera mainCam;
    [SerializeField] private LayerMask unwalkableLayer; 
    [SerializeField] private float checkRadius = 0.2f;  
    private bool isInvincible = false;
    private bool isMoving;
    public bool CanMove { get; set; } = true;
    [SerializeField] private float invincibleDuration = 2f;
    [SerializeField] private float flashInterval = 0.1f;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private int currentTypeIndex = 0;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            // ★ 死亡時イベント受信登録
            playerHealth.OnPlayerDeath -= HandlePlayerDeath;
            playerHealth.OnPlayerDeath += HandlePlayerDeath;
        }
    }
    void Start()
    {
        MiniMapManager.Instance?.RegisterPlayer(gameObject);
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        if (mainCam == null)
            mainCam = Camera.main;

        if (aimLine != null)
        {
            aimLine.enabled = false;
            aimLine.startWidth = 0.02f;
            aimLine.endWidth = 0.02f;
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (!CanMove) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        // ① 入力ベクトル（アニメ用）
        Vector2 input;
        input.x = UnityEngine.Input.GetAxisRaw("Horizontal");
        input.y = UnityEngine.Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(input.x) < 0.1f) input.x = 0f;
        if (Mathf.Abs(input.y) < 0.1f) input.y = 0f;

        // ② 実際に動かすベクトル（物理用）
        Vector2 moveDir = input;

        if (moveDir.magnitude > 1f)
            moveDir.Normalize();

        // 壁チェック用の仮ターゲット位置
        Vector2 targetPos = (Vector2)transform.position + moveDir * moveSpeed * Time.fixedDeltaTime;

        // 壁に当たるなら「移動だけ」止める（入力は残す）
        if (!IsWalkableTile(targetPos))
        {
            moveDir = Vector2.zero;
        }

        // ③ アニメーションは「入力」を渡す
        UpdateAnimation(input);

        // ④ 物理移動は「moveDir」を使う
        rb.linearVelocity = moveDir * moveSpeed;

        // スプライト反転も入力ベース
        FlipSprite(input.x);
    }

    private void UpdateAnimation(Vector2 inputDir)
    {
        bool isMoving = Mathf.Abs(inputDir.x) > 0.1f || Mathf.Abs(inputDir.y) > 0.1f;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            lastMoveDir = inputDir;
        }

        Vector2 animDir = isMoving ? inputDir : lastMoveDir;
        animator.SetFloat("moveX", animDir.x);
        animator.SetFloat("moveY", animDir.y);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Dog"))
        {
            if (playerHealth != null)
            {
                SoundManager.Instance.PlaySE(SESoundData.SE.TakeDamage);
                playerHealth.TakeDamage(1, collision.transform);
                StartCoroutine(ActivateInvincibility());
            }
        }
    }
    private void HandlePlayerDeath(Transform attacker)
    {
        StopImmediately();
        CanMove = false;
        StartCoroutine(Co_SuckIntoGhost(attacker));
    }

    // ★ 吸い込みアニメーション
    private IEnumerator Co_SuckIntoGhost(Transform ghost)
    {
        Vector3 start = transform.position;
        Vector3 end = ghost != null ? ghost.position : start;

        float duration = 1.2f;
        float t = 0f;

        Vector3 originalScale = transform.localScale;

        // アニメ停止
        animator.SetBool("isMoving", false);
        playerCollider.enabled = false;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;

            transform.position = Vector3.Lerp(start, end, p);
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, p);

            yield return null;
        }

        // 完全に消える
        transform.localScale = Vector3.zero;

        // GAME OVER
        GameManager.Instance.GameOver();
    }

    private IEnumerator ActivateInvincibility()
    {
        isInvincible = true;

        float elapsed = 0f;
        bool visible = true;
        if (playerCollider != null)
            playerCollider.enabled = false;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
        while (elapsed < invincibleDuration)
        {
            elapsed += flashInterval;
            visible = !visible;

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = visible ? 1f : 0.3f; 
                spriteRenderer.color = c;
            }

            yield return new WaitForSeconds(flashInterval);
        }
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
        if (playerCollider != null)
            playerCollider.enabled = true;

        isInvincible = false;
    }

    public void StopImmediately()
    {
        if (this != null)
        {
            StopAllCoroutines();
        }
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        isMoving = false;
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
        }
    }

    private bool IsWalkableTile(Vector2 checkPos)
    {
        Collider2D hit = Physics2D.OverlapCircle(checkPos, checkRadius, unwalkableLayer);
        return hit == null;
    }
    private void FlipSprite(float moveX)
    {
        if (Mathf.Abs(moveX) > 0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.x = (moveX > 0) ? -1 : 1;
            transform.localScale = scale;
        }
    }
    private GhostType[] allTypes =
    {
        GhostType.Normal,
        GhostType.Quick,
        GhostType.Suicide,
        GhostType.Tank,
    };
}
