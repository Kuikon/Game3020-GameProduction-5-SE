using System.Collections;
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
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private GhostType currentBulletType = GhostType.Normal;
    [SerializeField] public LineRenderer aimLine;
    [SerializeField] private Camera mainCam;
    [SerializeField] private LayerMask unwalkableLayer; // 🚫 歩けないタイルのレイヤー
    [SerializeField] private float checkRadius = 0.2f;   // 当たり判定の大きさ
    private bool isInvincible = false;
    [SerializeField] private float invincibleDuration = 2f;
    [SerializeField] private float flashInterval = 0.1f;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private int currentTypeIndex = 0;
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
        if (UIManager.Instance != null)
            UIManager.Instance.FocusBulletSlot(currentBulletType);
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        HandleMovement();
        HandleAimLine();
        HandleBulletSwitch();
    }
    private void HandleBulletSwitch()
    {
        // Qキーで左に、Eキーで右に切り替え
        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentTypeIndex--;
            if (currentTypeIndex < 0)
                currentTypeIndex = allTypes.Length - 1;
            SwitchBulletType();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            currentTypeIndex++;
            if (currentTypeIndex >= allTypes.Length)
                currentTypeIndex = 0;
            SwitchBulletType();
        }
    }

    private void SwitchBulletType()
    {
        currentBulletType = allTypes[currentTypeIndex];

        // 🎨 UIにフォーカスを反映
        if (UIManager.Instance != null)
            UIManager.Instance.FocusBulletSlot(currentBulletType);

        Debug.Log($"🔁 弾タイプ切り替え: {currentBulletType}");
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
    private void HandleAimLine()
    {
        if (aimLine == null) return;

        if (Input.GetMouseButton(1)) // 右クリック押している間
        {
            aimLine.enabled = true;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(mainCam.transform.position.z);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
            worldPos.z = 0f;

            aimLine.positionCount = 2;
            aimLine.SetPosition(0, transform.position);
            aimLine.SetPosition(1, worldPos);
            if (Input.GetMouseButtonDown(0))
            {
                ShootBullet(worldPos);
            }
        }
        else
        {
            aimLine.enabled = false;
        }
    }
    private void ShootBullet(Vector3 targetPos)
    {
        if (bulletPrefab == null) return;

        // LuckyタイプはNormalとして扱う
        GhostType fireType = (currentBulletType == GhostType.Lucky) ? GhostType.Normal : currentBulletType;

        // UIManagerの弾ストックチェック
        if (!UIManager.Instance.TryUseBullet(fireType))
        {
            Debug.Log($"⚠️ {fireType} 弾が足りません！");
            return;
        }

        Vector2 direction = (targetPos - transform.position).normalized;

        // 🟢 PlayerBullet生成
        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        PlayerBullet bullet = bulletObj.GetComponent<PlayerBullet>();
        if (bullet != null)
            bullet.Initialize(fireType, targetPos);

        // 見た目の色変更
        SpriteRenderer sr = bulletObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = GhostBase.GetColorByType(fireType);

        bulletObj.transform.up = direction;
    }


    public void ShootCurrentBullet()
    {
        if (mainCam == null || bulletPrefab == null) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCam.transform.position.z);
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;

        ShootBullet(worldPos);
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

        // 🚫 当たり判定をオフ
        if (playerCollider != null)
            playerCollider.enabled = false;

        // ❤️ 一瞬だけ赤く光る
        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.3f);

        // 🎨 赤から白に戻す
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        // ✨ 点滅フェーズ（白の半透明点滅）
        while (elapsed < invincibleDuration)
        {
            elapsed += flashInterval;
            visible = !visible;

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = visible ? 1f : 0.3f; // 薄くなるだけ
                spriteRenderer.color = c;
            }

            yield return new WaitForSeconds(flashInterval);
        }

        // ✅ 最後に完全に戻す
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        // 🟢 コライダー再有効化
        if (playerCollider != null)
            playerCollider.enabled = true;

        isInvincible = false;
    }



    private bool IsWalkableTile(Vector2 checkPos)
    {
        // 半径checkRadiusの円内に「歩けないレイヤー」があればfalse
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
