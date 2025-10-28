using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Lucky Control")]
    public int luckyScore = 0;
    public Button luckyButton;
    public Transform magnetPoint;
    public float magnetHoldTime = 3f;
    [Header("Type Gather Control")]
    public Button gatherButton;
    public float gatherSpeed = 3f;
    [Header("Ball Type Target Points")]
    public Transform normalPoint;
    public Transform quickPoint;
    public Transform tankPoint;
    public Transform suicidePoint;
    public Transform luckyPoint;

    private void Awake()
    {
        Instance = this;
        if (luckyButton != null)
            luckyButton.gameObject.SetActive(false);
        if (gatherButton != null)
        {
            gatherButton.onClick.RemoveAllListeners();
            gatherButton.onClick.AddListener(() =>
            {
                StartCoroutine(GatherAllBallsByType());
            });
        }
    }

    public void AddLuckyScore()
    {
        luckyScore++;
        Debug.Log($"🍀 Lucky Score: {luckyScore}");

        if (luckyScore >= 3 && luckyButton != null)
        {
            luckyButton.gameObject.SetActive(true);
            luckyButton.onClick.RemoveAllListeners();
            luckyButton.onClick.AddListener(() =>
            {
                StartCoroutine(GatherAllGhosts());
                luckyButton.gameObject.SetActive(false);
                luckyScore = 0;
            });
        }
    }

    private IEnumerator GatherAllGhosts()
    {
        if (magnetPoint == null)
        {
            Debug.LogWarning("⚠️ Magnet Point not assigned!");
            yield break;
        }

        GhostBase[] ghosts = FindObjectsByType<GhostBase>(FindObjectsSortMode.None);
        foreach (var ghost in ghosts)
        {
            if (ghost == null || ghost.isDead) continue;
            StartCoroutine(ghost.MoveToPointAndFreeze(magnetPoint.position, 3f));
        }

        yield return new WaitForSeconds(magnetHoldTime);

        foreach (var ghost in ghosts)
        {
            if (ghost != null && !ghost.isDead)
                ghost.ResumeMovement();
        }
    }
    private IEnumerator GatherAllBallsByType()
    {
        Debug.Log("⚾ Gathering all balls by GhostType...");

        BallController[] balls = FindObjectsByType<BallController>(FindObjectsSortMode.None);

        foreach (var ball in balls)
        {
            if (ball == null) continue;
            Collider2D col = ball.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;

            Transform target = GetTargetByType(ball.type);
            if (target != null)
            {
                StartCoroutine(MoveBallToTarget(ball, target.position, gatherSpeed));
            }
        }

        yield return new WaitForSeconds(magnetHoldTime);

        Debug.Log("✅ All balls reached their target zones!");
    }
    private IEnumerator MoveBallToTarget(BallController ball, Vector3 targetPos, float speed)
    {
        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        Collider2D col = ball.GetComponent<Collider2D>();

        if (rb == null) yield break;

        rb.simulated = false;
        ball.allowAutoCollision = true;
        while (Vector3.Distance(ball.transform.position, targetPos) > 0.05f)
        {
            ball.transform.position = Vector3.MoveTowards(ball.transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        rb.simulated = true;
        if (col != null)
            col.enabled = true; 
    }

    private Transform GetTargetByType(GhostType type)
    {
        switch (type)
        {
            case GhostType.Normal: return normalPoint;
            case GhostType.Quick: return quickPoint;
            case GhostType.Tank: return tankPoint;
            case GhostType.Suicide: return suicidePoint;
            case GhostType.Lucky: return luckyPoint;
            default: return null;
        }
    }
}
