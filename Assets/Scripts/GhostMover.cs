using UnityEngine;

public class RandomWalker : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;            // Normal walking speed
    public float changeDirectionTime = 2f;  // Interval to change movement direction (seconds)

    private Vector2 moveDir;
    private float timer;
    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        PickRandomDirection();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // Change direction every certain interval
        if (timer <= 0f)
        {
            PickRandomDirection();
        }

        float speed;

        // Idle state check
        if (moveDir == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            speed = 0f; // Idle
        }
        else
        {
            // Always move at walkSpeed
            rb.linearVelocity = moveDir * walkSpeed;
            speed = walkSpeed;
        }

        // ✅ Send values to BlendTree
        animator.SetFloat("MoveX", moveDir.x);
        animator.SetFloat("MoveY", moveDir.y);
        animator.SetFloat("Speed", speed);
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        // X-axis boundary check
        if (pos.x < -6f || pos.x > 6f)
        {
            moveDir.x = -moveDir.x; // Reverse direction
            rb.linearVelocity = new Vector2(moveDir.x * walkSpeed, rb.linearVelocity.y);

            pos.x = Mathf.Clamp(pos.x, -6f, 6f);
        }

        // Y-axis boundary check
        if (pos.y < -4f || pos.y > 4f)
        {
            moveDir.y = -moveDir.y; // Reverse direction
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, moveDir.y * walkSpeed);

            pos.y = Mathf.Clamp(pos.y, -4f, 4f);
        }

        transform.position = pos;
    }

    void PickRandomDirection()
    {
        // Randomly choose -1, 0, or 1 for direction
        int x = Random.Range(-1, 2);
        int y = Random.Range(-1, 2);

        moveDir = new Vector2(x, y).normalized;

        // Reset the timer for the next direction change
        timer = changeDirectionTime;
    }
}
