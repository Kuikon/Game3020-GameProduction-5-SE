using UnityEngine;

public class GhostBallSpawner : MonoBehaviour
{
    [Header("Ball Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private int ballCount = 8;
    [SerializeField] private float launchForce = 5f;

    private void OnEnable()
    {
        GhostEvents.OnGhostCaptured += OnGhostCaptured;
    }

    private void OnDisable()
    {
        GhostEvents.OnGhostCaptured -= OnGhostCaptured;
    }

    private void OnGhostCaptured(GhostType type, Vector3 position)
    {
        if (ballPrefab == null)
        {
            Debug.LogWarning("Ball prefab not assigned!");
            return;
        }

        // 🎯 Create a single ball
        GameObject ball = Instantiate(ballPrefab, position, Quaternion.identity);

        BallController bc = ball.GetComponent<BallController>();
        if (bc != null)
        {
            bc.type = type;
        }

        // 🎨 Optional: change color based on type
        SpriteRenderer sr = ball.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = GetColorByType(type);
        }

        // 💥 Launch in a random direction (8-way)
        int randomIndex = Random.Range(0, ballCount);
        float angle = randomIndex * Mathf.PI * 2f / ballCount;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.AddForce(dir * launchForce, ForceMode2D.Impulse);

        if (bc != null)
            bc.initialDirection = dir;
    }

    private Color GetColorByType(GhostType type)
    {
        switch (type)
        {
            case GhostType.Normal: return Color.white;
            case GhostType.Quick: return new Color(1f, 0.7f, 0.7f);
            case GhostType.Tank: return new Color(0.6f, 0.6f, 1f);
            case GhostType.Suicide: return new Color(1f, 0.4f, 0.4f);
            case GhostType.Lucky: return new Color(1f, 1f, 0.6f);
            default: return Color.gray;
        }
    }
}
