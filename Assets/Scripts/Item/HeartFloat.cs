using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Heart : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatAmplitude = 0.2f; // 浮く上下幅
    [SerializeField] private float floatSpeed = 2f;       // 上下スピード
    [SerializeField] private float gravityStopTime = 1f;  // 浮遊に切り替わるまでの時間

    [Header("Recovery Settings")]
    [SerializeField] private int healAmount = 2;          // 回復量
    [SerializeField] private float fadeOutTime = 0.5f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector3 startPos;
    private float spawnTime;
    private bool canFloat = false;
    private bool collected = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        startPos = transform.position;
        spawnTime = Time.time;
        StartCoroutine(EnableFloatingAfterDelay());
    }

    private IEnumerator EnableFloatingAfterDelay()
    {
        yield return new WaitForSeconds(gravityStopTime);
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
        canFloat = true;
    }

    private void Update()
    {
        if (canFloat && !collected)
        {
            // 💫 ふわふわ浮遊
            float newY = startPos.y + Mathf.Sin((Time.time - spawnTime) * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log($"💓 Heart touched by Player: {other.name}");
            collected = true;
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log($"🧠 Player HP before: {playerHealth.currentHP}/{playerHealth.maxHP}");
                if (playerHealth.currentHP < playerHealth.maxHP)
                {
                    // 💖 HPを回復
                    playerHealth.Heal(healAmount);
                    Debug.Log($"❤️ Healed by {healAmount}! → HP: {playerHealth.currentHP}/{playerHealth.maxHP}");
                }
                else
                {
                    // 💎 HPが最大なら上限を増やす
                    playerHealth.IncreaseMaxHP(1);
                    Debug.Log($"💪 Max HP increased! → HP: {playerHealth.currentHP}/{playerHealth.maxHP}");
                }
            }

            StartCoroutine(FadeAndDestroy());
        }
    }

    private IEnumerator FadeAndDestroy()
    {
        float elapsed = 0f;
        Color c = sr.color;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
            sr.color = c;
            yield return null;
        }

    }
}
