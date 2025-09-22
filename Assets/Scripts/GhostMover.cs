using UnityEngine;

public class RandomWalker : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;      // 通常の歩き速度
    public float dashSpeed = 4f;      // ダッシュ速度
    public float changeDirectionTime = 2f; // ランダム移動の方向を変える間隔（秒）

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

        // 一定時間ごとにランダムな方向を選ぶ
        if (timer <= 0f)
        {
            PickRandomDirection();
        }

        float speed;

        // Idle 判定
        if (moveDir == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            speed = 0f; // Idle用
        }
        else
        {
            // ランダムに「歩き」か「ダッシュ」速度を選ぶ
            if (Random.value > 0.7f) // 30%の確率でダッシュ
            {
                rb.linearVelocity = moveDir * dashSpeed;
                speed = dashSpeed;
            }
            else
            {
                rb.linearVelocity = moveDir * walkSpeed;
                speed = walkSpeed;
            }
        }

        // ✅ BlendTree に値を渡す
        animator.SetFloat("MoveX", moveDir.x);
        animator.SetFloat("MoveY", moveDir.y);
        animator.SetFloat("Speed", speed);
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        // X方向チェック
        if (pos.x < -6f || pos.x > 6f)
        {
            moveDir.x = -moveDir.x;                // 向きを反転
            rb.linearVelocity = new Vector2(moveDir.x, rb.linearVelocity.y);

            // はみ出し補正
            pos.x = Mathf.Clamp(pos.x, -6f, 6f);
        }

        // Y方向チェック
        if (pos.y < -4f || pos.y > 4f)
        {
            moveDir.y = -moveDir.y;                // 向きを反転
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, moveDir.y);

            // はみ出し補正
            pos.y = Mathf.Clamp(pos.y, -4f, 4f);
        }

        transform.position = pos;
    }


    void PickRandomDirection()
    {
        // -1, 0, 1 の中からランダムに方向を決める
        int x = Random.Range(-1, 2);
        int y = Random.Range(-1, 2);

        moveDir = new Vector2(x, y).normalized;

        // 次に方向を変えるまでの時間をリセット
        timer = changeDirectionTime;
    }
}
