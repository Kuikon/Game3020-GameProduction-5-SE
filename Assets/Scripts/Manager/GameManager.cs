using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Lucky Control")]
    public int luckyScore = 0;
    public Button luckyButton;
    public Transform magnetPoint;
    public float magnetHoldTime = 3f;
    [Header("Type Gather Control")]
    public Button sortButton;
    public float sortSpeed = 3f;
    [Header("Ball Type Target Points")]
    public Transform normalPoint;
    public Transform quickPoint;
    public Transform tankPoint;
    public Transform suicidePoint;
    [Header("Game Stats")]
    public Dictionary<GhostType, int> capturedGhosts = new();
    public int damageTaken = 0;
    public int sortFails = 0;
    private bool isGameOver = false;
    [SerializeField] private List<GhostData> allGhostData;
    private Dictionary<GhostType, GhostData> ghostDataDict = new();

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (luckyButton != null)
            luckyButton.gameObject.SetActive(false);
        if (sortButton != null)
        {
            sortButton.onClick.RemoveAllListeners();
            sortButton.onClick.AddListener(() =>
            {
                //StartCoroutine(GatherAllBallsByType());
            });
        }
        foreach (GhostType type in System.Enum.GetValues(typeof(GhostType)))
        {
            capturedGhosts[type] = 0;
        }
        foreach (var data in allGhostData)
        {
            if (data != null && !ghostDataDict.ContainsKey(data.type))
                ghostDataDict.Add(data.type, data);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    public void RegisterGhostCapture(GhostType type)
    {
        capturedGhosts[type]++;
    }

    public void RegisterDamage()
    {
        damageTaken++;
    }

    public void RegisterSortFail()
    {
        sortFails++;
    }
    public Color GetGhostColor(GhostType type)
    {
        if (ghostDataDict.TryGetValue(type, out var data))
            return data.ghostColor;
        return Color.gray;
    }
    public GhostData GetGhostData(GhostType type)
    {
        if (ghostDataDict.TryGetValue(type, out var data))
            return data;
        return null;
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
            if (ghost.gameObject == null) continue;
            StartCoroutine(ghost.MoveToPointAndFreeze(magnetPoint.position, 3f));
            yield return new WaitForSeconds(0.02f);
        }

        yield return new WaitForSeconds(magnetHoldTime);

        foreach (var ghost in ghosts)
        {
            if (ghost != null && ghost.gameObject != null && !ghost.isDead)
                ghost.ResumeMovement();
        }
    }
    //private IEnumerator GatherAllBallsByType()
    //{
    //    Debug.Log("⚾ Gathering all balls by GhostType...");

    //    BallController[] balls = FindObjectsByType<BallController>(FindObjectsSortMode.None);

    //    foreach (var ball in balls)
    //    {
    //        if (ball == null) continue;
    //        Collider2D col = ball.GetComponent<Collider2D>();
    //        if (col != null)
    //            col.enabled = true;

    //        Transform target = GetTargetByType(ball.type);
    //        if (target != null)
    //        {
    //            StartCoroutine(MoveBallToTarget(ball, target.position, sortSpeed));
    //        }
    //    }

    //    yield return new WaitForSeconds(magnetHoldTime);

    //    Debug.Log("✅ All balls reached their target zones!");
    //}
    //private IEnumerator MoveBallToTarget(BallController ball, Vector3 targetPos, float speed)
    //{
    //    Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
    //    Collider2D col = ball.GetComponent<Collider2D>();

    //    if (rb == null) yield break;

    //    rb.simulated = false;
    //    while (Vector3.Distance(ball.transform.position, targetPos) > 0.05f)
    //    {
    //        ball.transform.position = Vector3.MoveTowards(ball.transform.position, targetPos, speed * Time.deltaTime);
    //        yield return null;
    //    }

    //    rb.simulated = true;
    //    if (col != null)
    //        col.enabled = true; 
    //}

    private Transform GetTargetByType(GhostType type)
    {
        switch (type)
        {
            case GhostType.Normal: return normalPoint;
            case GhostType.Quick: return quickPoint;
            case GhostType.Tank: return tankPoint;
            case GhostType.Suicide: return suicidePoint;
            default: return null;
        }
    }
    public void GameOver()
    {
        if (isGameOver) return; 
        isGameOver = true;
        StartCoroutine(LoadGameOverScene());
    }
    public void Victory()
    {
        if (isGameOver) return;
        isGameOver = true;
        SceneManager.LoadScene("VictoryScene");
    }
    private System.Collections.IEnumerator LoadGameOverScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("GameOverScene");
    }
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        UIManager.Instance.ShowPlayerStatus(isPaused);
    }
}
