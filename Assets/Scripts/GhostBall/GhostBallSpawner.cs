using UnityEngine;

public class GhostBallSpawner : MonoBehaviour
{
    public static GhostBallSpawner Instance { get; private set; }
    [SerializeField] private GameObject droppedBallPrefab; // 👈 DroppedBallをここに設定

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnBulletByType(GhostType type, Vector3 position)
    {
        if (droppedBallPrefab == null)
        {
            Debug.LogWarning("DroppedBall prefab not assigned!");
            return;
        }

        GameObject ball = Instantiate(droppedBallPrefab, position, Quaternion.identity);

        // BallBaseにタイプ設定
        var ballBase = ball.GetComponent<BallBase>();
        if (ballBase != null)
            ballBase.type = type;

        // 色も反映
        SpriteRenderer sr = ball.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = GhostBase.GetColorByType(type);

        Debug.Log($"💧 Spawned DroppedBall of type {type} at {position}");
    }
}
