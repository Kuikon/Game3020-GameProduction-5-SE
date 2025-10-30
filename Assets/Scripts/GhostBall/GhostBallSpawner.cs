using UnityEngine;

public class GhostBallSpawner : MonoBehaviour
{
    [Header("Ball Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private float launchForce = 5f;
    [SerializeField, Range(0f, 90f)]
    private float maxAngle = 15f;

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

        int direction = Random.value < 0.5f ? -1 : 1;
        float randomAngle = Random.Range(-maxAngle, maxAngle) * Mathf.Deg2Rad; 

        Vector2 dir = new Vector2(
            direction * Mathf.Cos(randomAngle),
            Mathf.Sin(randomAngle)
        ).normalized;

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
            case GhostType.Quick: return Color.blue;
            case GhostType.Tank: return new Color(0.6f, 0.6f, 1f);
            case GhostType.Suicide: return new Color(1f, 0.4f, 0.4f);
            case GhostType.Lucky: return new Color(1f, 1f, 0.6f);
            default: return Color.gray;
        }
    }
}
