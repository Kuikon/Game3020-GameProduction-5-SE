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
    [Header("Game Stats")]
    public Dictionary<GhostType, int> capturedGhosts = new();
    public int damageTaken = 0;
    public int sortFails = 0;
    private bool isGameOver = false;
    [SerializeField] private List<GhostData> allGhostData;
    private Dictionary<GhostType, GhostData> ghostDataDict = new();
    public int capturedGhostCount = 0;
    [SerializeField] private int thresholdToStartSpawn = 10;

    [SerializeField] private EnemySpawner enemySpawner;
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
    public void DisablePlayerControl()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.CanMove = false;
                controller.StopImmediately();
            }
            var col = player.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }
        if (LineDraw.Instance != null)
            LineDraw.Instance.enabled = false;

        Debug.Log("🛑 Player + LineDraw Stopped");
    }

    public void EnablePlayerControl()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
                controller.CanMove = true;

            var col = player.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;
        }

        if (LineDraw.Instance != null)
            LineDraw.Instance.enabled = true;

        Debug.Log("▶ Player + LineDraw Resumed");
    }

    public void GameOver()
    {
        if (isGameOver) return; 
        isGameOver = true;
        StartCoroutine(LoadGameOverScene());
    }
    public void Victory()
    {
        if (isGameOver)
        {
            Debug.Log("Victory() blocked. isGameOver = true");
            return;
        }
        Debug.Log("Victory() OK! Loading scene...");

        isGameOver = false;
        DisablePlayerControl();
        SceneManager.LoadScene("VictoryScene");
    }
    private System.Collections.IEnumerator LoadGameOverScene()
    {
        DisablePlayerControl();
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("GameOverScene");
    }
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (isPaused)
            DisablePlayerControl();
        else
            EnablePlayerControl();
        UIManager.Instance.ShowPlayerStatus(isPaused);
    }
    public void OnGhostCaptured()
    {
        capturedGhostCount++;

        if (capturedGhostCount == thresholdToStartSpawn)
        {
            Debug.Log("🔥 Enough ghosts captured! Start spawning from GhostPoints!");
            enemySpawner.BeginSpawnFromGhostPoints();
        }
    }
}
