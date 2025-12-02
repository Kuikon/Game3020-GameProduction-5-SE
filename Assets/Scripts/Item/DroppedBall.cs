using UnityEngine;
using System.Collections;

public class DroppedBall : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatSpeed = 1.2f;
    public float floatAmplitude = 0.15f;

    public int expAmount = 1;

    private bool isCollected = false;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        transform.position = startPos +
            Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("ExpMagnet"))
        {
            PlayerExp playerExp = other.GetComponentInParent<PlayerExp>();

            if (playerExp != null)
            {
                Debug.Log("🧲 ExpMagnet に当たったのでプレイヤーへ吸い込み開始");
                GameManager.Instance.OnGhostCaptured();
                CollectTo(playerExp.transform); // ← Transform を渡す
            }
            else
            {
                Debug.LogError("❌ ExpMagnet の親から PlayerExp が見つからない");
            }
        }
    }

    public void CollectTo(Transform target)
    {
        if (isCollected) return;
        isCollected = true;

        StartCoroutine(Co_CollectTo(target));
    }

    private IEnumerator Co_CollectTo(Transform target)
    {
        Vector3 start = transform.position;
        Vector3 end = target.position;

        float duration = 0.35f;
        float t = 0f;

        Vector3 originalScale = transform.localScale;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        while (t < duration)
        {
            if (target == null) break;

            t += Time.deltaTime;
            float p = t / duration;

            transform.position = Vector3.Lerp(start, end, p);
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.1f, p);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, p);
                sr.color = c;
            }

            yield return null;
        }

        // ⭐ ここを Tank 対応に！
        PlayerExp exp = target.GetComponent<PlayerExp>();

        if (exp != null)
        {
            Debug.Log($"⭐ AddExp({expAmount}) を呼ぶ！");
            exp.AddExp(expAmount);
        }
        else
        {
            Debug.Log($"🟡 Tank または別オブジェクトが吸ったので経験値は付与しません");
        }

        Destroy(gameObject);
    }
}
