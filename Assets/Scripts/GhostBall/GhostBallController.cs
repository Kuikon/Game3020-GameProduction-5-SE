using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class BallController : MonoBehaviour
{
    [Header("Ball Type Settings")]
    public GhostType type;

    [Header("Float Settings")]
    public float floatAmplitude = 0.2f;  // 上下の揺れ幅
    public float floatSpeed = 2f;        // 上下のスピード

    [Header("Glow (Blink) Settings")]
    public float blinkSpeed = 3f;        // 点滅スピード（大きいほど速く点滅）
    public float blinkIntensity = 0.4f;  // 明滅の強さ（0〜1）

    [Header("Collect Effect Settings")]
    public float fadeSpeed = 2f;         // フェードアウト速度
    public float shrinkSpeed = 2f;       // 縮小速度

    private Vector3 startPos;
    private float timeOffset;
    private bool isCollected = false;
    private SpriteRenderer sr;
    private Color baseColor;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startPos = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);

        // Rigidbodyを無効化
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.gravityScale = 0f;
        }

        // Triggerでプレイヤーが触れるように
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        ApplyTypeStyle();
        baseColor = sr.color; // 元の色を記録
    }

    private void Update()
    {
        if (isCollected) return;

        // 🎈 上下ふわふわ（sin波）
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed + timeOffset) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        // ✨ 点滅（sin波で透明度を変化）
        float blink = (Mathf.Sin(Time.time * blinkSpeed + timeOffset) * 0.5f + 0.5f) * blinkIntensity;
        float alpha = Mathf.Lerp(0.5f, 1f, blink); // αを0.5〜1で変化
        sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;

            // 🎯 GameManager と UIManager をシーンから探す（Persistent前提なし）
            GameManager gm = GameObject.FindFirstObjectByType<GameManager>();
            UIManager ui = GameObject.FindFirstObjectByType<UIManager>();

            if (gm != null)
            {
                // カウント更新
                if (!gm.capturedGhosts.ContainsKey(type))
                    gm.capturedGhosts[type] = 0;
                gm.capturedGhosts[type]++;
                Debug.Log($"📈 Count for {type}: {gm.capturedGhosts[type]}");
            }

            if (ui != null && gm != null)
            {
                int count = gm.capturedGhosts[type];
                Debug.Log($"🧮 Updating UI: type={type}, count={count}");
                ui.UpdateSlot(type, count);
            }

            // 💥 回収エフェクト
            StartCoroutine(CollectEffect());
        }
    }


    private IEnumerator CollectEffect()
    {
        float t = 0f;
        Vector3 originalScale = transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;

            if (sr != null)
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f - t);

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t * shrinkSpeed);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void ApplyTypeStyle()
    {
        if (sr == null) return;

        switch (type)
        {
            case GhostType.Normal:
                sr.color = Color.white;
                floatAmplitude = 0.2f;
                floatSpeed = 2f;
                blinkSpeed = 3f;
                break;

            case GhostType.Quick:
                sr.color = Color.cyan;
                floatAmplitude = 0.15f;
                floatSpeed = 3.5f;
                blinkSpeed = 6f;
                break;

            case GhostType.Tank:
                sr.color = new Color(0.4f, 0.6f, 1f);
                floatAmplitude = 0.3f;
                floatSpeed = 1.2f;
                blinkSpeed = 2f;
                break;

            case GhostType.Suicide:
                sr.color = new Color(1f, 0.5f, 0.5f);
                floatAmplitude = 0.25f;
                floatSpeed = 2.8f;
                blinkSpeed = 4f;
                break;

            case GhostType.Lucky:
                sr.color = new Color(1f, 1f, 0.6f);
                floatAmplitude = 0.25f;
                floatSpeed = 2.5f;
                blinkSpeed = 8f;
                break;
        }
    }
}
