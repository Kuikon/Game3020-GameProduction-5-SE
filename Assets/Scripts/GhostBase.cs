using UnityEngine;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class GhostBase : MonoBehaviour
{
    public GhostData data;

    private bool isDead;
    private Vector2 moveDir;
    private float dirTimer;
    private float lifeTimer;
    private float chargeTimer; // Lucky 用
    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        lifeTimer = data.absorbTime;
        PickRandomDirection();
    }

    void Update()
    {
        if (isDead) return;

        // 共通処理（移動）
        dirTimer -= Time.deltaTime;
        if (dirTimer <= 0f)
            PickRandomDirection();

        rb.linearVelocity = moveDir * data.walkSpeed;

        animator.SetFloat("MoveX", moveDir.x);
        animator.SetFloat("MoveY", moveDir.y);
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);

        // 共通：自然吸収
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f && (data.type == GhostType.Normal || data.type == GhostType.Quick || data.type == GhostType.Tank))
        {
            Kill();
        }

        // 特殊挙動
        switch (data.type)
        {
            case GhostType.Quick:
                // 吸収時間を短縮
                if (lifeTimer <= data.absorbTime * 0.5f)
                    Kill();
                break;

            case GhostType.Tank:
                // Tankは absorbTime が長い or captureHitsNeeded を増やすで対応
                break;

            case GhostType.Suicide:
                UpdateSuicide();
                break;

            case GhostType.Lucky:
                UpdateLucky();
                break;
        }
    }

    private void UpdateSuicide()
    {
        Vector3 target = transform.position;
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            target = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
        }
#else
        if (Input.GetMouseButton(0))
        {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
#endif
        target.z = 0f;
        Vector2 dir = (target - transform.position).normalized;
        rb.linearVelocity = dir * (data.walkSpeed * 2f);
    }

    private void UpdateLucky()
    {
        chargeTimer += Time.deltaTime;
        if (chargeTimer >= data.luckyChargeTime)
        {
            CaptureAll();
            chargeTimer = 0f;
        }
    }

    private void CaptureAll()
    {
        GhostBase[] all = FindObjectsByType<GhostBase>(FindObjectsSortMode.None);
        foreach (GhostBase g in all)
        {
            if (g != this) g.Kill();
        }
        Debug.Log("Lucky Ghost が全ゴーストを吸収！");
    }

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

        Destroy(gameObject);
    }

    private void PickRandomDirection()
    {
        int x = Random.Range(-1, 2);
        int y = Random.Range(-1, 2);
        moveDir = new Vector2(x, y).normalized;
        dirTimer = data.changeDirectionTime;
    }
}
