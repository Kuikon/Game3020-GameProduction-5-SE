using UnityEngine;

public class GhostBallSpawner : MonoBehaviour
{
    public static GhostBallSpawner Instance { get; private set; }

    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private float launchForce = 3f;
    [SerializeField] private float maxAngle = 20f;

    private void Awake()
    {
        Instance = this;
    }

    // 👇 新しい公開関数を追加
    public void SpawnBulletByType(GhostType type, Vector3 position)
    {
        if (ballPrefab == null)
        {
            Debug.LogWarning("Ball prefab not assigned!");
            return;
        }

        // 🎯 弾を生成
        GameObject ball = Instantiate(ballPrefab, position, Quaternion.identity);

        // タイプ設定
        BallController bc = ball.GetComponent<BallController>();
        if (bc != null)
            bc.type = type;

        // 色設定
        SpriteRenderer sr = ball.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = GetColorByType(type);

        // 軽く跳ね飛ばす
        int direction = Random.value < 0.5f ? -1 : 1;
        float randomAngle = Random.Range(-maxAngle, maxAngle) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(
            direction * Mathf.Cos(randomAngle),
            Mathf.Sin(randomAngle)
        ).normalized;

        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.AddForce(dir * launchForce, ForceMode2D.Impulse);
    }

    private Color GetColorByType(GhostType type)
    {
        switch (type)
        {
            case GhostType.Normal: return Color.white;
            case GhostType.Quick: return Color.cyan;
            case GhostType.Tank: return new Color(0.4f, 0.6f, 1f);
            case GhostType.Suicide: return new Color(1f, 0.5f, 0.5f);
            case GhostType.Lucky: return new Color(1f, 1f, 0.6f);
            default: return Color.gray;
        }
    }
}
