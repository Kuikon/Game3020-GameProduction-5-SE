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

    private void Awake()
    {
        Instance = this;
        if (luckyButton != null)
            luckyButton.gameObject.SetActive(false);
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
}
