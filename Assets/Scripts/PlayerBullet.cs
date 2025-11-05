using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBullet : BallBase
{
    [Header("Bullet Settings")]
    public float speed = 10f;
    public float lifetime = 5f;
    private Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // 弾発射
    public void Launch(Vector2 direction)
    {
        if (rb != null)
            rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        // 🎯 箱に命中したら BoxManager に報告
        UIBox box = other.GetComponent<UIBox>();
        if (box != null)
        {
            BoxManager.Instance?.ProcessDrop(this, box);
            DestroyBall();
        }
    }
}
