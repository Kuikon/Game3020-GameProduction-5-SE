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
        {
            bullet.Initialize(fireType);
            bullet.speed = bulletSpeed;
            bullet.Launch(direction);
        }

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
    private GhostType[] allTypes =
    {
        GhostType.Normal,
        GhostType.Quick,
        GhostType.Suicide,
        GhostType.Tank,
    };
}
