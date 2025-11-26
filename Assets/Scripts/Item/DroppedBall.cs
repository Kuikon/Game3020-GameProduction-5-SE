using System.Collections;
using UnityEngine;

public class DroppedBall : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatSpeed = 1.5f;
    public float floatAmplitude = 0.2f;
    private Vector3 startPos;
    private bool isCollected = false;
    private Transform collectTarget;
    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // 🌀 上下に浮く
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;

           

          
          

            StartCoroutine(CollectEffect());
        }
    }
    public void CollectTo(Transform target)
    {
        if (isCollected) return;
        isCollected = true;
        collectTarget = target;
        StartCoroutine(CollectEffectToGhost());
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
    private IEnumerator CollectEffectToGhost()
    {
        // 🔹 パーティクル再生（任意）
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null)
            ps.Play();

        Vector3 start = transform.position;
        Vector3 targetPos = collectTarget.position;
        float duration = 0.5f;   // 飛ぶ時間
        float elapsed = 0f;

        Vector3 originalScale = transform.localScale;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // 🔹 少しカーブしながら吸い込まれる感じ
        Vector3 midPoint = (start + targetPos) / 2f + Vector3.up * 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 💫 曲線的な移動（ベジエ補間）
            float t = elapsed / duration;
            Vector3 p1 = Vector3.Lerp(start, midPoint, t);
            Vector3 p2 = Vector3.Lerp(midPoint, targetPos, t);
            transform.position = Vector3.Lerp(p1, p2, t);

            // 📉 小さくなりながら吸い込まれる
            float scaleFactor = Mathf.Lerp(1f, 0.1f, t);
            transform.localScale = originalScale * scaleFactor;

            // 🌟 フェードアウト
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0.3f, t);
                sr.color = c;
            }

            yield return null;
        }

        // 🔹 最後に完全に消す
        if (sr != null)
        {
            float fadeTime = 0.15f;
            float f = 0;
            Color c = sr.color;
            while (f < fadeTime)
            {
                f += Time.deltaTime;
                c.a = Mathf.Lerp(0.3f, 0f, f / fadeTime);
                sr.color = c;
                yield return null;
            }
        }

        Destroy(gameObject);
    }

}
