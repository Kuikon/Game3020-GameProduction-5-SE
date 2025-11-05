using System.Collections;
using UnityEngine;

public class DroppedBall : BallBase
{
    [Header("Float Settings")]
    public float floatSpeed = 1.5f;
    public float floatAmplitude = 0.2f;
    private Vector3 startPos;
    private bool isCollected = false;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (!isActive) return;
        // 🌀 上下に浮く
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;

            // 🎯 Lucky弾はNormal弾として扱う
            GhostType addType = (type == GhostType.Lucky) ? GhostType.Normal : type;

            GameManager gm = GameObject.FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                if (!gm.capturedGhosts.ContainsKey(addType))
                    gm.capturedGhosts[addType] = 0;
                gm.capturedGhosts[addType]++;
            }

            if (UIManager.Instance != null)
            {
                int count = gm != null ? gm.capturedGhosts[addType] : 1;
                UIManager.Instance.UpdateSlot(addType, count);
            }

            StartCoroutine(CollectEffect());
        }
    }
    private IEnumerator CollectEffect()
    {
        isCollected = true;

        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null)
            ps.Play();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 start = transform.position;
            Vector3 target = player.transform.position;
            float duration = 0.3f;
            float elapsed = 0f;

            // 👇 元のスケールを記録
            Vector3 originalScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, target, elapsed / duration);

                // 💫 元のスケールを基準に縮小
                float scaleFactor = Mathf.Lerp(1f, 0.2f, elapsed / duration);
                transform.localScale = originalScale * scaleFactor;

                yield return null;
            }
        }

        // 💥 消滅前のフェードアウト
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float fadeTime = 0.2f;
            float t = 0;
            Color c = sr.color;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / fadeTime);
                sr.color = c;
                yield return null;
            }
        }

        Destroy(gameObject);
    }


}
