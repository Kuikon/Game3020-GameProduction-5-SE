using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(moveInput.x) < 0.1f) moveInput.x = 0f;
        if (Mathf.Abs(moveInput.y) < 0.1f) moveInput.y = 0f;
        if (moveInput.magnitude > 1f)
            moveInput.Normalize();
        Vector2 targetPos = (Vector2)transform.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        if (!IsWalkableTile(targetPos))
        {
            moveInput = Vector2.zero;
        }
        UpdateAnimation();
        rb.linearVelocity = moveInput * moveSpeed;
        FlipSprite(moveInput.x);
    }
    private void UpdateAnimation()
    {
        bool isMoving = Mathf.Abs(moveInput.x) > 0.1f || Mathf.Abs(moveInput.y) > 0.1f;
        animator.SetBool("isMoving", isMoving);
        if (isMoving)
        {
            lastMoveDir = moveInput;
        }
        Vector2 animDir = isMoving ? moveInput : lastMoveDir;
        animator.SetFloat("moveX", animDir.x);
        animator.SetFloat("moveY", animDir.y);
    }
  

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Dog"))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
                StartCoroutine(ActivateInvincibility());
            }
        }
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
        StopAllCoroutines();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        isMoving = false;
        if (animator != null)
        {
            animator.SetFloat("moveX", 0);
            animator.SetFloat("moveY", 0);
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
