using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class BallController : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color ballColor = Color.white;  // ボールの色（共通）

    [Header("Float Settings")]
    public float floatAmplitude = 0.2f;
    public float floatSpeed = 2f;

    [Header("Glow Settings")]
    public float blinkSpeed = 3f;
    public float blinkIntensity = 0.4f;

    [Header("Collect Effect Settings")]
    public float fadeSpeed = 2f;
    public float shrinkSpeed = 2f;

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

        // 色
        sr.color = ballColor;
        baseColor = sr.color;

        // Rigidbody無効化
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.gravityScale = 0f;
        }

        // Triggerに設定
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (isCollected) return;

        // ふわふわ
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed + timeOffset) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        // 点滅
        float blink = (Mathf.Sin(Time.time * blinkSpeed + timeOffset) * 0.5f + 0.5f) * blinkIntensity;
        float alpha = Mathf.Lerp(0.5f, 1f, blink);
        sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;
        if (!other.CompareTag("Player")) return;

        isCollected = true;

        // 🎯 共通弾 +1
        UIManager.Instance?.AddCommonBullet();

        // 回収エフェクト
        StartCoroutine(CollectEffect());
    }

    private IEnumerator CollectEffect()
    {
        float t = 0f;
        Vector3 originalScale = transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;

            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f - t);
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t * shrinkSpeed);

            yield return null;
        }

        Destroy(gameObject);
    }
}
