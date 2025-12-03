using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("Type Gather Control")]
    public float sortSpeed = 3f;
    [Header("Game Stats")]
    public Dictionary<GhostType, int> capturedGhosts = new();
    private bool isGameOver = false;
    [SerializeField] private List<GhostData> allGhostData;
    private Dictionary<GhostType, GhostData> ghostDataDict = new();
    public int capturedGhostCount = 0;
    [SerializeField] private int thresholdToStartSpawn = 10;

    [SerializeField] private EnemySpawner enemySpawner;
    private bool isPaused = false;
    public Dictionary<RewardData, int> rewardCounts = new();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
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
    void Start()
    {
      
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        enemySpawner = FindAnyObjectByType<EnemySpawner>();

        if (enemySpawner != null)
            Debug.Log($"🔍 EnemySpawner refreshed: {enemySpawner.name}");
        else
            Debug.Log("⚠ EnemySpawner NOT found in this scene");
    }
    public void RegisterGhostCapture(GhostType type)
    {
        capturedGhosts[type]++;
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


        isGameOver = true;
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
            enemySpawner.BeginSpawnFromGhostPoints();
        }
    }
    public void AddRewardCount(RewardData reward)
    {
        if (!rewardCounts.ContainsKey(reward))
            rewardCounts[reward] = 0;

        rewardCounts[reward]++;

        UIManager.Instance.UpdateRewardIcons();
    }
    public void InitializeRewardCounts()
    {
        if (RewardManager.Instance == null) return;

        foreach (var reward in RewardManager.Instance.allRewards)
        {
            if (!rewardCounts.ContainsKey(reward))
                rewardCounts.Add(reward, 0);
        }
    }
}
