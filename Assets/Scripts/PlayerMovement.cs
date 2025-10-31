using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    public float moveSpeed = 3f;
    [SerializeField] private PlayerHealth playerHealth;
    void Start()
    {
        MiniMapManager.Instance?.RegisterPlayer(gameObject);
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(moveInput.x) < 0.1f) moveInput.x = 0f;
        if (Mathf.Abs(moveInput.y) < 0.1f) moveInput.y = 0f;
        if (moveInput.magnitude > 1f)
            moveInput.Normalize();
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
            }
        }
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
}
