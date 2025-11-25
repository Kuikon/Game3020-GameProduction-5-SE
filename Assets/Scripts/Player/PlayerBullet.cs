using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBullet : BallBase
{
    public GhostType Type { get; private set; }

    [Header("Bullet Settings")]
    [SerializeField] private float moveSpeed = 2f;           // ゆっくり移動速度
    [SerializeField] private float floatAmplitude = 0.2f;    // 上下のふわふわ幅
    [SerializeField] private float floatSpeed = 3f;          // ふわふわの速さ
    [SerializeField] private float stopDistance = 0.2f;      // 到達判定距離
    [SerializeField] private float lifetime = 8f;            // 自動消滅時間

    private Rigidbody2D rb;
    private Vector3 targetPos;
    private Vector3 startPos;
    private float spawnTime;
    private bool reached = false;
    // 🔹 強化システム
    private static int quickLevel = 0;
    private static int suicideLevel = 0;
    private static int tankLevel = 0;

    private static int quickProgress = 0;
    private static int suicideProgress = 0;
    private static int tankProgress = 0;

    // 各段階に必要な弾数
    private static readonly int[] levelThresholds = { 1, 2, 40, 80, 160 };

    protected override void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb.gravityScale = 0f;
        spawnTime = Time.time;
        startPos = transform.position;
        Destroy(gameObject, lifetime); // 時間切れで削除
    }

    private void Update()
    {
        if (reached) return;

        // 💫 ゆっくりターゲットに向かう
        Vector3 dir = (targetPos - transform.position).normalized;
        float step = moveSpeed * Time.deltaTime;

        transform.position += dir * step;

        // 🌬️ ふわふわ上下移動（sin波）
        float floatY = Mathf.Sin((Time.time - spawnTime) * floatSpeed) * floatAmplitude;
        transform.position += new Vector3(0, floatY * Time.deltaTime, 0);

        // 🎯 到達判定
        if (Vector3.Distance(transform.position, targetPos) < stopDistance)
        {
            reached = true;
            StartCoroutine(IdleFadeAndDestroy());
        }
    }
    private IEnumerator IdleFadeAndDestroy()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float t = 0f;
        Color c = sr.color;

        while (t < 1f)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t);
            sr.color = c;
            yield return null;
        }

        Destroy(gameObject);
    }
    public void Initialize(GhostType type, Vector3 mouseWorldPos)
    {
        Type = type;
        targetPos = mouseWorldPos;
        ApplyTypeEffect();
    }


    // =========================================================
    // 💡 タイプごとの特殊効果 + 強化システム
    // =========================================================
    private void ApplyTypeEffect()
    {
        switch (Type)
        {
            case GhostType.Quick:
                var lightCtrl = FindAnyObjectByType<Light2DRadiusController>();
                if (lightCtrl != null)
                {
                    float baseDuration = 1f;
                    float reduction = Mathf.Pow(quickLevel, 2f) * 0.3f;
                    lightCtrl.flashDuration = Mathf.Max(7f, baseDuration + reduction);
                    Debug.Log($"⚡ QUICK Lv.{quickLevel} → flashDuration={lightCtrl.flashDuration:F2}");
                }
                break;

            case GhostType.Suicide:
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null)
                {
                    float baseSpeed = 2f;
                    float bonus = (Mathf.Pow(1.35f, suicideLevel) - 1f) * 3f;
                    player.moveSpeed = Mathf.Min(6f, baseSpeed + bonus);

                    Debug.Log($"💀 SUICIDE Lv.{suicideLevel} → moveSpeed={player.moveSpeed:F1}");
                }
                break;

            case GhostType.Tank:
                var lineDraw = FindAnyObjectByType<LineDraw>();
                if (lineDraw != null)
                {
                    float baseLength = 8f;

                    // 📈 緩やかに始まり、Lvが上がるほど勢いよく伸びる
                    float bonus = Mathf.Pow(tankLevel, 2f) * 0.35f;
                    float newLength = Mathf.Min(30f, baseLength + bonus * 2f);
                    //lineDraw.MaxLineLength = Mathf.Min(30f, baseLength + bonus * 2f);

                    Debug.Log($"🛡️ TANK Lv.{tankLevel} → maxLineLength={newLength:F1}");
                }
                break;
        }
    }

    // =========================================================
    // 🎯 命中時：強化段階の進行
    // =========================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<UIBox>(out var box))
        {
            if (box.boxType == Type)
            {
                box.AddBall();
                Debug.Log($"✅ {Type} タイプ命中");

                // 命中で進行度を加算
                AddProgress(Type);

                Destroy(gameObject);
            }
        }
    }

    // =========================================================
    // 🔼 強化段階の進行管理
    // =========================================================
    private void AddProgress(GhostType type)
    {
        switch (type)
        {
            case GhostType.Quick:
                quickProgress++;
                if (quickLevel < 5 && quickProgress >= levelThresholds[quickLevel])
                {
                    quickLevel++;
                    quickProgress = 0;
                    Debug.Log($"⚡ QUICKがLv{quickLevel}にアップ！");
                    UIManager.Instance.UpdateBar("QuickBar", quickLevel);
                }
                ApplyTypeEffect();
                break;

            case GhostType.Suicide:
                suicideProgress++;
                if (suicideLevel < 5 && suicideProgress >= levelThresholds[suicideLevel])
                {
                    suicideLevel++;
                    suicideProgress = 0;
                    Debug.Log($"💀 SUICIDEがLv{suicideLevel}にアップ！");
                    UIManager.Instance.UpdateBar("SuicideBar", suicideLevel);
                }
                ApplyTypeEffect();
                break;

            case GhostType.Tank:
                tankProgress++;
                if (tankLevel < 5 && tankProgress >= levelThresholds[tankLevel])
                {
                    tankLevel++;
                    tankProgress = 0;
                    Debug.Log($"🛡️ TANKがLv{tankLevel}にアップ！");
                    UIManager.Instance.UpdateBar("TankBar", tankLevel);
                }
                ApplyTypeEffect();
                break;
        }
    }
}
