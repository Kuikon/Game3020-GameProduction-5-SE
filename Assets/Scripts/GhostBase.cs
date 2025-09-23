using UnityEngine;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class GhostBase : MonoBehaviour
{
    public GhostData data;
     private　Transform capturePoint;
    [SerializeField] private float captureSpeed = 2f;
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
        var obj = GameObject.Find("CapturePoint");
        if (obj != null)
            capturePoint = obj.transform;
        lifeTimer = data.absorbTime;
        PickRandomDirection();
    }

    void Update()
    {
        if (isDead)
        {
            if (capturePoint != null)
            {
                // ゆるやかに吸い込まれる (Lerp)
                transform.position = Vector3.Lerp(
                    transform.position,
                    capturePoint.position,
                    captureSpeed * Time.deltaTime
                );

                // capturePoint に十分近づいたら削除
                float distance = Vector3.Distance(transform.position, capturePoint.position);
                if (distance < 1f) // 閾値は調整可
                {
                    Destroy(gameObject);
                }
            }
            return; // 捕まったら通常処理はしない
        }

        // ↓ここからは通常の生きてるときの処理
        dirTimer -= Time.deltaTime;
        if (dirTimer <= 0f)
            PickRandomDirection();

        rb.linearVelocity = moveDir * data.walkSpeed;

        animator.SetFloat("MoveX", moveDir.x);
        animator.SetFloat("MoveY", moveDir.y);
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f && (data.type == GhostType.Normal || data.type == GhostType.Quick || data.type == GhostType.Tank))
        {
            GameObject dragon = GameObject.Find("Dragon");
            if (dragon != null)
            {
                Absorb(dragon.transform);
            }
        }

        switch (data.type)
        {
            case GhostType.Quick:
                if (lifeTimer <= data.absorbTime * 0.5f) Kill();
                break;
            case GhostType.Tank:
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
    }
    public void Absorb(Transform absorbPoint)
    {
        if (isDead) return;
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        animator.SetBool("Dead", true);

        capturePoint = absorbPoint;
    }
    private void PickRandomDirection()
    {
        int x = Random.Range(-1, 2);
        int y = Random.Range(-1, 2);
        moveDir = new Vector2(x, y).normalized;
        dirTimer = data.changeDirectionTime;
    }
}
